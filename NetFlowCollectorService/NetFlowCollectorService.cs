using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetFlowLibrary;

namespace NetFlowCollectorService
{
    /// <summary>
    /// Запуск коллектора Netflow пакетов как служба windows, 
    /// для корректной работы необходимо указать в Properties параметры подключения к базе Postgres,
    /// а также созать таблицу:
    /// create table if not exists "NetFlowData"
    /// (
    /// 	srcaddr cidr not null,
    /// 	dstaddr cidr not null,
    /// 	nexthop cidr not null,
    /// 	packetcount integer not null
    /// 		constraint NetFlowData_dpkts_check
    /// 			check (packetcount > 0),
    /// 	bytecount integer not null
    /// 		constraint NetFlowData_doctets_check
    /// 			check (bytecount > 0),
    /// 	first bigint not null,
    /// 	last bigint not null,
    /// 	srcport integer not null
    /// 		constraint NetFlowData_srcport_check
    /// 			check (srcport >= 0),
    /// 	dstport integer not null
    /// 		constraint NetFlowData_dstport_check
    /// 			check (dstport >= 0),
    /// 	protocol smallint not null,
    /// 	datetime timestamp not null
    /// );
    /// comment on column "NetFlowData".srcaddr is 'Source IP address';
    /// comment on column "NetFlowData".dstaddr is 'Destination IP address';
    /// comment on column "NetFlowData".nexthop is 'IP address of next hop router';
    /// comment on column "NetFlowData".packetcount is 'Packets in the flow';
    /// comment on column "NetFlowData".bytecount is 'Total number of Layer 3 bytes in the packets of the flow';
    /// comment on column "NetFlowData".first is 'SysUptime at start of flow';
    /// comment on column "NetFlowData".last is 'SysUptime at the time the last packet of the flow was received';
    /// comment on column "NetFlowData".srcport is 'TCP/UDP source port number or equivalent';
    /// comment on column "NetFlowData".dstport is 'TCP/UDP destination port number or equivalent';
    /// comment on column "NetFlowData".protocol is 'IP protocol type (for example, ICMP=1, TCP=6, Telnet=14, UDP=17)';
    /// comment on column "NetFlowData".datetime is 'Time Add';
    /// create index NetFlowData_index_srcport
    /// 	on "NetFlowData" (srcport);
    /// create index NetFlowData_index_dstport
    /// 	on "NetFlowData" (dstport);
    /// create index NetFlowData_index_datetime_srcaddr
    /// 	on "NetFlowData" (datetime, srcaddr);
    /// create index NetFlowData_index_datetime_dstaddr
    /// 	on "NetFlowData" (datetime, dstaddr);
    /// </summary>
    public partial class NetFlowCollectorService : ServiceBase
    {
        string conn_param;
        UdpServerNetFlow udpServerNetFlow;
        public NetFlowCollectorService()
        {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args)
        {
            try
            {
                conn_param = $"Server={Properties.Settings.Default.PGHost};Port={Properties.Settings.Default.PGPort};User Id={Properties.Settings.Default.PGLogin};Password={Properties.Settings.Default.PGPassword};Database={Properties.Settings.Default.PGDatabase};";
                udpServerNetFlow = new UdpServerNetFlow(Properties.Settings.Default.NetFlowUDPPort);
                udpServerNetFlow.OnNewPackage += UdpServerNetFlow_OnNewPackage;
                udpServerNetFlow.Start();
            }
            catch (Exception ex)
            {
                Logs.Write(ex);
                OnStop();
            }
        }
        /// <summary>
        /// Прилетел новый пакет, создаем отдельный процесс на его обработку и добавления в базу
        /// </summary>
        /// <param name="state"> объект NewPackageEvent </param>
        private void UdpServerNetFlow_OnNewPackage(object state)
        {
            Task task = new Task(InsertPacked, state);
            task.Start();
        }

        /// <summary>
        /// Выполняем процесс добавления записей из пакета Netflow в базу
        /// </summary>
        /// <param name="newPackage"></param>
        private void InsertPacked(object newPackage)
        {
            Database database = null;
            try
            {
                NewPackageEvent pack = (NewPackageEvent)newPackage;
                database = new Database(conn_param);
                database.AddNewRow(pack.Rows);
            }
            catch (Exception ex)
            {
                /// Складываем не выполненные SQL в отдельный файл для послейдущего анализа и ручного импорта, нечего не должно потерятся)
                if (!string.IsNullOrWhiteSpace(database?._lastSQL))
                {
                    Logs.Write("SQLNoInsert", "--" + DateTime.Now.ToString() + " " + ex.Message + Environment.NewLine + database._lastSQL + Environment.NewLine);
                }
            }
            finally{
                database.Close();
            }
        }

        protected override void OnStop()
        {
            if (udpServerNetFlow != null)
            {
                udpServerNetFlow.IsStop = true;
                Thread.Sleep(1000);
            }
        }
    }
}
