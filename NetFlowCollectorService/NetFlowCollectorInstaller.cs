using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace NetFlowCollectorService
{
    [RunInstaller(true)]
    public partial class NetFlowCollectorInstaller : System.Configuration.Install.Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;
        public NetFlowCollectorInstaller()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "NetFlowCollector";
            serviceInstaller.Description = "Служба сбора NetFlow v5 пакетов и выгрузка в базу PostGres";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
