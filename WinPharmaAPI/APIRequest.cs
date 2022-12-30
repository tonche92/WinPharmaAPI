using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace WinPharmaAPI
{
    public class APIRequest
    {
        public APIResult result { get; set; }
        public string status { get; set; }
        public string requestId { get; set; }
        public APIRequest()
        {
            result = new APIResult();
            status = "";
            requestId = "";
        }
        public APIRequest(APIResult result, string status, string requestId)
        {
            this.result = result;
            this.status = status;
            this.requestId = requestId;
        }
        public APIRequest(int? Skip, int? Take, string? Expand, string? Filter, string? OrderBy, string? OrderDirection, IMemoryCache cache, IConfiguration config)
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
            var res = new APIRequest();
            try
            {
                var url = config.GetValue<string>(
                "url");
                var auth = config.GetValue<string>(
                "base64Auth");
                var json = GetJsonFromUrl(url + addToUrl, auth);
                res = JsonConvert.DeserializeObject<APIRequest>(json);
                //Если апи ответила нормально, но со статусом Error - берем из кэша
                if (res != null && res.status == "Error")
                {
                    //Если в кэше не найдено - бросаем ошибку
                    if (!cache.TryGetValue(addToUrl, out res))
                    {
                        throw new Exception("API overloaded and value not in cache");
                    }
                    //Если найдено - меняем статус
                    else
                    {
                        //не понятно зачем эта проверка, но без нее студия ругается на possible null, не смотря на проверку на 62 строчке.
                        if (res != null)
                            res.status = "FromCache";
                    }
                }
                //Добавляем в кэш по ключу и полученному res
                else
                {
                    cache.Set(addToUrl, res);
                }
            }
            //Отлавливаем непредвиденные ошибки, пытаемся взять значение из кэша, если не получается - кидаем ошибку
            catch
            {
                if (!cache.TryGetValue(addToUrl, out res))
                {
                    throw new Exception("Error occured and value not in cache");
                }
                else
                {
                    if (res != null)
                        res.status = "FromCache";
                }
            }
            if (res != null)
            {
                this.result = res.result;
                this.status = res.status;
                this.requestId = res.requestId;
            }
            else
            {
                this.result = new APIResult();
                this.status = "Error";
                this.requestId = "Error";
            }
        }

        public static string GetJsonFromUrl(string url, string base64EncodedAuthenticationString)
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
        public int totalItems { get; set; }
        public APIResultRow[] items { get; set;}
        public APIResult()
        {
            this.totalItems = 0;
            this.items = new APIResultRow[0];
        }
    }

    public class APIResultRow
    {
        public string code { get; set; }
        public string title { get; set; }
        public string manufacturer { get; set; }
        public string description { get; set; }
        public string price { get; set; }
        public int stock { get; set; }

        public APIResultRow()
        {
            code = "";
            title = "";
            manufacturer = "";
            description = "";
            price = "";
            stock = 0;
        }
    }
}