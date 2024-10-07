using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;

namespace Neo4j.Models.ViewComponents
{
    public class FamilyRequestViewComponent : ViewComponent
    {
        private readonly Neo4jService _neo4jService;

        public FamilyRequestViewComponent(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            var requests = await _neo4jService.GetFamilyRequestsAsync(userId);
            return View(requests);
        }
    }
}