using System;

namespace NetFlowLibrary.Types
{
    /// <summary>
    /// Структура записи NetFlow v5 в человеко-понятном варианте
    /// </summary>
    public class NetFlowTable
    {
        /// <summary>
        /// Адрес источника
        /// </summary>
        public string srcaddr;

        /// <summary>
        /// Адрес назначения
        /// </summary>
        public string dstaddr;

        /// <summary>
        /// 
        /// </summary>
        public string nexthop;

        /// <summary>
        /// 
        /// </summary>
        public int packetcount;

        /// <summary>
        /// 
        /// </summary>
        public int bytecount;

        /// <summary>
        /// 
        /// </summary>
        public long first;

        /// <summary>
        /// 
        /// </summary>
        public long last;

        /// <summary>
        /// 
        /// </summary>
        public int srcport;

        /// <summary>
        /// 
        /// </summary>
        public int dstport;

        /// <summary>
        /// 
        /// </summary>
        public short protocol;

        /// <summary>
        /// 
        /// </summary>
        public DateTime datetime;
    }

    /*
     * 0-1	version	NetFlow export format version number
     * 2-3	count	Number of flows exported in this packet (1-30)
     * 4-7	sys_uptime	Current time in milliseconds since the export device booted
     * 8-11	unix_secs	Current count of seconds since 0000 UTC 1970
     * 12-15	unix_nsecs	Residual nanoseconds since 0000 UTC 1970
     * 16-19	flow_sequence	Sequence counter of total flows seen
     * 20	engine_type	Type of flow-switching engine
     * 21	engine_id	Slot number of the flow-switching engine
     * 22-23	sampling_interval	First two bits hold the sampling mode; remaining 14 bits hold value of sampling interval
     */
    public class HeaderNetFlow
    {
        public uint FromHost;
        public byte Version = 5;
        public ushort Count;
        public uint Sys_uptime;
        public uint Unix_secs;
        public uint Unix_nsecs;
        public uint Flow_sequence;
        public byte Engine_type;
        public byte Engine_id;
        public ushort Sampling_interval;
    }

    /*  
     *  0-3	srcaddr Source IP address
     *  4-7	dstaddr Destination IP address
     *  8-11	nexthop IP address of next hop router
     *  12-13	input SNMP index of input interface
     *  14-15	output SNMP index of output interface
     *  16-19	dPkts Packets in the flow
     *  20-23	dOctets Total number of Layer 3 bytes in the packets of the flow
     *  24-27	first SysUptime at start of flow
     *  28-31	last SysUptime at the time the last packet of the flow was received
     *  32-33	srcport TCP/UDP source port number or equivalent
     *  34-35	dstport TCP/UDP destination port number or equivalent
     *  36	pad1 Unused(zero) bytes
     *  37	tcp_flags Cumulative OR of TCP flags
     *  38	prot IP protocol type(for example, TCP = 6; UDP = 17)
     *  39	tos IP type of service(ToS)
     *  40-41	src_as Autonomous system number of the source, either origin or peer
     *  42-43	dst_as Autonomous system number of the destination, either origin or peer
     *  44	src_mask Source address prefix mask bits
     *  45	dst_mask Destination address prefix mask bits
     *  46-47	pad2 Unused(zero) bytes*/
    public class RowNetFlow
    {
        public uint srcaddr;
        public uint dstaddr;
        public uint nexthop;
        public ushort inputSNMP;
        public ushort outputSNMP;
        public uint dPkts;
        public uint dOctets;
        public uint first;
        public uint last;
        public uint srcport;
        public uint dstport;
        public byte pad1;
        public byte tcp_flags;
        public byte protIP;
        public byte tosIP;
        public uint src_as;
        public uint dst_as;
        public byte src_mask;
        public byte dst_mask;
        public uint pad2;
    }


}
