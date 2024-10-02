namespace Neo4j.ViewModels
{
    public class FilterCBOVM
    {
        public IEnumerable<FilterVM> Users { get; set; }
        public IEnumerable<string> Genders { get; set; }
        public IEnumerable<string> Cities { get; set; }
        public IEnumerable<string> Hobbies { get; set; }
        public string SelectedGender { get; set; }
        public string SelectedCity { get; set; }
        public string SelectedHobbies { get; set; }
    }
}
