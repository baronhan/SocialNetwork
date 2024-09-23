using Neo4jClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4j.ViewModels;

namespace MyMVCApp.Services
{
    public class Neo4jService
    {
        private readonly IDriver _driver;

        public Neo4jService(string uri, string username, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
        }

        public async Task CreateUserAsync(string username, string email, string password, DateOnly dob, string gender)
        {
            var session = _driver.AsyncSession();
            try
            {
                await session.RunAsync("CREATE (u:User {username: $username, email: $email, password: $password, dob: $dob, gender: $gender}) RETURN u", 
                    new { username, email, password, dob, gender });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync("MATCH (u:User {email: $email}) RETURN u", new { email });

                return await result.FetchAsync(); 
            }
            finally
            {
                await session.CloseAsync();
            }
        }


        public async Task<IEnumerable<SearchVM>> SearchUsersAsync(string searchTerm)
        {
            var users = new List<SearchVM>();

            var query = @"
                MATCH (u:User)
                WHERE u.username CONTAINS $searchTerm
                OPTIONAL MATCH (u)<-[:follows]-(follower:User)
                RETURN u AS user, COUNT(follower) AS followersCount
                ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { searchTerm });
                await result.ForEachAsync(record =>
                {
                    var userNode = record["user"].As<INode>();
                    var followersCount = record["followersCount"].As<int>();

                    // Kiểm tra và lấy giá trị của các thuộc tính
                    userNode.Properties.TryGetValue("username", out var username);
                    userNode.Properties.TryGetValue("city", out var city);

                    var user = new SearchVM
                    {
                        ID = userNode.Id,
                        Name = username?.ToString(), 
                        City = city?.ToString(), 
                        FollowersCount = followersCount 
                    };

                    users.Add(user);
                });
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu cần
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }


            return users;
        }

        public async Task<string?> GetHashedPassword(string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync("MATCH (u:User {email: $email}) RETURN u.password AS password", new { email });

                if (await result.FetchAsync())
                {
                    return result.Current["password"].As<string>();
                }
                return null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<string> GetUserName(string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync("MATCH (u:User {email: $email}) RETURN u.username AS username", new { email });

                if (await result.FetchAsync())
                {
                    return result.Current["username"].As<string>();
                }
                return null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

    }
}
