using NetFlowLibrary.Types;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace NetFlowLibrary
{
    /// <summary>
    /// Класс прямой работы с базой
    /// </summary>
    public class Database
    {
        private const string TableName = "NetFlowData";
        private string _StringConnect;
        private NpgsqlConnection _npgsqlConnection;
        public string _lastSQL = "";

        public Database(string stringconnect)
        {
            try
            {
                _StringConnect = stringconnect;
                _npgsqlConnection = new NpgsqlConnection(_StringConnect);
                _npgsqlConnection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка подключенния к БД :"+ ex.Message);
                //Console.WriteLine(ex.Message);
            }
        }
        public void Truncate()
        {
            if (_npgsqlConnection != null && _npgsqlConnection.State == ConnectionState.Open)
            {
                _lastSQL = $"TRUNCATE  \"{TableName}\";";
                NpgsqlCommand comm = new NpgsqlCommand(_lastSQL, _npgsqlConnection);
                comm.ExecuteNonQuery();
            }
        }
        public bool AddNewRow(NetFlowTable row)
        {
            int x = 0;
            if (_npgsqlConnection != null && _npgsqlConnection.State == ConnectionState.Open)
            {
                try
                {
                    string sql = $"INSERT INTO \"{TableName}\" (srcaddr, dstaddr, nexthop, packetcount, bytecount, first, last, srcport, dstport, protocol, datetime) " +
                    $"VALUES('{row.srcaddr}', '{row.dstaddr}', '{row.nexthop}', {row.packetcount}, {row.bytecount}, {row.first}, {row.last}, {row.srcport}, {row.dstport}, {row.protocol}, '{row.datetime.ToString("yyyy-MM-dd HH:mm:ss")}');";
                    _lastSQL = sql;
                    NpgsqlCommand comm = new NpgsqlCommand(sql, _npgsqlConnection);
                    x = comm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logs.Write(ex);
                    throw new Exception("Ошибка Выполнения SQL " + ex.Message);
                }
            }
            return (x > 0);
        }
        public int AddNewRow(List<NetFlowTable> rows)
        {
            int x = 0;
            if (_npgsqlConnection != null && _npgsqlConnection.State == ConnectionState.Open && rows.Count > 0)
            {
                try
                {
                    string sql = $"INSERT INTO \"{TableName}\" (srcaddr, dstaddr, nexthop, packetcount, bytecount, first, last, srcport, dstport, protocol, datetime) VALUES ";
                    string[] vs = new string[rows.Count];
                    int i = 0;
                    foreach (NetFlowTable row in rows)
                    {
                        vs[i++] = $"('{row.srcaddr}', '{row.dstaddr}', '{row.nexthop}', {row.packetcount}, {row.bytecount}, {row.first}, {row.last}, {row.srcport}, {row.dstport}, {row.protocol}, '{row.datetime.ToString("yyyy-MM-dd HH:mm:ss")}')";
                    }
                    this._lastSQL = sql + string.Join(",", vs);
                    NpgsqlCommand comm = new NpgsqlCommand(this._lastSQL, _npgsqlConnection);
                    x = comm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logs.Write(ex);
                    throw new Exception("Ошибка Выполнения SQL " + ex.Message);
                }
            }
            return x;
        }
        public int AddNewRow(RowNetFlow[] rows)
        {
            if (_npgsqlConnection != null && _npgsqlConnection.State == ConnectionState.Open && rows.Length > 0)
            {
                try
                {
                    string sql = $"INSERT INTO \"{TableName}\" (srcaddr, dstaddr, nexthop, packetcount, bytecount, first, last, srcport, dstport, protocol, datetime) VALUES \n";
                    string[] vs = new string[rows.Length];
                    int i = 0;
                    foreach (RowNetFlow row in rows)
                    {
                        vs[i++] = $"(int2inet({row.srcaddr}), int2inet({row.dstaddr}), int2inet({row.nexthop}), {row.dPkts}, {row.dOctets}, {row.first}, {row.last}, {row.srcport}, {row.dstport}, {row.protIP}, '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')";
                    }
                    this._lastSQL = sql + string.Join(",\n", vs);
                    NpgsqlCommand comm = new NpgsqlCommand(this._lastSQL, _npgsqlConnection);
                    return comm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logs.Write(ex);
                    throw new Exception("Ошибка Выполнения SQL " + ex.Message);
                }
            }
            return 0;
        }

        public List<T> SqlQuery<T>(string sql) where T : class, new()
        {
            List<T> ret = new List<T>();
            if (_npgsqlConnection != null && _npgsqlConnection.State == ConnectionState.Open && !string.IsNullOrWhiteSpace(sql))
            {
                _lastSQL = sql;
                try
                {
                    using (NpgsqlCommand comm = new NpgsqlCommand(_lastSQL, _npgsqlConnection))
                    {
                        using(var reader = comm.ExecuteReader())
                        {
                          
                            var ListProp = typeof(T).GetProperties(System.Reflection.BindingFlags.Public);
                            string[] listColl = null;
                            if (listColl == null)// ?.Length==0)
                            {
                                listColl = new string[reader.FieldCount];
                                for (int r = 0; r < reader.FieldCount; r++)
                                    listColl[r] = reader.GetName(r);
                            }
                            while (reader.Read())
                            {
                                T tmp = new T();
                                for (int p = 0; p < ListProp.Length; p++)
                                {
                                    int i = Array.IndexOf(listColl, ListProp[p].Name);
                                    ListProp[p].SetValue(tmp, (i >= 0 && !(reader.GetValue(i) is System.DBNull)) ? reader.GetValue(i) : null);
                                }
                                ret.Add(tmp);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {

                }
            }
            return ret;
        }

        public void Close()
        {
            if (_npgsqlConnection != null && _npgsqlConnection.State == ConnectionState.Open)
            {
                _npgsqlConnection.Close();
            }

            _npgsqlConnection?.Dispose();
        }

        ~Database()
        {
            if (_npgsqlConnection != null && _npgsqlConnection.State == ConnectionState.Open)
            {
                _npgsqlConnection.Close();
            }

            _npgsqlConnection?.Dispose();
        }

    }
}
