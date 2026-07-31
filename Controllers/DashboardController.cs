using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Interfaces;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepo;

        public DashboardController(IDashboardRepository dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        public async Task<IActionResult> Index()
        {
            var dto = await _dashboardRepo.ObtenerDashboardAsync();
            return View(dto);
        }
    }
}
