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

            ViewData["CurrentSearchTerm"] = searchTerm;

            var users = await _neo4jService.SearchUsersAsync(searchTerm, userId);

            foreach (var user in users)
            {
                if (user.ID.ToString() == userId)
                {
                    user.IsCurrentUser = true;
                }
                else
                {
                    var areFriends = await _neo4jService.AreFriendsAsync(userId, user.ID.ToString());
                    if (areFriends == true)
                    {
                        user.AreFriends = areFriends;

                        user.AreCloseFriends = await _neo4jService.AreCloseFriendsAsync(userId, user.ID.ToString());
                        user.HasFollowed = await _neo4jService.HasFollowedAsync(userId, user.ID.ToString());
                        user.Blocked = await _neo4jService.HasBlockedAsync(userId, user.ID.ToString());
                    }
                    else
                    {
                        user.FriendRequestSent = await _neo4jService.HasSentFriendRequestAsync(userId, user.ID.ToString());
                        user.FriendRequestReceived = await _neo4jService.HasReceivedFriendRequestAsync(userId, user.ID.ToString());
                    }
                }
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.AddFriendAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> CancelFriendRequest(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.CancelFriendRequestAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> Unfollow(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.UnfollowAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> Follow(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.FollowAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> Unfriend(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.Unfriend_UnblockAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> Block(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.BlockAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptFriendRequest(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.AcceptFriendRequestAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> RejectFriendRequest(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.RejectFriendRequestAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.Unfriend_UnblockAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> Accept(string otherUserId)
        {
            var userId = HttpContext.Session.GetString("id");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            await _neo4jService.AcceptFriendRequestAsync(userId, otherUserId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string otherUserId)
        {
            var userId = HttpContext.Session.GetString("id");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            await _neo4jService.RejectFriendRequestAsync(userId, otherUserId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SetCloseFriend(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.CloseFriendAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCloseFriend(string otherUserId, string searchTerm)
        {
            var userId = HttpContext.Session.GetString("id");
            await _neo4jService.RemoveCloseFriendAsync(userId, otherUserId);

            return RedirectToAction("Index", new { searchTerm });
        }
    }
}