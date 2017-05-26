using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace MasterService
{
    [RunInstaller(true)]
    public class MasterServiceInstaller : Installer
    {
        public MasterServiceInstaller()
        {
            var serviceInstaller = new ServiceInstaller();
            serviceInstaller.ServiceName = "MasterService";
            serviceInstaller.DisplayName = "Master Service";
            serviceInstaller.DelayedAutoStart = false;
            serviceInstaller.Description = "Manages scanner services via MSMQ.";
            serviceInstaller.StartType = ServiceStartMode.Manual;

            var serviceProcessInstaller = new ServiceProcessInstaller();
            serviceProcessInstaller.Account = ServiceAccount.NetworkService;
            //serviceProcessInstaller.Username = "MINSK\\Aliaksandr_Zhytnitsk";
            //serviceProcessInstaller.Password = "";

            Installers.Add(serviceInstaller);
            Installers.Add(serviceProcessInstaller);
        }
    }
}
