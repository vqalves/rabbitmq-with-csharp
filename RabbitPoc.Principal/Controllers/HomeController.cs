using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitPoc.Principal.Business;
using RabbitPoc.Principal.Models;

namespace RabbitPoc.Principal.Controllers
{
    public class HomeController : Controller
    {
        private readonly NotificacaoBusiness notificacaoBusiness;
        private readonly ILogger<HomeController> _logger;

        public HomeController(NotificacaoBusiness notificacaoBusiness, ILogger<HomeController> logger)
        {
            this.notificacaoBusiness = notificacaoBusiness;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Enviar([FromQuery]string message)
        {
            //var content = $"{Guid.NewGuid().ToString("n")} Created: {DateTime.Now.ToString("mm:ss.fff")}";

            notificacaoBusiness.EnviarAlt(message);
            return Ok();
        }

        public IActionResult Enviar2([FromQuery] string message)
        {
            //var content = $"{Guid.NewGuid().ToString("n")} Created: {DateTime.Now.ToString("mm:ss.fff")}";
            //
            //RpcClient client = new RpcClient();
            //var response = client.Call(content);
            //return Json(response);

            var result = notificacaoBusiness.EnviarComRPC(message);
            return Json(result);
        }
        public IActionResult Enviar3([FromQuery] string message)
        {
            //var content = $"{Guid.NewGuid().ToString("n")} Created: {DateTime.Now.ToString("mm:ss.fff")}";

            notificacaoBusiness.EnviarComDelay2(message);
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
