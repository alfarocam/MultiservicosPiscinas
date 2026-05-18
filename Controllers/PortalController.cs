using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiservicosPiscinas.Controllers
{
    
    public class PortalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
