using Microsoft.Extensions.Logging;
using RestAPI.Class;
using RestAPI.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Services
{
    public interface IUserService
    {
        bool IsAnExistingUser(string userName);
        bool IsValidUserCredentials(string userName, string password);
        string GetUserRole(string userName);
        List<Warehous> GetUserWarehous(string userName);
        bool loggingInJWT(string userName, string jwt);
        bool loggingOutJWT(string userName, string jwt);
    }
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ISessionMethods _sessionMethods;

        private readonly IDictionary<string, string> _users = new Dictionary<string, string>
        {
            { "test1", "password1" },
            { "test2", "password2" },
            { "admin", "securePassword" }
        };
        // inject your database here for user validation
        public UserService(ILogger<UserService> logger, ISessionMethods SessionMethods)
        {
            _logger = logger;
            _sessionMethods = SessionMethods;
        }

        public bool IsValidUserCredentials(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            SQLServer sqlConnect = _sessionMethods.GetConnectionSA();
            //string s = "SELECT *  FROM[SCPRD].[wmwhse1].[PS_BIDDING_CODE]";
            //DataTable dt = sqlConnect.SelectDataTable(s);
            string passwordSHA = password.GenerateSHA256String();
            _logger.LogInformation($"Validating user [{userName}]");
            string q = "";
            q = " SELECT * FROM [SCPRDD1].[dbo].[e_sso_user]";
            q += " WHERE sso_user_name='" + userName + "'";
            q += " AND sso_hash_password='" + password + "'";

            DataTable dtCheckLogin = sqlConnect.SelectDataTable(q);
            if (dtCheckLogin.Rows.Count == 0)
            {
                sqlConnect.CloseConnection();
                return false;
            }
            //else
            //{
            //    string fullName = dtCheckLogin.Rows[0]["fully_qualified_id"].ToString();

            //    //q = " SELECT db_logid CODE,db_name DISPLAY";
            //    //q += " ,'WMwhSqlWMwhSql'+RIGHT(db_logid,1)PASS,'"+ fullName + "' FULLNAME";
            //    //q += " FROM SCPRD.[wmsadmin].[pl_db]";
            //    //q += " WHERE db_logid<>'enterprise'";
            //    q = " SELECT C.db_logid CODE,C.[db_name] DISPLAY";
            //    q += " ,'WMwhSqlWMwhSql'+RIGHT(C.db_logid,1)PASS,'" + fullName + "' FULLNAME";
            //    q += " FROM [SCPRDD1].[dbo].[e_sso_user_role] A";
            //    q += " INNER JOIN [SCPRDD1].[dbo].[e_sso_role] B";
            //    q += " ON B.e_sso_role_id=A.[sso_role_id]";
            //    q += " INNER JOIN  SCPRD.[wmsadmin].[pl_db] C";
            //    q += " ON C.db_logid=B.sso_role_name";
            //    q += " WHERE A.externuserid='" + userName + "'";
            //    DataTable dtWarehouse = sqlConnect.SelectDataTable(q);
            //    q = "";
            //}
            sqlConnect.CloseConnection();
            return true;
        }

        public bool IsAnExistingUser(string userName)
        {
            return _users.ContainsKey(userName);
        }

        public string GetUserRole(string userName)
        {
            if (!IsAnExistingUser(userName))
            {
                return string.Empty;
            }

            if (userName == "admin")
            {
                return UserRoles.Admin;
            }

            return UserRoles.BasicUser;
        }
        public List<Warehous> GetUserWarehous(string userName)
        {
            SQLServer sqlConnect = _sessionMethods.GetConnectionSA();
            string q;
            q = " SELECT C.db_logid CODE,C.[db_name] DISPLAY";
            q += " FROM [SCPRDD1].[dbo].[e_sso_user_role] A";
            q += " INNER JOIN [SCPRDD1].[dbo].[e_sso_role] B";
            q += " ON B.e_sso_role_id=A.[sso_role_id]";
            q += " INNER JOIN  SCPRD.[wmsadmin].[pl_db] C";
            q += " ON C.db_logid=B.sso_role_name";
            q += " WHERE A.externuserid='" + userName + "'";
            DataTable dtWarehouse = sqlConnect.SelectDataTable(q);
            List<Warehous> result = new List<Warehous>();
            foreach (DataRow r in dtWarehouse.Rows)
            {
                Warehous _result = new Warehous();
                _result.Code = r["CODE"].ToString();
                _result.Display = r["DISPLAY"].ToString();
                result.Add(_result);
            }
            sqlConnect.CloseConnection();
            return result;
        }
        public bool loggingInJWT(string userName, string jwt)
        {
            try
            {
                SQLServer sqlConnect = _sessionMethods.GetConnection();
                try
                {
                    string i = "";
                    i = "INSERT [dbo].[activeUser] ([username],[jwt],[login]) VALUES ('" + userName + "','" + jwt + "',1)";
                    sqlConnect.Execute(i);
                    sqlConnect.CloseConnection();
                    return true;
                }
                catch
                {
                    sqlConnect.CloseConnection();
                    return false;
                }
            }

            catch
            {
                return false;
            }
        }
        public bool loggingOutJWT(string userName, string jwt)
        {
            try
            {
                SQLServer sqlConnect = _sessionMethods.GetConnection();
                try
                {
                    string i = "";
                    i = "UPDATE [dbo].[activeUser] SET [login] = 0 WHERE username='" + userName + "' and jwt ='" + jwt + "'";
                    sqlConnect.Execute(i);
                    sqlConnect.CloseConnection();
                    return true;
                }
                catch
                {
                    sqlConnect.CloseConnection();
                    return false;
                }
            }

            catch
            {
                return false;
            }
        }
    }
    public class Warehous
    {
        public string Code { get; set; }
        public string Display { get; set; }
    }
    public static class UserRoles
    {
        public const string Admin = nameof(Admin);
        public const string BasicUser = nameof(BasicUser);
    }
}
