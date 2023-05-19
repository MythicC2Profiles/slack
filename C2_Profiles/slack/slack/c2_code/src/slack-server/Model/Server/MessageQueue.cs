using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slack_server.Model.Server
{
    public class MessageQueue
    {
        public string sender_id { get; set; }
        public string message { get; set; }
        public bool is_file { get; set; }
    }
}
