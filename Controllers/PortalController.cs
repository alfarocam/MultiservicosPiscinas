using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiservicosPiscinas.Controllers
{
    [Authorize(Roles = "Cliente,Administrador")]
    public class PortalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}