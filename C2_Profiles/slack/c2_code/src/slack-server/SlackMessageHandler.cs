using Newtonsoft.Json;
using slack_server.Model.Mythic;
using SlackNet;
using SlackNet.Events;

namespace slack_server
{
    public class SlackMessageHandler : IEventHandler<MessageEvent>
    {
        private static int _count;

        private readonly ISlackApiClient _slack;
        public SlackMessageHandler(ISlackApiClient slack) => _slack = slack;
        public async Task Handle(MessageEvent slackEvent)
        {
            if(slackEvent.Channel != Globals.serverconfig.channel_id)
            {
                return;
            }

            switch (slackEvent.Subtype)
            {
                case "message_deleted":
                    break;
                case "bot_message":
                    await ProcessMessage(slackEvent);
                    break;
                case "file_share": //I'm apparently not handling this right now
                    await ProcessMessage(slackEvent);
                    break;
                default:
                    Console.WriteLine("Unknown Event: " + slackEvent.Subtype);
                    break;
            }
        }
        private async Task ProcessMessage(MessageEvent slackEvent)
        {
            try
            {
                MythicMessageWrapper mw = JsonConvert.DeserializeObject<MythicMessageWrapper>(slackEvent.Text) ?? new MythicMessageWrapper();
                
                if (mw.to_server)
                {
                    await Globals.slackClient.DeleteMessage(slackEvent.Ts);

                    if (String.IsNullOrEmpty(mw.message))
                    {
                        //It's a file, inform Athena that we've begun processing.
                        await Globals.slackClient.AddReaction(slackEvent.Ts);
                        mw.message = await Globals.slackClient.DownloadFile(slackEvent.Files.FirstOrDefault().UrlPrivate);
                    }
                    await Utilities.HandleAgentMessage(mw);
                }
            }
            catch
            {
                Console.WriteLine($"[SlackMessageHandler] Failed to decode message: {slackEvent.Text}");
                Console.WriteLine($"[SlackMessageHandler] Failed to decode message: {slackEvent.Type}");
            }
        }
       

    }
}
