using Newtonsoft.Json;
using slack_server.Clients;
using slack_server.Model.Mythic;
using slack_server.Model.Server;
using SlackNet;

namespace slack_server
{
    class Program
    {
        /// <summary>
        /// Main loop
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
#if DEBUG
                Globals.serverconfig = JsonConvert.DeserializeObject<ServerConfig>(System.IO.File.ReadAllText(@"C:\Users\scott\Desktop\slack-server-config.json"));

#else
                Globals.serverconfig = JsonConvert.DeserializeObject<ServerConfig>(System.IO.File.ReadAllText("config.json")) ?? new ServerConfig();
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Main] Error parsing config: {e.Message}");
                Environment.Exit(e.HResult);
            }

            if (Globals.serverconfig.IsAnyNullOrEmpty())
            {
                Console.WriteLine("[Error] config.json is missing values!");
                Environment.Exit(0);
            }

            Globals.slackClient = new SlackClient(Globals.serverconfig);
            Globals.mythicClient = new MythicClient();
            //Start the handler
            AsyncMain(args).GetAwaiter().GetResult();
        }
        public static async Task AsyncMain(string[] args)
        {
            //Ch
            if (Globals.serverconfig.clear_messages)
            {
                await Globals.slackClient.ClearSlackMessages();
            }
            else
            {
                await Globals.slackClient.Catchup();
            }

            var slackServices = new SlackServiceBuilder()
                .UseApiToken(Globals.serverconfig.message_token)
                .UseAppLevelToken(Globals.serverconfig.subscription_token)
                .RegisterEventHandler(ctx => new SlackMessageHandler(ctx.ServiceProvider.GetApiClient()));

            using var socketModeClient = slackServices.GetSocketModeClient();

            await socketModeClient.Connect();

            if (!socketModeClient.Connected)
            {
                Console.WriteLine("Failed to connect to slack.");
                Environment.Exit(0);
            }

            Console.WriteLine("Started Server!");
            await SendLoop();
        }
        static async Task SendLoop()
        {
            while (true) //This is single threaded so that we can limit impact of large amounts of messages needing to be sent
            {
                try
                {
                    //This should block until a message is available.
                    MessageQueue curMsg = Globals.outqueue.Take();
                    if (curMsg.is_file)
                    {
                        //Kick off seperate thread so a long file upload doesn't force other agents to wait
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async() =>
                        {
                            await Globals.slackClient.SendMessage(curMsg.message, curMsg.sender_id);
                            //Attempt 3 times and then give up.
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                    {
                        await Globals.slackClient.SendMessage(curMsg.message, curMsg.sender_id);
                    }
                    await Task.Delay(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[SendLoop] {e.Message}");
                }
            }
        }
    }
}
