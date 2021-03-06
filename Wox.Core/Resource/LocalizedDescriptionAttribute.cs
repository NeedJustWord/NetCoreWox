using System.ComponentModel;
using Wox.Core.Resource;

namespace Wox.Core
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly Internationalization _translator;
        private readonly string _resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey)
        {
            _translator = InternationalizationManager.Instance;
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string description = _translator.GetTranslation(_resourceKey);
                return string.IsNullOrWhiteSpace(description) ?
                    $"[[{_resourceKey}]]" : description;
            }
        }
    }
}
