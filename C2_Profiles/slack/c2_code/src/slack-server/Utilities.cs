using slack_server.Model.Mythic;
using slack_server.Model.Server;

namespace slack_server
{
    public static class Utilities
    {
        public static async Task<bool> HandleAgentMessage(MythicMessageWrapper mw)
        {
            string res = await Globals.mythicClient.SendToMythic(mw.message);

            if (!String.IsNullOrEmpty(res))
            {
                bool is_file = false;

                if (res.Count() > 3850)
                {
                    is_file = true;
                }

                Globals.outqueue.Add(new MessageQueue()
                {
                    is_file = is_file,
                    message = res,
                    sender_id = mw.sender_id,
                });
            }
            return true;
        }
    }
}
