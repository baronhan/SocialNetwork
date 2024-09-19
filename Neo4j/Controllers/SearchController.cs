using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;

namespace Neo4j.Controllers
{
    public class SearchController : Controller
    {
        private readonly Neo4jService _neo4jService;

        public SearchController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm)
        {
            var users = await _neo4jService.SearchUsersAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View(users); 
        }
    }
}