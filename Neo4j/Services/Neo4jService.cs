using Neo4jClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace MyMVCApp.Services
{
    public class Neo4jService
    {
        private readonly IDriver _driver;

    public Neo4jService(string uri, string username, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
    }

    // Hàm tạo user
    public async Task CreateUserAsync(string username, string email, string password)
    {
        var session = _driver.AsyncSession();
        try
        {
            await session.RunAsync("CREATE (u:User {username: $username, email: $email, password: $password}) RETURN u", 
                new { username, email, password });
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    // Hàm kiểm tra user có tồn tại không (cho đăng nhập sau này)
    public async Task<bool> UserExistsAsync(string username, string password)
    {
        var session = _driver.AsyncSession();
        try
        {
            var result = await session.RunAsync("MATCH (u:User {username: $username, password: $password}) RETURN u", 
                new { username, password });

            return await result.FetchAsync() != null;
        }
        finally
        {
            await session.CloseAsync();
        }
    }
    }
}
