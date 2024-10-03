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

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            var id = userId ?? HttpContext.Session.GetString("id");

            if (string.IsNullOrEmpty(id))
            {
                return View("Error");
            }

            var user = await _neo4jService.GetDetailAboutByIdAsync(id);

            if (user == null)
            {
                return View("UserNotFound");
            }

            return View(user);
        }
    }
}
