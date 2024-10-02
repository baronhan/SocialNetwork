using System.ComponentModel.DataAnnotations;

namespace Neo4j.ViewModels
{
    public class AccountSettingVM
    {
        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "User Name must be between 3 and 50 characters.")]
        public string userName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string email { get; set; }
        [Required(ErrorMessage = "At least one language must be selected.")]
        [MinLength(1, ErrorMessage = "Please select at least one language.")]
        public List<string> languages { get; set; }
        public AccountSettingVM() { }
        public AccountSettingVM(string userName, string email, List<string> languages)
        {
            this.userName = userName;
            this.email = email;
            this.languages = languages;
        }
    }
}
