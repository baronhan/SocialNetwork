using Microsoft.AspNetCore.Mvc;
using MyMVCApp.Services;
using Neo4j.ViewModels;

namespace Neo4j.Models.ViewComponents
{
    public class FriendListViewComponent : ViewComponent
    {
        private readonly Neo4jService _neo4jService;

        public FriendListViewComponent(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string listType, string userId)
        {
            var id = userId ?? HttpContext.Session.GetString("id");

            if (string.IsNullOrEmpty(userId))
            {
                return View("Error");
            }

            IEnumerable<SearchVM> users = new List<SearchVM>();

            switch (listType)
            {
                case "FriendList":
                    users = await _neo4jService.FriendListByIdAsync(id); 
                    break;
                case "RecentlyFriendList":
                    users = await _neo4jService.RecentlyAddedFriendsByIdAsync(id); 
                    break;
                case "CloseFriendList":
                    users = await _neo4jService.CloseFriendsByIdAsync(id); 
                    break;
                case "MutualHometownFriendList":
                    users = await _neo4jService.FriendsFromHometownByIdAsync(id); 
                    break;
            }

            if (users == null)
            {
                return View("UserNotFound");
            }

            return View("Default", users);
        }

    }
}
