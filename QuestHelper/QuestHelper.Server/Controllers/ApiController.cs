using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuestHelper.Server.Controllers
{
    [Route("api/")]
    public class ApiController : Controller
    {
        private ILogger<ApiController> logger;
        public ApiController(IConfiguration configuration, ILogger<ApiController> logger)
        {
            this.logger = logger;
        }

        // GET api/values
        [HttpGet]
        public string Get()
        {
            string version = typeof(Startup).Assembly.GetName().Version.ToString();
            this.logger.LogInformation(version);
            return $"Backend for QuestHelper (GoSh!) Api version:{version}";
        }
    }
}
