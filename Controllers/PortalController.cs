/*
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiserviciosPiscinas.Controllers
{
    
    public class PortalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiserviciosPiscinas.Controllers
{
    
    // Si intenta entrar a la fuerza sin loguearse, el sistema lo re manda  al Login.
    [Authorize]
    public class PortalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}