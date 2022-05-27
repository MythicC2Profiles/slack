using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slack_server.Model.Mythic
{
    public class ConversationHistoryResponse
    {
        public bool ok { get; set; }
        public List<SlackMessages> messages { get; set; }
        public bool has_more { get; set; }
        public int pin_count { get; set; }
        public ResponseMetadata response_metadata { get; set; }
    }
    public class ResponseMetadata
    {
        public string next_cursor { get; set; }
    }
    public class SlackMessages
    {
        public string type { get; set; }
        public string text { get; set; }
        public List<SlackFile> files { get; set; }
        public bool upload { get; set; }
        public string user { get; set; }
        public bool display_as_bot { get; set; }
        public string ts { get; set; }
        public string subtype { get; set; }
        public string username { get; set; }
        public string bot_id { get; set; }
        public string app_id { get; set; }
    }
    public class SlackFile
    {
        public string id { get; set; }
        public int created { get; set; }
        public int timestamp { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string mimetype { get; set; }
        public string filetype { get; set; }
        public string pretty_type { get; set; }
        public string user { get; set; }
        public bool editable { get; set; }
        public int size { get; set; }
        public string mode { get; set; }
        public bool is_external { get; set; }
        public string external_type { get; set; }
        public bool is_public { get; set; }
        public bool public_url_shared { get; set; }
        public bool display_as_bot { get; set; }
        public string username { get; set; }
        public string url_private { get; set; }
        public string url_private_download { get; set; }
        public string media_display_type { get; set; }
        public string permalink { get; set; }
        public string permalink_public { get; set; }
        public bool is_starred { get; set; }
        public bool has_rich_preview { get; set; }
    }
}
