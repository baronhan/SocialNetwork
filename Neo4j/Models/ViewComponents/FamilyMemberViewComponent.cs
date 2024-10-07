using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;


namespace Neo4j.Models.ViewComponents
{
    public class FamilyMemberViewComponent : ViewComponent
    {
        private readonly Neo4jService _neo4jService;

        public FamilyMemberViewComponent(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            var familyMembers = await _neo4jService.GetFamilyMembersAsync(userId);
            return View(familyMembers);
        }
    }

}