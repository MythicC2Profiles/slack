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
                Console.WriteLine($"[Main] Error parsing config: {e}");
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
            if (Globals.serverconfig.clear_messages)
            {
                Console.WriteLine("Clearing messages before server start.");
                await Globals.slackClient.ClearSlackMessages();
                Console.WriteLine("Done clearing.");
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

            Console.WriteLine("Server started!");
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
                        Task.Run(async() =>
                        {
                            await Globals.slackClient.SendMessage(curMsg.message, curMsg.sender_id);
                            //Attempt 3 times and then give up.
                        });
                    }
                    else
                    {
                        await Globals.slackClient.SendMessage(curMsg.message, curMsg.sender_id);
                    }
                    await Task.Delay(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[SendLoop] {e}");
                }
            }
        }
    }
}
