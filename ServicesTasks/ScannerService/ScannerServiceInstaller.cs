using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ScannerService
{
    [RunInstaller(true)]
    public class ScannerServiceInstaller : Installer
    {
        public ScannerServiceInstaller()
        {
            var serviceInstaller = new ServiceInstaller();
            serviceInstaller.ServiceName = "ScannerService";
            serviceInstaller.DisplayName = "Scanner Service";
            serviceInstaller.DelayedAutoStart = false;
            serviceInstaller.Description = "Monitors the My Pictures folder for images from a scanner and composes them into documents.";
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