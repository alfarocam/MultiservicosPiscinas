using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MultiservicosPiscinas.Models;

namespace MultiservicosPiscinas.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

    }
}
