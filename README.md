### Описание
Задача собирать NetFlow пакеты от маршрутизатора Cisco и добавления в базу Postgresql для последующего анализа траффика и выявления инцедентов. Сервис поддерживает тип потока Netflow v5.

### Поля NetFlow v5
Краткое описание полей, которые содержатся в  NetFlow протоколе версии 5.

|Наименование|Описание|
|------|------|
| srcaddr | IP источника данных |
|dstaddr|IP приемника данных|
|nexthop|IP следующего сетевого устройства (маршрутизатора), через которых будут пересылаться пакеты данных|
|input|Входной интерфейс|
|output|Выходной интерфейс|
|dPkts|Количество пакетов в потоке|
|dOctets|Количество байт в потоке|
|first|Время начала потока в системе SysUptime|
|last|Время SysUptime, когда последний пакет потока был получен|
|scrport|Номер порта источника данных (4-го уровня сетевой модели)|
|dstport|Номер порта приемника данных (4-го уровня сетевой модели)|
|pad1|неиспользуемый байт|
|tcp_flags|TCP флаги|
|prot|Протокол 4-го уровня (например, 6=TCP, 17=UDP и т.п.)|
|tos|Тип сервиса IP протокола|
|scr_as|Номер автономной системы источника данных|
|dst_as|Номер автономной системы приемника данных|
|scr_mask|Маска адреса источника данных|
|dst_mask|Маска адреса приемника данных|
|pad2|неиспользуемый байт|

### Необходимая таблица

```sql
create table if not exists "NetFlowData"
 (
 	srcaddr cidr not null,
 	dstaddr cidr not null,
 	nexthop cidr not null,
 	packetcount integer not null
 		constraint NetFlowData_dpkts_check
 			check (packetcount > 0),
 	bytecount integer not null
 		constraint NetFlowData_doctets_check
 			check (bytecount > 0),
 	first bigint not null,
 	last bigint not null,
 	srcport integer not null
 		constraint NetFlowData_srcport_check
 			check (srcport >= 0),
 	dstport integer not null
 		constraint NetFlowData_dstport_check
 			check (dstport >= 0),
 	protocol smallint not null,
 	datetime timestamp not null
 );
comment on column "NetFlowData".srcaddr is 'Source IP address';
comment on column "NetFlowData".dstaddr is 'Destination IP address';
comment on column "NetFlowData".nexthop is 'IP address of next hop router';
comment on column "NetFlowData".packetcount is 'Packets in the flow';
comment on column "NetFlowData".bytecount is 'Total number of Layer 3 bytes in the packets of the flow';
comment on column "NetFlowData".first is 'SysUptime at start of flow';
comment on column "NetFlowData".last is 'SysUptime at the time the last packet of the flow was received';
comment on column "NetFlowData".srcport is 'TCP/UDP source port number or equivalent';
comment on column "NetFlowData".dstport is 'TCP/UDP destination port number or equivalent';
comment on column "NetFlowData".protocol is 'IP protocol type (for example, ICMP=1, TCP=6, Telnet=14, UDP=17)';
comment on column "NetFlowData".datetime is 'Time Add';
create index NetFlowData_index_srcport
	on "NetFlowData" (srcport);
create index NetFlowData_index_dstport
	on "NetFlowData" (dstport);
create index NetFlowData_index_datetime_srcaddr
	on "NetFlowData" (datetime, srcaddr);
create index NetFlowData_index_datetime_dstaddr
	on "NetFlowData" (datetime, dstaddr);
```
### На стороне маршрутизатора выполнить настройки
Необходимо выбрать интерфейсы, на которых мы хотим организовать сбор статистики и направление трафика, (на вход, на выход или оба).
```sh
R1(conf)# interface FastEthernet 0/1
R1(config-if)# ip flow ingress
R1(config-if)# ip flow egress
```
Также необходимо указать версию протокола и адресс и порт коллектора (где будет запущена служба)
```
R1(config)# ip flow-export destination 192.168.0.100 999
R1(config)# ip flow-export version 5
```
License
----
The MIT License.
