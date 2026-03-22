using Microsoft.AspNetCore.Mvc;
using SomonStore.Models;

namespace SomonStore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel());
        }
    }
}