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

        public IActionResult RedirectToProfile(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                HttpContext.Session.SetString("UserId", id); 
                return RedirectToAction("Profile");
            }

            return RedirectToAction("SignIn", "SignUp");
        }


        public async Task<IActionResult> Profile()
        {
            ProfileVM user = null;

            var userId = HttpContext.Session.GetString("UserId");
            var sessionId = HttpContext.Session.GetString("id"); 

            if (string.IsNullOrEmpty(userId))
            {
                userId = sessionId; 
            }

            if (!string.IsNullOrEmpty(userId))
            {
                user = await _neo4jService.GetProfileByIdAsync(userId);
                HttpContext.Session.Remove("UserId");

                if (user != null)
                {
                    user.isYour = (user.ID == sessionId); 
                }
            }
            else
            {
                return RedirectToAction("SignIn", "SignUp");
            }

            return View(user);
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


        public IActionResult AccountSetting()
        {
            var id = HttpContext.Session.GetString("id");

            if (id == null)
            {
                return RedirectToAction("SignIn", "SignUp");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AccountSetting(AccountSettingVM model)
        {
            if (ModelState.IsValid)
            {
                var id = HttpContext.Session.GetString("id");

                if (string.IsNullOrEmpty(id))
                {
                    ModelState.AddModelError("", "User session expired or not found.");
                    return RedirectToAction("SignIn", "SignUp");
                }

                bool result = await _neo4jService.UpdateAccountSettingAsync(id, model.userName, model.email, model.languages);

                if (result)
                {
                    TempData["SuccessMessage"] = "Account settings updated successfully!";
                    return RedirectToAction("AccountSetting");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update the contact information. Please try again.");
                }
            }   
            
            return View("AccountSetting");
        }

        [HttpPost]
        public async Task<IActionResult> SocialMedia(SocialMediaVM model)
        {
            if (ModelState.IsValid)
            {
                var id = HttpContext.Session.GetString("id");

                if (string.IsNullOrEmpty(id))
                {
                    ModelState.AddModelError("", "User session expired or not found.");
                    return RedirectToAction("SignIn", "SignUp");
                }

                bool result = await _neo4jService.UpdateSocialMediaAsync(id, model.facebookLink, model.googleLink, model.instagramLink, model.twitterLink, model.youtubeLink);

                if (result)
                {
                    TempData["SuccessMessage"] = "Social Media updated successfully!";
                    return RedirectToAction("AccountSetting");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update the contact information. Please try again.");
                }
            }

            return View("AccountSetting");
        }

        //[HttpPost]
        //public IActionResult UpdateFriendship(string otherUserId, string actionType)
        //{

        //    var userId = HttpContext.Session.GetString("UserId");

        //    switch (actionType)
        //    {
        //        case "addFriend":
        //            _neo4jService.AddFriendAsync(userId, otherUserId);
        //            break;
        //        case "unfriend":
        //            _neo4jService.Unfriend_UnblockAsync(userId, otherUserId);
        //            break;
        //        case "follow":
        //            _neo4jService.FollowAsync(userId, otherUserId);
        //            break;
        //        case "unfollow":
        //            _neo4jService.UnfollowAsync(userId, otherUserId);
        //            break;
        //        case "closeFriend":
        //            //_neo4jService.AddFriendAsync(userId, otherUserId);
        //            break;
        //        case "removeCloseFriend":
        //            //_neo4jService.AddFriendAsync(userId, otherUserId);
        //            break;
        //        case "block":
        //            _neo4jService.BlockAsync(userId, otherUserId);
        //            break;
        //        case "unblock":
        //            _neo4jService.Unfriend_UnblockAsync(userId, otherUserId);
        //            break;
        //        case "cancelRequest":
        //            _neo4jService.CancelFriendRequestAsync(userId, otherUserId);
        //            break;
        //        case "accept":
        //            _neo4jService.AcceptFriendRequestAsync(userId, otherUserId);
        //            break;
        //        case "reject":
        //            _neo4jService.RejectFriendRequestAsync(userId, otherUserId);
        //            break;
        //    }

        //    return Json(new { success = true });
        //}

        //public IActionResult RefreshTab(string listType)
        //{
        //    var userId = HttpContext.Session.GetString("UserId");

        //    switch (listType)
        //    {
        //        case "FriendList":
        //            return ViewComponent("FriendListViewComponent", new { listType = "FriendList", userId = userId });
        //        case "RecentlyFriendList":
        //            return ViewComponent("FriendListViewComponent", new { listType = "RecentlyFriendList", userId = userId });
        //        case "CloseFriendList":
        //            return ViewComponent("FriendListViewComponent", new { listType = "CloseFriendList", userId = userId });
        //        case "MutualHometownFriendList":
        //            return ViewComponent("FriendListViewComponent", new { listType = "MutualHometownFriendList", userId = userId });
        //        case "BlockedList":
        //            return ViewComponent("FriendListViewComponent", new { listType = "BlockedList", userId = userId });
        //        default:
        //            return ViewComponent("FriendListViewComponent", new { listType = "FriendList", userId = userId });
        //    }
        //}

        //public IActionResult LoadViewComponent()
        //{
        //    return ViewComponent("FriendListViewComponent");
        //}

    }
}
