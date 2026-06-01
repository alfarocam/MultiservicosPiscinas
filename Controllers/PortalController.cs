using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "3")]
    public class PortalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
