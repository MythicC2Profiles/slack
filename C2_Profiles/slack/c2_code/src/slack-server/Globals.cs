using Newtonsoft.Json;
using slack_server.Clients;
using slack_server.Model.Mythic;
using slack_server.Model.Server;
using System.Collections.Concurrent;

namespace slack_server
{
    public class Globals
    {
        public static ServerConfig? serverconfig;
        public static SlackClient slackClient;
        public static MythicClient mythicClient;
        public static BlockingCollection<MessageQueue> outqueue = new BlockingCollection<MessageQueue>();
    }
}
