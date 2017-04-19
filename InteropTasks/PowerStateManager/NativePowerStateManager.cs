using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

using static PowerStateManagement.NativeAPI;

namespace PowerStateManagement
{
    public static class NativePowerStateManager
    {
        public static ulong GetLastSleepTime()
        {
            return CallNtPowerInformationUlong(POWER_INFORMATION_LEVEL.LastSleepTime);
        }

        public static ulong GetLastWakeTime()
        {
            return CallNtPowerInformationUlong(POWER_INFORMATION_LEVEL.LastWakeTime);
        }

        public static SystemBatteryState GetBatteryState()
        {
            var outputBuffer = new SYSTEM_BATTERY_STATE[1];

            var result = CallNtPowerInformation(POWER_INFORMATION_LEVEL.SystemBatteryState, IntPtr.Zero, 0,
                outputBuffer, (uint)Marshal.SizeOf<SYSTEM_BATTERY_STATE>());
            if (result != STATUS_SUCCESS)
                throw new Win32Exception();

            var nativeState = outputBuffer[0];
            return Map<SYSTEM_BATTERY_STATE, SystemBatteryState>(nativeState);
        }

        public static SystemPowerInformation GetPowerInformation()
        {
            var outputBuffer = new SYSTEM_POWER_INFORMATION[1];

            var result = CallNtPowerInformation(POWER_INFORMATION_LEVEL.SystemPowerInformation, IntPtr.Zero, 0,
                outputBuffer, (uint)Marshal.SizeOf<SYSTEM_POWER_INFORMATION>());
            if (result != STATUS_SUCCESS)
                throw new Win32Exception();

            var nativeState = outputBuffer[0];
            return Map<SYSTEM_POWER_INFORMATION, SystemPowerInformation>(nativeState);
        }

        public static void ReserveHibernationFile()
        {
            SetHibernationFileState(true);
        }

        public static void RemoveHibernationFile()
        {
            SetHibernationFileState(false);
        }

        public static void Hibernate()
        {
            SetSuspendState(true);
        }

        public static void Suspend()
        {
            SetSuspendState(false);
        }

        private static void SetSuspendState(bool hibernate)
        {
            if (!NativeAPI.SetSuspendState(hibernate, false, false))
                throw new Win32Exception();
        }

        private static void SetHibernationFileState(bool value)
        {
            var inputBuffer = new[] { value };

            var result = CallNtPowerInformation(POWER_INFORMATION_LEVEL.SystemReserveHiberFile, inputBuffer, 
                (uint)Marshal.SizeOf<bool>(), IntPtr.Zero, 0);
            if (result != STATUS_SUCCESS)
                throw new Win32Exception();
        }

        private static ulong CallNtPowerInformationUlong(POWER_INFORMATION_LEVEL informationLevel)
        {
            var outputBuffer = new ulong[1];

            var result = CallNtPowerInformation(informationLevel, IntPtr.Zero, 0, outputBuffer, (uint)Marshal.SizeOf<ulong>());
            if (result != STATUS_SUCCESS)
                throw new Win32Exception();

            return outputBuffer[0];
        }
        
        private static TDest Map<TSource, TDest>(TSource source)
            where TDest : new()
        {
            var result = new TDest();

            var sourceFields = typeof(TSource).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var destFields = typeof(TDest).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            foreach (var destField in destFields)
            {
                var sourceField = sourceFields.FirstOrDefault(field => field.Name == destField.Name && destField.FieldType.IsAssignableFrom(field.FieldType));
                if (sourceField != null)
                    destField.SetValue(result, sourceField.GetValue(source));
            }

            return result;
        }
    }
}
