using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slack_server.Model.Server
{
    public class MythicEventArgs : EventArgs
    {
        public string message { get; set; }
        public string sender_id { get; set; }
        public MythicEventArgs(string message, string sender_id)
        {
            this.message = message;
            this.sender_id = sender_id;
        }
    }
}
