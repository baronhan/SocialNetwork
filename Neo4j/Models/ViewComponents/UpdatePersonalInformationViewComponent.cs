using Microsoft.AspNetCore.Mvc;

namespace Neo4j.Models.ViewComponents
{
    public class UpdatePersonalInformationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
