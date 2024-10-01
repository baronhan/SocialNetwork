namespace Neo4j.ViewModels
{
    public class DetailAboutVM
    {
        public string firstName { get; set; }
        public string lastName { get; set; }    
        public int age { get; set; }    
        public string country { get; set; }
        public string userName { get; set; }

        public DetailAboutVM() { }
        public DetailAboutVM(string firstName, string lastName, int age, string country, string userName)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.age = age;
            this.country = country;
            this.userName = userName;
        }
    }
}
