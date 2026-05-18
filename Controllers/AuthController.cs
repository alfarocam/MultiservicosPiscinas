using Microsoft.AspNetCore.Mvc;

namespace MultiservicosPiscinas.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username)
        {
            // Simple redirect to home
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Simple redirect to login
            return RedirectToAction("Login", "Auth");
        }
    }
}
