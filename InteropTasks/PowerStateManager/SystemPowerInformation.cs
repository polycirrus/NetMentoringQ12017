using System.Runtime.InteropServices;

namespace PowerStateManagement
{
    [ComVisible(true)]
    public class SystemPowerInformation
    {
        public uint MaxIdlenessAllowed;
        public uint Idleness;
        public uint TimeRemaining;
        public byte CoolingMode;
    }
}