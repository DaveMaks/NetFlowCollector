using NetFlowLibrary;
using NetFlowLibrary.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetFlowCollectorConsole
{
    /// <summary>
    /// Тестовый вывод пакетов Netflow пакетов в консоль из UDP сервера
    /// </summary>    
    class Program
    {
        static void Main(string[] args)
        {
            UdpServerNetFlow udpServerNetFlow = new UdpServerNetFlow(9999);
            udpServerNetFlow.OnNewPackage += UdpServerNetFlow_OnNewPackage;
            udpServerNetFlow.Start();
            Console.ReadKey();
        }

        /**
         * Новый пакет с UDP сервера
         */
        private static void UdpServerNetFlow_OnNewPackage(object state)
        {
            byte[] newPackageEvent = (byte[])state;
            foreach (byte x in newPackageEvent)
            {
                Console.Write(x);
            }
            Console.WriteLine("---------------------");
        }
    }
}
