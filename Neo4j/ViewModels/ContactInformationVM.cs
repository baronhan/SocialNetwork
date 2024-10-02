namespace Neo4j.ViewModels
{
    public class ContactInformationVM
    {
        public string email { get; set; }
        public string mobile { get; set; }
        public string address { get; set; }
        public string instagramLink { get; set; }
        public DateOnly Dob { get; set; }
        public string gender { get; set; }
        public List<string> languages { get; set; }

        public ContactInformationVM () { }

        public ContactInformationVM(string email, string mobile, string address, string instagramLink, DateOnly dob, string gender, List<string> languages)
        {
            this.email = email;
            this.mobile = mobile;
            this.address = address;
            this.instagramLink = instagramLink;
            Dob = dob;
            this.gender = gender;
            this.languages = languages;
        }
    } 
}
