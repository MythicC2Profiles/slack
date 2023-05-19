using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace slack_server.Model.Server
{
    public class ServerConfig
    {
        public string subscription_token { get; set; }
        public string message_token { get; set; }
        public string channel_id { get; set; }
        public bool debug { get; set; }
        public bool clear_messages { get; set; }

        public bool IsAnyNullOrEmpty()
        {
            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(this);
                    if (string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
