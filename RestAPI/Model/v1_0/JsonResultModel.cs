using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Model.v1_0
{
    public class JsonResultModel
    {
        public int result { get; set; }  // 0 Nothing  1 OK   2 Warning  3 Error
        public string message { get; set; }
        public object data { get; set; }

        public JsonResultModel(int result, string message, object data)
        {
            this.result = result;
            this.message = message;
            this.data = data;
        }
    }
}
