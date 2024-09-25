namespace Neo4j.ViewModels
{
    public class UpdatePersonalInformationVM
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string Username { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Gender { get; set; }
        public DateOnly Dob { get; set; }
        public int Age { get; set; }
        public string? MaritalStatus { get; set; }
        public string? ProfileDescription { get; set; }
        public string? ProfileImage { get; set; }
    }
}
