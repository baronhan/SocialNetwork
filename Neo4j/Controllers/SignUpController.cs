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

                    //Gọi dịch vụ Neo4j để tạo người dùng
                    await _neo4jService.CreateUserAsync(model.Name, model.Email, pass, model.DateOfBirth, model.Gender);

                    //Lưu thông tin đăng nhập vào session
                    HttpContext.Session.SetString("username", model.Name);

                    //Điều hướng về trang Home
                    return RedirectToAction("Index", "Home");
                }    
            }    

            return View();
        }
    }
}
