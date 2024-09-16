using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;

namespace Neo4j.Controllers
{
    public class UserController : Controller
    {
        private readonly Neo4jService _neo4jService;

        public UserController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        //public async Task<IActionResult> CreateUser(string username)
        //{
        //    await _neo4jService.ConnectAsync();
        //    await _neo4jService.CreateUserAsync(username);
        //    return View("UserCreated", username);
        //}

        //public async Task<IActionResult> AddFriend(string userA, string userB)
        //{
        //    await _neo4jService.ConnectAsync();
        //    await _neo4jService.CreateFriendshipAsync(userA, userB);
        //    return View("FriendAdded", new { userA, userB });
        //}
    }
}
