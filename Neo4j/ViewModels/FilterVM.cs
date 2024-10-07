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
        public bool? AreCloseFriends { get; set; }   
        public bool? HasFollowed { get; set; }        
        public bool? Blocked { get; set; }          
        public bool? FriendRequestSent { get; set; } 
        public bool? FriendRequestReceived { get; set; } 
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
    }
}
