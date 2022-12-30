using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;

namespace WinPharmaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiRedirectionController : Controller
    {
        //Задаем кэш
        private IMemoryCache cache;
        public ApiRedirectionController(IMemoryCache memoryCache)
        {
            cache = memoryCache;
        }
        [HttpGet("~/RedirectToAPI", Name = "RedirectToAPI")]
        public ActionResult RedirectToAPI(int? Skip, int? Take, string? Expand, string? Filter, string? OrderBy, string? OrderDirection)
        {
            var url = "http://fakestock.everys.com/api/v1/Stock?";
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
                var json = GetJsonFromUrl(url + addToUrl);
                res = JsonConvert.DeserializeObject<APIRequest>(json);
                //Если апи ответила нормально, но со статусом Error - берем из кэша
                if (res.status == "Error")
                {
                    //Если в кэше не найдено - бросаем ошибку
                    if (!cache.TryGetValue(addToUrl, out res))
                    {
                        throw new Exception("API overloaded and value not in cache");
                    }
                    //Если найдено - меняем статус
                    else
                    {
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
            catch (Exception ex)
            {
                if (!cache.TryGetValue(addToUrl, out res))
                {
                    throw new Exception("Error occured ("+ex.Message+") and value not in cache");
                }
                else
                {
                    res.status = "FromCache";
                }
            }

            return Json(res);
        }

        public static string GetJsonFromUrl(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
            url);

            var authenticationString = "candidate:candidate321";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
            request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

            //request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            var client = new HttpClient();

            var response = client.Send(request);
            var stream = response.Content.ReadAsStream();
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            return json;
        }
    }
}
