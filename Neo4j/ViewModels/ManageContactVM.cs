using System.ComponentModel.DataAnnotations;

namespace Neo4j.ViewModels
{
    public class ManageContactVM
    {
        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression(@"^\+?\d{1,4}?[-.\s]?\(?\d{1,3}?\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}$", ErrorMessage = "Please enter a valid phone number.")]
        public string phoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid emaild address")]
        public string email { get; set; }

        public ManageContactVM() { }
        
        public ManageContactVM(string phoneNumber, string email)
        {
            this.phoneNumber = phoneNumber;
            this.email = email;
        }
    }
}
