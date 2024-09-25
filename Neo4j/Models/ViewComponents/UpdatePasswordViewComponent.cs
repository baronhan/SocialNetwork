using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;

namespace Neo4j.Models.ViewComponents
{
    public class UpdatePasswordViewComponent : ViewComponent
    {
        private readonly Neo4jService _neo4jService;

        public UpdatePasswordViewComponent(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {    

            return View();

        }
    }
}
