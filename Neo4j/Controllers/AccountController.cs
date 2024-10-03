using Microsoft.AspNetCore.Mvc;
using Neo4j.ViewModels;
using Neo4j.Services;
using MyMVCApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Neo4j.Controllers
{
    public class AccountController : Controller
    {
        private readonly Neo4jService _neo4jService;
        private readonly IEmailSender _emailSender;

        public AccountController(Neo4jService neo4jService, IEmailSender emailSender)
        {
            _neo4jService = neo4jService;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _neo4jService.EmailExistsAsync(model.Email))
            { 
                var token = await _neo4jService.GeneratePasswordResetTokenAsync(model.Email);
                var resetpwUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "Reset Your Password", resetpwUrl);

                TempData["Email"] = model.Email;

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View();
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            var email = TempData["Email"]?.ToString();

            ViewBag.Email = email;

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError(string.Empty, "Yêu cầu không hợp lệ.");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _neo4jService.EmailExistsAsync(model.Email))
            {
                var result = await _neo4jService.ResetPasswordAsync(model.Email, model.Token, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
            return View();
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
