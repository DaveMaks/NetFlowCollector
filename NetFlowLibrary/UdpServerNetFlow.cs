using NetFlowLibrary.Types;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetFlowLibrary
{
    /// <summary>
    /// Класс для создания сервера прослушивания UDP 
    /// </summary>
    /// <example>
    ///  ...
    ///  UdpServerNetFlow udpServerNetFlow = new UdpServerNetFlow(9999);
    ///  udpServerNetFlow.OnNewPackage += UdpServerNetFlow_OnNewPackage;
    ///  udpServerNetFlow.Start();
    ///  ...
    /// 
    /// private static void UdpServerNetFlow_OnNewPackage(object state)
    ///    {
    ///        byte[] newPackageEvent = (byte[])state;
    ///        foreach (byte x in newPackageEvent)
    ///        {
    ///            Console.Write(x);
    ///        }
    ///        Console.WriteLine("---------------------");
    ///    }
    /// </example>
    public class UdpServerNetFlow
    {
        public bool IsStop = false;
        public int PortServer;
        public event SendOrPostCallback OnNewPackage;
        private Thread ServerUDP;
        private SynchronizationContext uiContext;

        public UdpServerNetFlow(int port)
        {
            this.PortServer = port;
            uiContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Запустить процесс прослушивания порта
        /// </summary>
        public void Start()
        {
            ServerUDP = new Thread(StartThread);
            ServerUDP.Name = "UDPServer_NetFlow";
            ServerUDP.Start();
        }

        /// <summary>
        /// Остановить процесс прослушивания порта
        /// </summary>
        public void Stop()
        {
            IsStop = true;
            if (ServerUDP.IsAlive)
                ServerUDP.Abort();
        }

        private void StartThread()
        {
            UdpClient receivingUdpClient = new UdpClient(PortServer);
            IPEndPoint RemoteIpEndPoint = null;
            try
            {
                while (!IsStop)
                {
                    try
                    {
                        byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                        byte[] ip = RemoteIpEndPoint.Address.GetAddressBytes();
                        if (receiveBytes[1] != 0x05) continue;
                        HeaderNetFlow header = this.ParsingHeder(ref receiveBytes);
                        header.FromHost = (uint)(ip[3] & 0xFF | (ip[2] & 0xFF) << 8 | (ip[1] & 0xFF) << 16 | (ip[0] & 0xFF) << 24);//RawToUInt(ref ip, 0);
                        RowNetFlow[] rows = new RowNetFlow[header.Count];

                        for (int i = 0; i < header.Count; i++)
                        {
                            rows[i] = this.ParsingRow(ref receiveBytes, 24 + i * 48);
                        }

                        if (uiContext != null)
                        {
                            uiContext.Post(OnNewPackage, new NewPackageEvent(receiveBytes) { Header = header, Rows = rows });
                        }
                        else
                        {
                            OnNewPackage(new NewPackageEvent(receiveBytes) { Header = header, Rows = rows });
                        }
                    }
                    catch (ThreadAbortException exAbort)
                    {
                        Logs.Write(exAbort);
                    }
                    catch (Exception e)
                    {
                        Logs.Write(e);
                    }

                }
                receivingUdpClient.Close();
            }
            catch (Exception ex)
            {
                Logs.Write(ex);
                receivingUdpClient.Close();
            }
        }

        /// <summary>
        /// Парсинг заголовка пакета Netflow v5
        /// </summary>
        /// param name="Data" ссылка не массив байт пакета
        private HeaderNetFlow ParsingHeder(ref byte[] Data)
        {
            HeaderNetFlow ret = new HeaderNetFlow();
            ret.Version = 5;
            ret.Count = (ushort)((Data[3] & 0xFF) | (Data[2] & 0xFF) << 8);
            ret.Sys_uptime = RawToUInt(ref Data, 4);
            ret.Unix_secs = RawToUInt(ref Data, 8);
            ret.Unix_nsecs = RawToUInt(ref Data, 12);
            ret.Flow_sequence = RawToUInt(ref Data, 16);
            ret.Engine_type = Data[20];
            ret.Engine_id = Data[21];
            ret.Sampling_interval = (ushort)((Data[23] & 0xFF) | (Data[22] & 0xFF) << 8);
            return ret;
        }

        /// <summary>
        /// Пасинг записей из пакета 
        /// </summary>
        /// <param name="Data">ссылка не массив байт пакета</param>
        /// <param name="start">позиция откуда выбирать следущию запись</param>
        /// <returns>Объект записи</returns>
        private RowNetFlow ParsingRow(ref byte[] Data, int start)
        {
            RowNetFlow ret = new RowNetFlow();
            ret.srcaddr = RawToUInt(ref Data, start);// new byte[] { Data[start], Data[start + 1], Data[start + 2], Data[start + 3] }; //Data.Skip(start).Take(4).ToArray();
            ret.dstaddr = RawToUInt(ref Data, start + 4); //new byte[] { Data[start + 4], Data[start + 5], Data[start + 6], Data[start + 7] }; //Data.Skip(start + 4).Take(4).ToArray();
            ret.nexthop = RawToUInt(ref Data, start + 8); //new byte[] { Data[start + 8], Data[start + 9], Data[start + 10], Data[start + 11] }; ;
            ret.inputSNMP = (ushort)((Data[start + 13] & 0xFF) | (Data[start + 12] & 0xFF) << 8); ;
            ret.outputSNMP = (ushort)((Data[start + 15] & 0xFF) | (Data[start + 14] & 0xFF) << 8); ; ;
            ret.dPkts = RawToUInt(ref Data, start + 16);
            ret.dOctets = RawToUInt(ref Data, start + 20);
            ret.first = RawToUInt(ref Data, start + 24);
            ret.last = RawToUInt(ref Data, start + 28);
            ret.srcport = (ushort)((Data[start + 33] & 0xFF) | (Data[start + 32] & 0xFF) << 8);
            ret.dstport = (ushort)((Data[start + 35] & 0xFF) | (Data[start + 34] & 0xFF) << 8);
            ret.pad1 = Data[start + 36];
            ret.tcp_flags = Data[start + 37];
            ret.protIP = Data[start + 38];
            ret.tosIP = Data[start + 39];
            ret.src_as = (ushort)((Data[start + 41] & 0xFF) | (Data[start + 40] & 0xFF) << 8);
            ret.dst_as = (ushort)((Data[start + 43] & 0xFF) | (Data[start + 42] & 0xFF) << 8);
            ret.src_mask = Data[start + 44];
            ret.dst_mask = Data[start + 45];
            ret.pad2 = (ushort)((Data[start + 47] & 0xFF) | (Data[start + 46] & 0xFF) << 8);
            return ret;
        }

        /// <summary>
        /// Конверт байтов в Uint
        /// </summary>
        /// <param name="Data">ссылка на массив байт пакета</param>
        /// <param name="start">позиция откуда выбирать 3 байта</param>
        /// <returns></returns>
        private uint RawToUInt(ref byte[] Data, int start)
        {
            return (uint)(Data[start + 3] & 0xFF |
                (Data[start + 2] & 0xFF) << 8 |
            (Data[start + 1] & 0xFF) << 16 |
            (Data[start + 0] & 0xFF) << 24);
        }
    }

    /// <summary>
    /// Структура пакет Netflow v5
    /// </summary>
    public class NewPackageEvent : EventArgs
    {
        public byte[] src;
        public HeaderNetFlow Header;
        public RowNetFlow[] Rows;
        public NewPackageEvent(byte[] raw)
        {
            src = raw;
        }
        public NewPackageEvent()
        {
        }
    }


}
