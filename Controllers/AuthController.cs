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
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username)
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecoverPassword(string email)
        {
            // Implementation for password recovery
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Simple redirect to login
            return RedirectToAction("Login", "Auth");
        }
    }
}
