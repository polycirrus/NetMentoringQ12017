using System;

namespace Shared
{
    [Serializable]
    public class UpdateConfigurationCommand
    {
        public ScannerConfiguration Configuration { get; set; }
    }
}
