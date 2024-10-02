using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.ViewModels;

namespace Neo4j.Controllers
{
    public class FilterController : Controller
    {

        private readonly Neo4jService _neo4jService;

        public FilterController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string gender, string city, string hobbies)
        {
            var userId = HttpContext.Session.GetString("id");

            var users = await _neo4jService.FilterUsersAsync(gender, city, hobbies);
            if (users == null)
            {
                users = new List<FilterVM>(); 
            }

            foreach (var user in users)
            {
                if (user.ID != null && user.ID.ToString() == userId)
                {
                    user.IsCurrentUser = true;
                }
                else
                {
                    var areFriends = await _neo4jService.AreFriendsAsync(userId, user.ID.ToString());
                    user.AreFriends = areFriends;
                }
            }

           // Truyền dữ liệu vào combobox
            var gendersCBO = await _neo4jService.GetGendersAsync();
            var citiesCBO = await _neo4jService.GetCitiesAsync();
            var hobbiesCBO = await _neo4jService.GetHobbiesAsync();

            var filterCBOVM = new FilterCBOVM
            {
                Users = users,
                Genders = gendersCBO,
                Cities = citiesCBO,
                Hobbies = hobbiesCBO,
                SelectedGender = gender,
                SelectedCity = city,
                SelectedHobbies = hobbies
            };

            return View(filterCBOVM);
        }
    }
}

