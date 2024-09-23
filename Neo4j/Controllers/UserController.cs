using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;

namespace Neo4j.Controllers
{
    public class UserController : Controller
    {
        private readonly Neo4jService _neo4jService;

        public UserController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("username");

            if (username != null)
            {
                ViewBag.Username = username;
            }
            else
            {
                return RedirectToAction("SignIn", "SignUp");
            }

            return View();
        }

        [HttpPost]
        public IActionResult PersonalInformation()
        {
            var username = HttpContext.Session.GetString("username");

            if (username != null)
            {
                ViewBag.Username = username;
            }
            else
            {
                return RedirectToAction("SignIn", "SignUp");
            }

            return View();
        }
        
        public IActionResult EditProfile()
        {
            var username = HttpContext.Session.GetString("username");

            if (username != null)
            {
                ViewBag.Username = username;
            }
            else
            {
                return RedirectToAction("SignIn", "SignUp");
            }

            return View();
        }

        [HttpPost]
        public IActionResult UpdatePersonalInformation()
        {
            var username = HttpContext.Session.GetString("username");
            return View();
        }
    }
}
