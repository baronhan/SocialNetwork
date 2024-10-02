using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.Models;
using Neo4j.ViewModels;
using System.Diagnostics;

namespace Neo4j.Controllers
{
    public class HomeController : Controller
    {
        private readonly Neo4jService _neo4jService;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, Neo4jService neo4jService)
        {
            _logger = logger;
            _neo4jService = neo4jService;
        }

        public async Task<IActionResult> Index()
        {
            var id = HttpContext.Session.GetString("id");

            ProfileVM user = null;

            if (id != null)
            {
                user = await _neo4jService.GetProfileByIdAsync(id);
            }
            else
            {
                return RedirectToAction("SignIn", "SignUp");
            }

            return View(user);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
