using System;

namespace Shared
{
    [Serializable]
    public class ScannerConfiguration
    {
        public TimeSpan ScanTimeout { get; set; }
    }
}