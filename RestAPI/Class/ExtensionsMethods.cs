using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace RestAPI.Class
{
    public static class ExtensionsMethods
    {
        public static string GenerateSHA256String(this string inputString)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }
        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
        public static string DataTableToJson(this DataTable dt)
        {

            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                data.Add(row);
            }

            //string xxx= JsonConvert.SerializeObject(data, new JsonSerializerSettings
            //{
            //    Formatting = Formatting.Indented
            //});
            return JsonConvert.SerializeObject(data);
        }
    }
}
