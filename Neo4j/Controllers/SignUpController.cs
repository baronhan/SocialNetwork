using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.Services;
using Neo4j.ViewModels;

namespace Neo4j.Controllers
{
    public class SignUpController : Controller
    {
        private readonly Neo4jService _neo4jService;

        public SignUpController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if(ModelState.IsValid)
            {
                if(await _neo4jService.EmailExistsAsync(model.Email)) 
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại. Vui lòng sử dụng một email khác.");
                }
                else
                {
                    var userService = new UserService();
                    string pass = userService.RegisterUser(model.Password);


                    string userId = await _neo4jService.CreateUserAsync(model.Name, model.Email, pass, model.DateOfBirth, model.Gender);


                    HttpContext.Session.SetString("id", userId);

                    
                    return RedirectToAction("Index", "Home");
                }    
            }    

            return View();
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!await _neo4jService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            string enteredPassword = model.Password;
            string? storedHashedPassword = await _neo4jService.GetHashedPassword(model.Email);

            if (storedHashedPassword != null)
            {
                var userService = new UserService();
                bool isPasswordCorrect = userService.VerifyPassword(enteredPassword, storedHashedPassword);

                if (!isPasswordCorrect)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(model);
                }

             
                HttpContext.Session.SetString("email", model.Email);
                string username = await _neo4jService.GetUserName(model.Email);
                HttpContext.Session.SetString("username", username);

              
                string userId = await _neo4jService.GetUserIdByEmailAsync(model.Email);
                HttpContext.Session.SetString("id", userId);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }


        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("SignIn", "SignUp");
        }
    }
}
