using System.IO;
using Wox.Infrastructure.UserSettings;

namespace Wox.Infrastructure.Storage
{
    public class WoxJsonStorage<T> : JsonStrorage<T> where T : new()
    {
        public WoxJsonStorage()
        {
            var directoryPath = Path.Combine(DataLocation.DataDirectory(), DirectoryName);
            Helper.ValidateDirectory(directoryPath);

            var filename = typeof(T).Name;
            FilePath = Path.Combine(directoryPath, $"{filename}{FileSuffix}");
        }
    }
}
