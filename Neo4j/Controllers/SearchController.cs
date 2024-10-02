using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.Services;

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
            var userId = HttpContext.Session.GetString("id");

            var users = await _neo4jService.SearchUsersAsync(searchTerm);

            foreach (var user in users)
            {
                if (user.ID.ToString() == userId)
                {
                    user.IsCurrentUser = true;
                }
                else
                {
                    var areFriends = await _neo4jService.AreFriendsAsync(userId, user.ID.ToString());
                    user.AreFriends = areFriends;
                }
            }

            return View(users);
        }
    }
}