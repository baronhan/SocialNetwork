using System.ComponentModel.DataAnnotations;

namespace Neo4j.ViewModels
{
    public class SignInVM
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid emaild address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$", ErrorMessage = "Password must be at least 6 characters long and contain uppercase letters, lowercase letters, digits, and special characters.")]
        public string Password { get; set; }
    }
}
