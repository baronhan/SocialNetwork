using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;

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
        public async Task<IActionResult> Register(string name, string email, string password)
        {
            if(ModelState.IsValid)
            {
                //Gọi dịch vụ Neo4j để tạo người dùng
                await _neo4jService.CreateUserAsync(name, email, password);

                //Lưu thông tin đăng nhập vào session
                HttpContext.Session.SetString("username",  name);

                //Điều hướng về trang Home
                return RedirectToAction("Index", "Home");
            }    

            return View();
        }
    }
}
