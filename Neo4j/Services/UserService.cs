using Microsoft.CodeAnalysis.Scripting;
using BCrypt.Net;

namespace Neo4j.Services
{
    public class UserService
    {
        public UserService() { }

        public string RegisterUser(string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            return hashedPassword;
        }

        public bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHashedPassword);
        }
    }
}
