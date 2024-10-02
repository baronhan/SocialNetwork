using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.ViewModels;

namespace Neo4j.Models.ViewComponents
{
    public class DetailAboutViewComponent : ViewComponent
    {

        private readonly Neo4jService _neo4jService;

        public DetailAboutViewComponent(Neo4jService neo4jService)
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

            var user = await _neo4jService.GetDetailAboutByIdAsync(userId);

            if (user == null)
            {
                return View("UserNotFound");
            }

            return View(user);
        }
    }
}
