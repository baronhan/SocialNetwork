using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;

namespace Neo4j.Models.ViewComponents
{
    public class ContactAndBasicInfoViewComponent : ViewComponent
    {
        private readonly Neo4jService _neo4jService;

        public ContactAndBasicInfoViewComponent(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            var userId = id ?? HttpContext.Session.GetString("id");

            if (string.IsNullOrEmpty(userId))
            {
                return View("Error");
            }

            var user = await _neo4jService.GetContactAndBasicInfoByIdAsync(userId);

            if (user == null)
            {
                return View("UserNotFound");
            }

            return View(user);
        }
    }
}
