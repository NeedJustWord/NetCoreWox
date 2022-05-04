using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Wox.Infrastructure.UserSettings;

namespace Wox.Infrastructure.Exception
{
    public class ExceptionFormatter
    {
        private static string _systemLanguage;
        private static string _woxLanguage;

        public static void Initialize(string systemLanguage, string woxLanguage)
        {
            _systemLanguage = systemLanguage;
            _woxLanguage = woxLanguage;
        }

        public static string FormattedException(System.Exception ex)
        {
            return FormattedAllExceptions(ex).ToString();
        }

        private static StringBuilder FormattedAllExceptions(System.Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Exception begin --------------------");
            int index = 1;
            FormattedSingleException(ex, sb, 1);
            ex = ex.InnerException;
            while (ex != null)
            {
                sb.Append(Indent(index));
                sb.Append("InnerException ");
                sb.Append(index);
                sb.AppendLine(": ------------------------------------------------------------");
                index++;
                FormattedSingleException(ex, sb, index);
                ex = ex.InnerException;
            }

            sb.AppendLine("Exception end ------------------------------------------------------------");
            sb.AppendLine();
            return sb;
        }

        private static string Indent(int indentLevel)
        {
            return new string(' ', indentLevel * 2);
        }

        private static void FormattedSingleException(System.Exception ex, StringBuilder sb, int indentLevel)
        {
            sb.Append(Indent(indentLevel));
            sb.Append(ex.GetType().FullName);
            sb.Append(": ");
            sb.AppendLine(ex.Message);
            sb.Append(Indent(indentLevel));
            sb.Append("HResult: ");
            sb.AppendLine(ex.HResult.ToString());
            foreach (object key in ex.Data.Keys)
            {
                object value = ex.Data[key];
                sb.Append(Indent(indentLevel));
                sb.Append("Data: <");
                sb.Append(key);
                sb.Append("> -> <");
                sb.Append(value);
                sb.AppendLine(">");
            }

            if (ex.Source != null)
            {
                sb.Append(Indent(indentLevel));
                sb.Append("Source: ");
                sb.AppendLine(ex.Source);
            }
            if (ex.TargetSite != null)
            {
                sb.Append(Indent(indentLevel));
                sb.Append("TargetAssembly: ");
                sb.AppendLine(ex.TargetSite.Module.Assembly.ToString());
                sb.Append(Indent(indentLevel));
                sb.Append("TargetModule: ");
                sb.AppendLine(ex.TargetSite.Module.ToString());
                sb.Append(Indent(indentLevel));
                sb.Append("TargetSite: ");
                sb.AppendLine(ex.TargetSite.ToString());
            }
            sb.Append(Indent(indentLevel));
            sb.AppendLine("StackTrace: --------------------");
            sb.AppendLine(ex.StackTrace);
        }

        public static StringBuilder RuntimeInfoFull()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(RuntimeInfo());
            sb.AppendLine(SDKInfo());
            sb.Append(AssemblyInfo());
            return sb;
        }

        private static string AssemblyInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("## Assemblies - " + AppDomain.CurrentDomain.FriendlyName);
            sb.AppendLine();
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies().OrderBy(o => o.GlobalAssemblyCache ? 50 : 0))
            {
                sb.Append("* ");
                sb.Append(ass.FullName);
                sb.Append(" (");

                if (ass.IsDynamic)
                {
                    sb.Append("dynamic assembly doesn't has location");
                }
                else if (string.IsNullOrEmpty(ass.Location))
                {
                    sb.Append("location is null or empty");
                }
                else
                {
                    sb.Append(ass.Location);
                }
                sb.AppendLine(")");
            }
            return sb.ToString();
        }

        public static string RuntimeInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("## Runtime Info");
            sb.AppendLine($"* Command Line: {Environment.CommandLine}");
            sb.AppendLine($"* Portable Mode: {DataLocation.PortableDataLocationInUse()}");
            sb.AppendLine($"* Timestamp: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
            sb.AppendLine($"* Wox version: {Constant.Version}");
            sb.AppendLine($"* OS Version: {Environment.OSVersion.VersionString}");
            sb.AppendLine($"* x64 OS: {Environment.Is64BitOperatingSystem}");
            sb.AppendLine($"* x64 Process: {Environment.Is64BitProcess}");
            sb.AppendLine($"* System Language: {_systemLanguage}");
            sb.AppendLine($"* Wox Language: {_woxLanguage}");
            sb.AppendLine($"* CLR Version: {Environment.Version}");
            sb.AppendLine($"* Installed .NET Framework: ");
            foreach (var result in GetFrameworkVersionFromRegistry())
            {
                sb.Append("   * ");
                sb.AppendLine(result);
            }

            return sb.ToString();
        }

        public static string SDKInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("## SDK Info");
            sb.AppendLine($"* Python Path: {Constant.PythonPath}");
            sb.AppendLine($"* Everything SDK Path: {Constant.EverythingSDKPath}");
            return sb.ToString();
        }

        public static string ExceptionWithRuntimeInfo(System.Exception ex, string id)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Error id: ");
            sb.AppendLine(id);
            var formatted = FormattedAllExceptions(ex);
            sb.Append(formatted);
            var info = RuntimeInfoFull();
            sb.Append(info);

            return sb.ToString();
        }

        // http://msdn.microsoft.com/en-us/library/hh925568%28v=vs.110%29.aspx
        private static List<string> GetFrameworkVersionFromRegistry()
        {
            try
            {
                var result = new List<string>();
                using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                {
                    foreach (var versionKeyName in ndpKey.GetSubKeyNames())
                    {
                        if (versionKeyName == "v4") continue;

                        if (versionKeyName.StartsWith("v"))
                        {
                            RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                            var name = (string)versionKey.GetValue("Version", "");
                            var sp = versionKey.GetValue("SP", "").ToString();
                            var install = versionKey.GetValue("Install", "").ToString();
                            if (install != "")
                            {
                                if (sp != "" && install == "1")
                                    result.Add($"{versionKeyName} {name} SP{sp}");
                                else
                                    result.Add($"{versionKeyName} {name}");
                            }

                            if (name != "") continue;

                            foreach (var subKeyName in versionKey.GetSubKeyNames())
                            {
                                RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                                name = (string)subKey.GetValue("Version", "");
                                if (name != "")
                                    sp = subKey.GetValue("SP", "").ToString();
                                install = subKey.GetValue("Install", "").ToString();
                                if (install != "")
                                {
                                    if (sp != "" && install == "1")
                                        result.Add($"{versionKeyName} {subKeyName} {name} SP{sp}");
                                    else if (install == "1")
                                        result.Add($"{versionKeyName} {subKeyName} {name}");
                                }
                            }
                        }
                    }
                }
                using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                {
                    int releaseKey = (int)ndpKey.GetValue("Release");
                    {
                        if (releaseKey >= 528040)
                            result.Add("v4.8 or later");
                        else if (releaseKey >= 461808)
                            result.Add("v4.7.2");
                        else if (releaseKey >= 461308)
                            result.Add("v4.7.1");
                        else if (releaseKey >= 460798)
                            result.Add("v4.7");
                        else if (releaseKey >= 394802)
                            result.Add("v4.6.2");
                        else if (releaseKey >= 394254)
                            result.Add("v4.6.1");
                        else if (releaseKey >= 393295)
                            result.Add("v4.6");
                        else if (releaseKey >= 379893)
                            result.Add("v4.5.2");
                        else if (releaseKey >= 378675)
                            result.Add("v4.5.1");
                        else if (releaseKey >= 378389)
                            result.Add("v4.5");
                    }
                }
                return result;
            }
            catch (System.Exception)
            {
                return new List<string>();
            }
        }
    }
}
