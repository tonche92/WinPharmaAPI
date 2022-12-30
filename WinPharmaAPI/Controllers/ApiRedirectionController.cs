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
        private readonly IConfiguration config;
        //Задаем кэш
        private IMemoryCache cache;
        public ApiRedirectionController(IMemoryCache memoryCache, IConfiguration _config)
        {
            cache = memoryCache;
            config = _config;
        }
        [HttpGet("~/RedirectToAPI", Name = "RedirectToAPI")]
        public ActionResult RedirectToAPI(int? Skip, int? Take, string? Expand, string? Filter, string? OrderBy, string? OrderDirection)
        {
            var res = new APIRequest(Skip, Take, Expand, Filter, OrderBy, OrderDirection, cache, config);

            return Json(res);
        }
    }
}
