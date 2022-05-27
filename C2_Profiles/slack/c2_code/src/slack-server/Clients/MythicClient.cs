using System.Net;

namespace slack_server.Model.Mythic
{
    public class MythicClient
    {
        private static HttpClient mythicClient = new HttpClient();

        public MythicClient()
        {
            mythicClient.DefaultRequestHeaders.Add("mythic", "slack");
            ServicePointManager.DefaultConnectionLimit = 10;
        }

        public async Task<string> SendToMythic(string data)
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
                string strResponse = await response.Content.ReadAsStringAsync();
                return strResponse;
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

            return "";
        }
    }
}
