using Microsoft.CodeAnalysis.Scripting;
using BCrypt.Net;

namespace Neo4j.Services
{
    public class UserService
    {
        public string RegisterUser(string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            return hashedPassword;
        }
    }
}
