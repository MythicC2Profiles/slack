using slack_server.Model.Server;
using System.Net;

namespace slack_server.Model.Mythic
{
    public class MythicClient
    {
        private static HttpClient mythicClient = new HttpClient();
        public delegate void MythicMessageReadyHandler(object sender, MythicEventArgs e);
        public event EventHandler<MythicEventArgs> MythicMessageReady;
        public MythicClient()
        {
            mythicClient.DefaultRequestHeaders.Add("mythic", "slack");
            ServicePointManager.DefaultConnectionLimit = 10;
        }

        public async Task SendToMythic(string data, string sender)
        {
#if DEBUG
            string url = "http://192.168.4.201:17443/api/v1.4/agent_message";
#else
            string url = Environment.GetEnvironmentVariable("MYTHIC_ADDRESS");
#endif

            try //POST Slack Message
            {
                HttpContent postBody = new StringContent(data);
                var response = await mythicClient.PostAsync(url, postBody);

                string strRes = await response.Content.ReadAsStringAsync();
                
                MythicEventArgs args = new MythicEventArgs(strRes, sender);
                MythicMessageReady(this, args);
            }
            catch (WebException ex)
            {
                Console.WriteLine($"[SendToMythic] WebException: {ex.Message} - {ex.Status}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SendToMythic] Exception: {e.Message}");
                Console.WriteLine(e.InnerException);
                Console.WriteLine(e.StackTrace);
            };
        }
    }
}
