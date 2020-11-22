using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace QuestHelper.Server.Controllers
{
    [Route("api/")]
    public class ApiController : Controller
    {

        public ApiController(IConfiguration configuration)
        {
        }

        // GET api/values
        [HttpGet]
        public string Get()
        {
            string version = typeof(Startup).Assembly.GetName().Version.ToString();
            return $"Backend for QuestHelper (GoSh!) Api version:{version}";
        }
    }
}
