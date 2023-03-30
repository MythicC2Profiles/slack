using slack_server.Model.Mythic;
using slack_server.Model.Server;

namespace slack_server
{
    public static class Utilities
    {
        public static async Task HandleAgentMessage(MythicMessageWrapper mw)
        {
            await Globals.mythicClient.SendToMythic(mw.message, mw.sender_id);
        }
    }
}
