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
        public static async Task Main(string[] args)
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
            Globals.mythicClient.MythicMessageReady += onMythicMessageReady;



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
            await Task.Delay(-1);
        }

        static async void onMythicMessageReady(object sender, MythicEventArgs e)
        {
            try
            {
                await Globals.slackClient.SendMessage(e.message, e.sender_id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
