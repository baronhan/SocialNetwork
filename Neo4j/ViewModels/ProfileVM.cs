namespace Neo4j.ViewModels
{
    public class ProfileVM
    {
        public string ID { get; set; }
        public string username { get; set; }
        public string profileImage { get; set; }
        public int followers { get; set; }
        public int following { get; set; }
        public string facebookLink { get; set; }
        public string twitterLink { get; set; }
        public string googleLink { get; set; }
        public string instagramLink { get; set; }
        public string youtubeLink { get; set; }

        public ProfileVM() { }
        public ProfileVM(string ID, string username, string profileImage, int followers, int following, string facebookLink, string twitterLink, string googleLink, string instagramLink, string youtubeLink)
        {
            this.ID = ID;
            this.username = username;
            this.profileImage = profileImage;
            this.followers = followers;
            this.following = following;
            this.facebookLink = facebookLink;
            this.twitterLink = twitterLink;
            this.googleLink = googleLink;
            this.instagramLink = instagramLink;
            this.youtubeLink = youtubeLink;
        }
    }
}
