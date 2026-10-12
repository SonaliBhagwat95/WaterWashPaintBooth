
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintBooth.Repositories.Interface;

namespace Bullows.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _repo;

        public DashboardController(IDashboardRepository repo)
        {
            _repo = repo;
        }

        // GET /Dashboard/Index
        public IActionResult Index()
        {
            var model = _repo.GetDashboardData();
            return View(model);
        }
    }
}
