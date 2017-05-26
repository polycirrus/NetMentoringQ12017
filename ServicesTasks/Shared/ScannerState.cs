using System;

namespace Shared
{
    [Serializable]
    public class ScannerState
    {
        public ScannerStatus Status { get; set; }
        public ScannerConfiguration Configuration { get; set; }
    }
}
