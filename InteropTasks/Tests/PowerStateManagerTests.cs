using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerStateManager;

namespace Tests
{
    [TestClass]
    public class PowerStateManagerTests
    {
        [TestMethod]
        public void GetLastSleepTime()
        {
            Console.WriteLine(PowerStateManager.PowerStateManager.GetLastSleepTime());
        }

        [TestMethod]
        public void GetLastWakeTime()
        {
            Console.WriteLine(PowerStateManager.PowerStateManager.GetLastWakeTime());
        }

        [TestMethod]
        public void GetBatteryState()
        {
            var state = PowerStateManager.PowerStateManager.GetBatteryState();
            Console.WriteLine(state.AcOnLine);
            Console.WriteLine(state.BatteryPresent);
            Console.WriteLine(state.RemainingCapacity);
        }

        [TestMethod]
        public void GetPowerInformation()
        {
            var info = PowerStateManager.PowerStateManager.GetPowerInformation();
            Console.WriteLine(info.CoolingMode);
            Console.WriteLine(info.Idleness);
            Console.WriteLine(info.MaxIdlenessAllowed);
            Console.WriteLine(info.TimeRemaining);
        }

        [TestMethod]
        public void ReserveHibernationFile()
        {
            PowerStateManager.PowerStateManager.ReserveHibernationFile();
        }

        [TestMethod]
        public void RemoveHibernationFile()
        {
            PowerStateManager.PowerStateManager.RemoveHibernationFile();
        }
    }
}
