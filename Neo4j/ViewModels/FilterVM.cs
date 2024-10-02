namespace Neo4j.ViewModels
{
    public class FilterVM
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int FollowersCount { get; set; }
        public string ProfileImage { get; set; }
        public bool? AreFriends { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
