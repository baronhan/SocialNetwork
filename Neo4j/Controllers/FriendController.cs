using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.ViewModels;
using System.Text;

namespace Neo4j.Controllers
{
    [Route("Friend")]
    public class FriendController : Controller
    {
        private readonly Neo4jService _neo4jService;

        public FriendController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(string id, string action)
        {
            var userId = HttpContext.Session.GetString("id");
            switch (action)
            {
                case "closeFriend":
                    await _neo4jService.CloseFriendAsync(userId, id);
                    break;
                case "removeCloseFriend":
                    await _neo4jService.RemoveCloseFriendAsync(userId, id);
                    break;
                case "unfriend":
                    await _neo4jService.Unfriend_UnblockAsync(userId, id);
                    break;
                case "block":
                    await _neo4jService.BlockAsync(userId, id);
                    break;
                case "unblock":
                    await _neo4jService.Unfriend_UnblockAsync(userId, id);
                    break;
                case "follow":
                    await _neo4jService.FollowAsync(userId, id);
                    break;
                case "unfollow":
                    await _neo4jService.UnfollowAsync(userId, id);
                    break;
            }

            var updatedUser = await _neo4jService.GetFriendByIdAsync(id);

            Console.WriteLine("Blocked: " + updatedUser.Blocked);
            Console.WriteLine("AreCloseFriends: " + updatedUser.AreCloseFriends);
            Console.WriteLine("HasFollowed: " + updatedUser.HasFollowed);

            // Chỉ kiểm tra và cập nhật khi cần thiết
            if (action == "closeFriend" || action == "removeCloseFriend")
            {
                updatedUser.AreCloseFriends = await _neo4jService.AreCloseFriendsAsync(userId, id);
            }

            if (action == "follow" || action == "unfollow")
            {
                updatedUser.HasFollowed = await _neo4jService.HasFollowedAsync(userId, id);
            }

            if (action == "block" || action == "unblock")
            {
                updatedUser.Blocked = await _neo4jService.HasBlockedAsync(userId, id);
            }

            var buttonHtml = RenderButtonHtml(updatedUser);
            return Json(new { updatedButtonHtml = buttonHtml });
        }

        private string RenderButtonHtml(SearchVM user)
        {
            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<button class=\"dropdown-toggle btn btn-secondary me-2\" id=\"friendButton_" + user.ID + "\" data-bs-toggle=\"dropdown\" aria-expanded=\"true\" type=\"button\">");

            if (user.Blocked == true)
            {
                htmlBuilder.AppendLine("<i class=\"ri-close-circle-line me-1 text-white\"></i> Unlock");
                htmlBuilder.AppendLine("</button>");
            }
            else 
            {
                htmlBuilder.AppendLine("<i class=\"ri-check-line me-1 text-white\"></i> Friend");
                htmlBuilder.AppendLine("</button>");

                htmlBuilder.AppendLine("<div class=\"dropdown-menu dropdown-menu-right\" aria-labelledby=\"dropdownMenuButton_" + user.ID + "\">");
                if (user.AreCloseFriends == true)
                {
                    htmlBuilder.AppendLine("<button type=\"submit\" class=\"dropdown-item\" onclick=\"updateFriendStatus('" + user.ID + "', 'removeclosefriend')\">Remove Close Friend</button>");
                }
                else
                {
                    htmlBuilder.AppendLine("<button type=\"submit\" class=\"dropdown-item\" onclick=\"updateFriendStatus('" + user.ID + "', 'closefriend')\">Close Friend</button>");
                }

                if (user.HasFollowed == true)
                {
                    htmlBuilder.AppendLine("<button type=\"submit\" class=\"dropdown-item\" onclick=\"updateFriendStatus('" + user.ID + "', 'unfollow')\">Unfollow</button>");
                }
                else
                {
                    htmlBuilder.AppendLine("<button type=\"submit\" class=\"dropdown-item\" onclick=\"updateFriendStatus('" + user.ID + "', 'follow')\">Follow</button>");
                }

                htmlBuilder.AppendLine("<button type=\"submit\" class=\"dropdown-item\" onclick=\"updateFriendStatus('" + user.ID + "', 'unfriend')\">Unfriend</button>");
                htmlBuilder.AppendLine("<button type=\"submit\" class=\"dropdown-item\" onclick=\"updateFriendStatus('" + user.ID + "', 'block')\">Block</button>");
                htmlBuilder.AppendLine("</div>");
            }

            return htmlBuilder.ToString();
        }

    }
}
