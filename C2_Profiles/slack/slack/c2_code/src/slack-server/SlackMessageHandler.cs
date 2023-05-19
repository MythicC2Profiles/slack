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
            Console.WriteLine($"({slackEvent.Subtype})Message Received!");
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
                    if (!string.IsNullOrEmpty(slackEvent.Text))
                    {
                        await ProcessMessage(slackEvent);
                    }
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
            catch (Exception e)
            {
                Console.WriteLine($"[SlackMessageHandler] Failed to decode message: {e}");
                Console.WriteLine($"[SlackMessageHandler] Text: {slackEvent.Text}");
                Console.WriteLine($"[SlackMessageHandler] Event: {slackEvent.Type}");
            }
        }
       

    }
}
