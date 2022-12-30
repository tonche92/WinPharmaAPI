using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace WinPharmaAPI
{
    public class APIRequest
    {
        public APIResult? result { get; set; }
        public string? status { get; set; }
        public string? requestId { get; set; }
        public APIRequest()
        {
        }
        public APIRequest(int? Skip, int? Take, string? Expand, string? Filter, string? OrderBy, string? OrderDirection, IMemoryCache cache, IConfiguration config)
        {
            string addToUrl = setUrl(Skip, Take, Expand, Filter, OrderBy, OrderDirection);
            try
            {
                setFromAPI(addToUrl, cache, config);
            }
            catch
            {
                setFromCache(addToUrl, cache);
            }
        }
        private void setFromAPI(string addToUrl, IMemoryCache cache, IConfiguration config)
        {
            var url = config.GetValue<string>(
                "url");
            var auth = config.GetValue<string>(
            "base64Auth");
            var json = GetJsonFromUrl(url + addToUrl, auth);
            var res = JsonConvert.DeserializeObject<APIRequest>(json);
            if (res == null || res.status == "Error")
                throw new Exception("API unavailible or overloaded.");
            else
            {
                cache.Set(addToUrl, res);
                result = res.result;
                status = res.status;
                this.requestId = res.requestId;
            }
        }

        private void setFromCache(string addToUrl, IMemoryCache cache)
        {
            //Если в кэше не найдено - бросаем ошибку
            if (!cache.TryGetValue(addToUrl, out APIRequest res))
            {
                throw new Exception("API overloaded and value not in cache");
            }
            //Если найдено - меняем статус
            else
            {
                result = res.result;
                requestId = res.requestId;
                status = "FromCache";
            }
        }

        private static string setUrl (int? Skip, int? Take, string? Expand, string? Filter, string? OrderBy, string? OrderDirection)
        {
            //объявляем переменную, которая будет ключем в кэше
            string addToUrl = "";
            if (Skip != null)
            {
                addToUrl += "Skip=" + Skip.ToString();
            }
            if (Take != null)
            {
                addToUrl += (addToUrl != "" ? "&" : "") + "Take=" + Take.ToString();
            }
            if (Expand != null)
            {
                addToUrl += (addToUrl != "" ? "&" : "") + "Expand=" + Expand;
            }
            if (Filter != null)
            {
                addToUrl += (addToUrl != "" ? "&" : "") + "Filter=" + Filter;
            }
            if (OrderBy != null)
            {
                addToUrl += (addToUrl != "" ? "&" : "") + "OrderBy=" + OrderBy;
            }
            if (OrderDirection != null)
            {
                addToUrl += (addToUrl != "" ? "&" : "") + "OrderDirection=" + OrderDirection;
            }
            return addToUrl;
        }

        private static string GetJsonFromUrl(string url, string base64EncodedAuthenticationString)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
            url);

            request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

            var client = new HttpClient();

            var response = client.Send(request);
            var stream = response.Content.ReadAsStream();
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            return json;
        }
    }

    public class APIResult
    {
        public int? totalItems { get; set; }
        public APIResultRow[]? items { get; set;}
        public APIResult()
        {
            this.totalItems = 0;
            this.items = new APIResultRow[0];
        }
    }

    public class APIResultRow
    {
        public string? code { get; set; }
        public string? title { get; set; }
        public string? manufacturer { get; set; }
        public string? description { get; set; }
        public string? price { get; set; }
        public int? stock { get; set; }
    }
}