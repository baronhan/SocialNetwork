using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        public async Task<IActionResult> Index(string gender, string city, string maritalStatus)
        {
            var userId = HttpContext.Session.GetString("id");

            var gendersCBO = await _neo4jService.GetGendersAsync();
            var citiesCBO = await _neo4jService.GetCitiesAsync();
            var maritalsCBO = await _neo4jService.GetMaritalStaAsync();
            var users = await _neo4jService.FilterUsersAsync(gender, city, maritalStatus);

            var filterCBOVM = new FilterCBOVM
            {
                Users = users,
                Genders = gendersCBO,
                Cities = citiesCBO,
                Maritals = maritalsCBO,
                SelectedGender = gender,
                SelectedCity = city,
                SelectedMaritals = maritalStatus
            };
          
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
            return View(filterCBOVM);
        }
    }
}
