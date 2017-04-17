using System;
using System.Runtime.InteropServices;

namespace PowerStateManager
{
    internal static class NativeAPI
    {
        public static readonly int STATUS_SUCCESS = 0;

        public enum POWER_INFORMATION_LEVEL
        {
            LastSleepTime = 15,
            LastWakeTime = 14,
            SystemBatteryState = 5,
            SystemPowerInformation = 12,
            SystemReserveHiberFile = 10
        }

        public struct SYSTEM_BATTERY_STATE
        {
            public byte AcOnLine;
            public byte BatteryPresent;
            public byte Charging;
            public byte Discharging;
            public byte Spare1;
            public byte Spare2;
            public byte Spare3;
            public byte Spare4;
            public UInt32 MaxCapacity;
            public UInt32 RemainingCapacity;
            public Int32 Rate;
            public UInt32 EstimatedTime;
            public UInt32 DefaultAlert1;
            public UInt32 DefaultAlert2;
        }

        public struct SYSTEM_POWER_INFORMATION
        {
            public UInt32 MaxIdlenessAllowed;
            public UInt32 Idleness;
            public UInt32 TimeRemaining;
            public byte CoolingMode;
        }

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern uint CallNtPowerInformation(int InformationLevel, IntPtr lpInputBuffer,
            uint nInputBufferSize, IntPtr lpOutputBuffer, uint nOutputBufferSize);

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern uint CallNtPowerInformation(POWER_INFORMATION_LEVEL InformationLevel, IntPtr lpInputBuffer,
          uint nInputBufferSize, [Out] ulong[] lpOutputBuffer, uint nOutputBufferSize);

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern uint CallNtPowerInformation(POWER_INFORMATION_LEVEL InformationLevel, IntPtr lpInputBuffer,
          uint nInputBufferSize, [Out] SYSTEM_BATTERY_STATE[] lpOutputBuffer, uint nOutputBufferSize);

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern uint CallNtPowerInformation(POWER_INFORMATION_LEVEL InformationLevel, IntPtr lpInputBuffer,
          uint nInputBufferSize, [Out] SYSTEM_POWER_INFORMATION[] lpOutputBuffer, uint nOutputBufferSize);

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern uint CallNtPowerInformation(POWER_INFORMATION_LEVEL InformationLevel, [In] bool[] lpInputBuffer,
          uint nInputBufferSize, IntPtr lpOutputBuffer, uint nOutputBufferSize);

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
    }
}
