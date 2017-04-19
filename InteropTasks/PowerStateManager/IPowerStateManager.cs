using System.Runtime.InteropServices;

namespace PowerStateManagement
{
    [ComVisible(true)]
    [Guid("5315DEE1-ED1F-4E92-B612-B3824CFDFC79")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IPowerStateManager
    {
        int GetLastSleepTime();

        int GetLastWakeTime();

        SystemBatteryState GetBatteryState();

        SystemPowerInformation GetPowerInformation();

        void ReserveHibernationFile();

        void RemoveHibernationFile();

        void Hibernate();

        void Suspend();

        int TestMethod();
    }
}