using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerStateManagement;

namespace Tests
{
    [TestClass]
    public class NativePowerStateManagerTests
    {
        [TestMethod]
        public void GetLastSleepTime()
        {
            Console.WriteLine(NativePowerStateManager.GetLastSleepTime());
        }

        [TestMethod]
        public void GetLastWakeTime()
        {
            Console.WriteLine(NativePowerStateManager.GetLastWakeTime());
        }

        [TestMethod]
        public void GetBatteryState()
        {
            var state = NativePowerStateManager.GetBatteryState();
            Console.WriteLine(state.AcOnLine);
            Console.WriteLine(state.BatteryPresent);
            Console.WriteLine(state.RemainingCapacity);
        }

        [TestMethod]
        public void GetPowerInformation()
        {
            var info = NativePowerStateManager.GetPowerInformation();
            Console.WriteLine(info.CoolingMode);
            Console.WriteLine(info.Idleness);
            Console.WriteLine(info.MaxIdlenessAllowed);
            Console.WriteLine(info.TimeRemaining);
        }

        [TestMethod]
        public void ReserveHibernationFile()
        {
            NativePowerStateManager.ReserveHibernationFile();
        }

        [TestMethod]
        public void RemoveHibernationFile()
        {
            NativePowerStateManager.RemoveHibernationFile();
        }

        [TestMethod]
        public void Hibernate()
        {
            NativePowerStateManager.Hibernate();
        }

        [TestMethod]
        public void Suspend()
        {
            NativePowerStateManager.Suspend();
        }
    }
}
