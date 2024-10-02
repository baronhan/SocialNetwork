using System.ComponentModel.DataAnnotations;

namespace Neo4j.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
