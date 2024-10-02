using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.ViewModels;

namespace Neo4j.Models.ViewComponents
{
    public class FriendListTimeLineViewComponent : ViewComponent
    {
        private readonly Neo4jService _neo4jService;

        public FriendListTimeLineViewComponent(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var id = HttpContext.Session.GetString("id");

            if (string.IsNullOrEmpty(id))
            {
                return View("Error");
            }

            IEnumerable<SearchVM> users = new List<SearchVM>();


            users = await _neo4jService.FriendListTimeLineByIdAsync(id);

            if (users == null)
            {
                return View("UserNotFound");
            }

            return View("Default", users);
        }
    }
}
