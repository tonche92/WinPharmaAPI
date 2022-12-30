using Newtonsoft.Json;

namespace WinPharmaAPI
{
    public class APIRequest
    {
        public APIResult result { get; set; }
        public string status { get; set; }
        public string requestId { get; set; }
    }

    public class APIResult
    {
        public int totalItems { get; set; }
        public APIResultRow[] items { get; set;}
    }

    public class APIResultRow
    {
        public string code { get; set; }
        public string title { get; set; }
        public string manufacturer { get; set; }
        public string description { get; set; }
        public string price { get; set; }
        public int stock { get; set; }
    }
}