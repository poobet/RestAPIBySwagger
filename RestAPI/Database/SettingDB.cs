using System;
using System.Data.SqlClient;
using System.Data;

namespace RestAPI.Database
{
    public class ConfigConnectDB
    {
        public String storerkey { get; set; }
        public String warehouse { get; set; }

        public String server { get; set; }
        public String database { get; set; }
        public String Login { get; set; }
        public String password { get; set; }

        public String serverSA { get; set; }
        public String LoginSA { get; set; }
        public String passwordSA { get; set; }
        public String databaseSA { get; set; }

        public String serverSCE { get; set; }
        public String LoginSCE { get; set; }
        public String passwordSCE { get; set; }
        public String databaseSCE { get; set; }

    }
}
