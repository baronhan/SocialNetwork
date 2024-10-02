using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.Services;
using Neo4j.ViewModels;

namespace Neo4j.Controllers
{
    public class UserController : Controller
    {
        private readonly Neo4jService _neo4jService;

        public UserController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        public async Task<IActionResult> Profile()
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

            string email = HttpContext.Session.GetString("email");
            var friends = await _neo4jService.GetFriendsAsync(email);
            return View(friends);
        }

        public IActionResult EditProfile()
        {
            var username = HttpContext.Session.GetString("username");

            if (username == null)
            {
                return RedirectToAction("SignIn", "SignUp");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePersonalInformation(UpdatePersonalInformationVM model, IFormFile? profileImage)
        {
            if (ModelState.IsValid)
            {
                
                if (model.Username != null) 
                {
                    var currentImage = await _neo4jService.GetCurrentProfileImageAsync(model.Username);
                    model.ProfileImage = currentImage; 
                }

                
                if (profileImage != null && profileImage.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images/user");
                    var fileName = Path.GetFileName(profileImage.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    model.ProfileImage = $"/assets/images/user/{fileName}";
                }

      
                bool isUpdated = await _neo4jService.UpdatePersonalInformationAsync(model);
                if (isUpdated)
                {
                    TempData["SuccessMessage"] = "Information updated successfully!";
                    return RedirectToAction("EditProfile", "User");
                }
                else
                {
                    ModelState.AddModelError("", "Update failed, please try again.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            }

            return View("EditProfile", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var id = HttpContext.Session.GetString("id");

                if (string.IsNullOrEmpty(id))
                {
                    ModelState.AddModelError("", "User session expired or not found.");
                    return RedirectToAction("SignIn", "SignUp");
                }

                string? storedHashedPassword = await _neo4jService.GetHashedPasswordById(id);

                if (storedHashedPassword == null)
                {
                    ModelState.AddModelError("", "Failed to retrieve stored password.");
                    return View("EditProfile", model);
                }

                var userService = new UserService();
                bool isPasswordCorrect = userService.VerifyPassword(model.currentPassword, storedHashedPassword);

                if (!isPasswordCorrect)
                {
                    ModelState.AddModelError("", "Invalid current password.");
                    return View("EditProfile", model);
                }

                string hashedNewPassword = userService.RegisterUser(model.newPassword);

              
                bool result = await _neo4jService.UpdatePasswordAsync(id, hashedNewPassword);

                if (result)
                {
                    return RedirectToAction("SignIn", "SignUp");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update the password. Please try again.");
                }
            }
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            }


            return View("EditProfile", model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageContact(ManageContactVM model)
        {
            if (ModelState.IsValid)
            {
                var id = HttpContext.Session.GetString("id");

                if (string.IsNullOrEmpty(id))
                {
                    ModelState.AddModelError("", "User session expired or not found.");
                    return RedirectToAction("SignIn", "SignUp");
                }

                bool result = await _neo4jService.UpdateContactInformationAsync(id, model.phoneNumber, model.email);

                if (result)
                {
                    return RedirectToAction("EditProfile"); 
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update the contact information. Please try again.");
                }
            }

            return View("EditProfile");
        }


    }
}
