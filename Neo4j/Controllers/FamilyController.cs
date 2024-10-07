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
            var result = await _neo4jService.AddFamilyRequestAsync(userId, familyUsername, relationship);
            if (result)
            {
                return Json(new { success = true, message = "Family request sent successfully." });
            }
            return Json(new { success = false, message = "Failed to send family request." });
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
            var users = await _neo4jService.GetUsersByUsernameAsync(username);
            return Json(users.Select(u => new { u.Username, u.Firstname, u.Lastname, u.ProfileImage }));
        }

    }
}