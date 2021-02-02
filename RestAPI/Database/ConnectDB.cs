using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Database
{
    public interface ISessionMethods
    {
        string warehouse { get; set; }
        SQLServer GetConnection();
        SQLServer GetConnectionSA();
    }
    public class SessionMethods : ISessionMethods
    {
        public string warehouse { get; set; }


        private readonly ConfigConnectDB config;
        public SessionMethods(ConfigConnectDB configConnectDB)
        {
            this.config = configConnectDB;

        }
        public SQLServer GetConnection()
        {
            return new SQLServer(config.server, config.database, config.Login, config.password);
        }
        public SQLServer GetConnectionSA()
        {
            return new SQLServer(config.serverSA, config.databaseSA, config.LoginSA, config.passwordSA);
        }
        public SQLServer GetConnectionSCE()
        {
            return new SQLServer(config.serverSCE, config.databaseSCE, config.LoginSCE, config.passwordSCE);
        }
    }
    public class SQLServer
    {
        public SqlConnection cn;
        public SqlCommand cm;
        private SqlDataAdapter da;
        private SqlTransaction tran;
        private String ConnectionString;

        public SQLServer(string server, string database, string userName, string password)
        {

            SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder();
            cn = new SqlConnection();
            cm = new SqlCommand(null, cn);
            da = new SqlDataAdapter(null, cn);

            cm.CommandTimeout = 0;
            cb.DataSource = server;
            cb.InitialCatalog = database;
            cb.UserID = userName;
            cb.Password = password;
            da.SelectCommand.CommandTimeout = 30000; //Set Sql Server Timeout
            ConnectionString = cb.ConnectionString;

        }

        public void Open()
        {
            try
            {
                if (cn.State == ConnectionState.Closed)
                {
                    cn.ConnectionString = ConnectionString;
                    cn.Open();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Message Error : " + ex.Message, ex);
            }
        }

        public void Close()
        {
            cn.Close();
        }

        public int Execute(string sql)
        {
            try
            {
                int rowExecute;
                Open();
                cm.CommandType = CommandType.Text;
                cm.CommandText = sql;
                rowExecute = cm.ExecuteNonQuery();
                return rowExecute;
            }
            catch (Exception ex)
            {
                throw new Exception("SQL Error : " + sql + "  <br />"
                                    + "Message Error : " + ex.Message, ex);
            }

        }

        public string ExecuteScalar(string sql)
        {
            try
            {
                Open();
                cm.CommandType = CommandType.Text;
                cm.CommandText = sql;
                return cm.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("SQL Error : " + sql + " <br />"
                                    + "Message Error : " + ex.Message, ex);
            }

        }
        public DataTable SelectDataTable(string sql)
        {

            DataTable dataTable = new DataTable();
            try
            {
                if (sql.Trim() == "")
                {
                    sql = "SELECT 'Not found SQL Command for select data from database' RESULT";
                }
                Open();
                da.SelectCommand.CommandText = sql;
                da.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception("SQL Error : " + sql + " <br />"
                                    + "Message Error : " + ex.Message, ex);
            }

            return dataTable;

        }
        public DataTable TranSelectDataTable(string sql)
        {

            DataTable dataTable = new DataTable();
            try
            {
                if (sql.Trim() == "")
                {
                    sql = "SELECT 'Not found SQL Command for select data from database' RESULT";
                }
                Open();
                da.SelectCommand.Transaction = cm.Transaction;
                da.SelectCommand.CommandText = sql;
                da.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception("SQL Error : " + sql + " <br />"
                                    + "Message Error : " + ex.Message, ex);
            }

            return dataTable;

        }
        public string TranExecuteScalar(string sql)
        {
            string excuteReturn = "";
            try
            {
                if (sql.Trim() == "")
                {
                    sql = "SELECT 'Not found SQL Command for select data from database' RESULT";
                }
                Open();
                cm.Transaction = cm.Transaction;
                cm.CommandText = sql;
                excuteReturn = cm.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("SQL Error : " + sql + " <br />"
                                    + "Message Error : " + ex.Message, ex);
            }

            return excuteReturn;

        }
        public void SetTransaction()
        {
            try
            {
                Open();
                tran = cn.BeginTransaction(IsolationLevel.ReadCommitted);
                cm.Transaction = tran;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CommitTransaction()
        {
            try
            {
                tran.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                tran.Rollback();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                    cn.Dispose();
                    cm.Dispose();
                    da.Dispose();
                    if (tran != null) tran.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetNCounter(string keyName, string userLogin)
        {
            int counter = 0;

            string q;
            q = " SELECT KEYNAME,KEYCOUNT FROM NCOUNTER";
            q += " WHERE KEYNAME='" + keyName + "'";
            DataTable dtNCounter = SelectDataTable(q);
            if (dtNCounter.Rows.Count > 0)
            {
                counter = Convert.ToInt32(dtNCounter.Rows[0]["KEYCOUNT"].ToString());
                q = "  UPDATE NCOUNTER";
                q += " SET KEYCOUNT=KEYCOUNT+1";
                q += " WHERE KEYNAME='" + keyName + "'";
                Execute(q);
                counter += 1;
            }
            else
            {
                q = " INSERT INTO NCOUNTER (KEYNAME, KEYCOUNT, ADDDATE, ADDWHO, EDITDATE, EDITWHO)";
                q += " VALUES (";
                q += " '" + keyName + "'";
                q += " ,1";
                q += " ,GETUTCDATE()";
                q += " ,'" + userLogin + "'";
                q += " ,GETUTCDATE() ";
                q += " ,'" + userLogin + "'";
                q += " )";
                Execute(q);
                counter = 1;
            }
            return counter;
        }

        public int TranGetNCounter(string keyName, string userLogin, string whLogin)
        {
            int counter = 0;

            string q;
            q = " SELECT KEYNAME,KEYCOUNT FROM SCPRD." + whLogin + ".NCOUNTER";
            q += " WHERE KEYNAME='" + keyName + "'";
            DataTable dtNCounter = TranSelectDataTable(q);
            if (dtNCounter.Rows.Count > 0)
            {
                counter = Convert.ToInt32(dtNCounter.Rows[0]["KEYCOUNT"].ToString());
                q = "  UPDATE SCPRD." + whLogin + ".NCOUNTER";
                q += " SET KEYCOUNT=KEYCOUNT+1";
                q += " WHERE KEYNAME='" + keyName + "'";
                Execute(q);
                counter += 1;
            }
            else
            {
                q = " INSERT INTO SCPRD." + whLogin + ".NCOUNTER (KEYNAME, KEYCOUNT, ADDDATE, ADDWHO, EDITDATE, EDITWHO)";
                q += " VALUES (";
                q += " '" + keyName + "'";
                q += " ,1";
                q += " ,GETUTCDATE()";
                q += " ,'" + userLogin + "'";
                q += " ,GETUTCDATE() ";
                q += " ,'" + userLogin + "'";
                q += " )";
                Execute(q);
                counter = 1;
            }
            return counter;
        }

        public Tuple<DataTable, string> ExecuteDataTableSP(string SQL, string ParameterName, DataTable ParameterValues, string returnmessage)
        {
            string _returnVaules = "";
            DataTable dt = new DataTable();
            try
            {
                Open();
                cm.CommandType = CommandType.StoredProcedure;
                cm.CommandText = SQL;

                // Parameter01 //
                SqlParameter p1 = new SqlParameter(ParameterName, SqlDbType.Structured);
                p1.Value = ParameterValues;
                p1.TypeName = "CONFIRMCHARGE";
                cm.Parameters.Add(p1);


                System.Data.IDbDataParameter _Procedur = cm.CreateParameter();
                _Procedur.ParameterName = returnmessage;
                _Procedur.Direction = System.Data.ParameterDirection.Output;
                _Procedur.DbType = System.Data.DbType.AnsiString;
                _Procedur.Size = 900;
                cm.Parameters.Add(_Procedur);

                da = new SqlDataAdapter(cm);
                da.Fill(dt);

                //cm.ExecuteScalar();
                _returnVaules = Convert.ToString(_Procedur.Value);
            }
            catch (Exception ex)
            {
                _returnVaules = ex.Message;
            }

            return Tuple.Create(dt, _returnVaules);
        }

        public string ExecuteSP(string ProcedureName, string Parameter01, string Parameter02, string Parameter03, string Parameter04, string Parameter05, string Parameter06, string Parameter07)
        {
            try
            {
                string _returnVaules = "";

                Open();
                cm.CommandType = CommandType.StoredProcedure;
                cm.CommandText = ProcedureName;
                /*
	                SET @input_facilityid = 'wmwhse5' >>>>>>>>>>>>>>>>>>>>>>>> from whesid in use
	                SET @input_owner = 'BFI' >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Select on screen
	                SET @input_billingdate = '20170216' >>>>>>>>>>>>>>>>>>>>>> Select on screen
	                SET @input_chargecode = 'RS,FREEZE,PACK,THAW,OTHER' >>>>>> Select on screen
	                SET @input_userid = 'SP' >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Userid for login
	                SET @inupt_linkedserver = 'BPCS.S651994B.bpusfmm612' >>>>> from config  -- LIBs temp
	                SET @inupt_linkedserver2 = 'BPCS.S651994B.bpfmm612'  >>>>> from config  -- LIBs true           
                 */

                // Parameter01 //
                SqlParameter p1 = new SqlParameter("@input_facilityid", SqlDbType.NVarChar, 40);
                p1.Value = Parameter01;
                cm.Parameters.Add(p1);
                // Parameter02 //
                SqlParameter p2 = new SqlParameter("@input_owner", SqlDbType.NVarChar, 40);
                p2.Value = Parameter02;
                cm.Parameters.Add(p2);
                // Parameter03 //
                SqlParameter p3 = new SqlParameter("@input_billingdate", SqlDbType.NVarChar, 8);
                p3.Value = Parameter03;
                cm.Parameters.Add(p3);
                // Parameter04 //
                SqlParameter p4 = new SqlParameter("@input_chargecode", SqlDbType.NVarChar, 2000);
                p4.Value = Parameter04;
                cm.Parameters.Add(p4);
                // Parameter05 //
                SqlParameter p5 = new SqlParameter("@input_userid", SqlDbType.NVarChar, 30);
                p5.Value = Parameter05;
                cm.Parameters.Add(p5);
                // Parameter06 //
                SqlParameter p6 = new SqlParameter("@inupt_linkedserver", SqlDbType.NVarChar, 50);
                p6.Value = Parameter06;
                cm.Parameters.Add(p6);
                // Parameter07 //
                SqlParameter p7 = new SqlParameter("@inupt_linkedserver2", SqlDbType.NVarChar, 50);
                p7.Value = Parameter07;
                cm.Parameters.Add(p7);

                System.Data.IDbDataParameter _Procedur = cm.CreateParameter();
                _Procedur.ParameterName = "return_message";
                _Procedur.Direction = System.Data.ParameterDirection.Output;
                _Procedur.DbType = System.Data.DbType.AnsiString;
                _Procedur.Size = 900;
                cm.Parameters.Add(_Procedur);

                cm.ExecuteScalar();
                _returnVaules = Convert.ToString(_Procedur.Value);

                return _returnVaules;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
