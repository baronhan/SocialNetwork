using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;

namespace Neo4j.Models.ViewComponents
{
    public class UpdatePersonalInformationViewComponent : ViewComponent
    {
        private readonly Neo4jService _neo4jService;

        public UpdatePersonalInformationViewComponent(Neo4jService neo4jService)
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

            var user = await _neo4jService.GetPersonalInformationByIdAsync(id);

            if (user == null)
            {
                return View("UserNotFound"); 
            }

            return View(user);
        }
    }
}
