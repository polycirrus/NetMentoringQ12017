using System;
using System.Runtime.InteropServices;

namespace PowerStateManagement
{
    [ComVisible(true)]
    [Guid("5270BB6F-2E6B-43DA-AEC1-E160E69C58D2")]
    [ClassInterface(ClassInterfaceType.None)]
    public class PowerStateManager : IPowerStateManager
    {
        public int GetLastSleepTime()
        {
            var result = NativePowerStateManager.GetLastSleepTime();

            try
            {
                return (int) result;
            }
            catch (OverflowException)
            {
                return int.MaxValue;
            }
        }

        public int GetLastWakeTime()
        {
            var result = NativePowerStateManager.GetLastWakeTime();

            try
            {
                return (int)result;
            }
            catch (OverflowException)
            {
                return int.MaxValue;
            }
        }

        public SystemBatteryState GetBatteryState()
        {
            return NativePowerStateManager.GetBatteryState();
        }

        public SystemPowerInformation GetPowerInformation()
        {
            return NativePowerStateManager.GetPowerInformation();
        }

        public void ReserveHibernationFile()
        {
            NativePowerStateManager.ReserveHibernationFile();
        }

        public void RemoveHibernationFile()
        {
            NativePowerStateManager.RemoveHibernationFile();
        }

        public void Hibernate()
        {
            NativePowerStateManager.Hibernate();
        }

        public void Suspend()
        {
            NativePowerStateManager.Suspend();
        }

        public int TestMethod()
        {
            return 3456;
        }
    }
}
