using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitPoc.Publisher.Business;
using RabbitPoc.Publisher.Models;

namespace RabbitPoc.Publisher.Controllers
{
    public class HomeController : Controller
    {
        private readonly MQService MQService;
        private readonly ILogger<HomeController> Logger;

        public HomeController(MQService mqService, ILogger<HomeController> logger)
        {
            this.MQService = mqService;
            this.Logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/api/send")]
        public IActionResult Send()
        {
            MQService.SendExampleMessage2();
            return Ok();
        }

        [HttpPost("/api/send-with-response")]
        public IActionResult SendWithResponse(string message)
        {
            var result = MQService.SendAndWaitResponse_WithRPC(message);
            return Json(result);
        }

        [HttpPost("/api/send-with-delay")]
        public IActionResult SendWithDelay(string message)
        {
            MQService.SendExampleMessage1_WithDelay(message);
            return Ok();
        }
    }
}
