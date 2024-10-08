using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4jClient;

namespace Neo4j.Controllers
{
    public class FamilyController : Controller
    {
        private readonly Neo4jService _neo4jService;

        public FamilyController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }
        [HttpPost]
        public async Task<IActionResult> AddFamilyRequest(string familyUsername, string relationship)
        {
            var userId = HttpContext.Session.GetString("id");

            var checkisfriend = await _neo4jService.CheckFamilyRequestIsFriend(userId, familyUsername);
            var checkfamilyrequest = await _neo4jService.CheckFamilyRequest(userId, familyUsername);
            if (!checkisfriend)
            {
                return Json(new { success = false, message = "You cannot add who is not your friend." });
            }
            if (checkfamilyrequest)
            {
                return Json(new { success = false, message = "You already sent a family request to this user." });
            }

            var result = await _neo4jService.AddFamilyRequestAsync(userId, familyUsername, relationship);
            if (result)
            {
                return Json(new { success = true, message = "Family request sent successfully." });
            }

            return Json(new { success = false, message = "Error when sending a family request" });
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmFamilyRequest(string requestId, string requestname, string requestRelationship)
        {
            var userId = HttpContext.Session.GetString("id");
            var users = await _neo4jService.GetUsersByUsernameAsync(requestname);
            if (!users.Any())
            {
                return Json(new { success = false, message = "User not found." });
            }
            var isConfirmed = await _neo4jService.ConfirmFamilyRequestAsync(requestId);
            if (isConfirmed)
            {
                await _neo4jService.DeleteFamilyRequestAsync(requestId);

                var isAdded = await _neo4jService.AddFamilyMemberAsync(userId, requestname, requestRelationship);
                if (isAdded)
                {
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, message = "Failed to confirm and add family." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFamilyRequest(string requestId)
        {
            var result = await _neo4jService.DeleteFamilyRequestAsync(requestId);
            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetFamilyRequests()
        {
            var userId = HttpContext.Session.GetString("id");
            var requests = await _neo4jService.GetFamilyRequestsAsync(userId);
            return View(requests);
        }


        [HttpGet]
        public async Task<IActionResult> GetUserSuggestions(string username)
        {
            var userId = HttpContext.Session.GetString("id");
            var users = await _neo4jService.GetUsersByUsernameOnlyFriendAsync(username, userId);
            return Json(users.Select(u => new { u.Username, u.Firstname, u.Lastname, u.ProfileImage }));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFamilyMember(string FriendId, string relationship)
        {
            var userId = HttpContext.Session.GetString("id");

            if (string.IsNullOrEmpty(FriendId) || string.IsNullOrEmpty(relationship))
            {
                return Json(new { success = false, message = "Invalid input." });
            }

            try
            {
                var result = await _neo4jService.UpdateFamilyMemberAsync(FriendId, relationship, userId);

                if (result)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update family member." });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFamilyMember(string userId)
        {
            var _userId = HttpContext.Session.GetString("id");

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Invalid input." });
            }

            try
            {
                var result = await _neo4jService.DeleteFamilyMemberAsync(userId, _userId);

                if (result)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete family member." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}