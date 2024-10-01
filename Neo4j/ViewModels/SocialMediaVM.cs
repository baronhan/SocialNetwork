using System.ComponentModel.DataAnnotations;

namespace Neo4j.ViewModels
{
    public class SocialMediaVM
    {
        [Url(ErrorMessage = "Invalid Facebook link format.")]
        public string facebookLink { get; set; }
        [Url(ErrorMessage = "Invalid Twitter link format.")]
        public string twitterLink {get; set; }  
        [Url(ErrorMessage = "Invalid Google link format.")]
        public string googleLink { get; set; }
        [Url(ErrorMessage = "Invalid Instagram link format.")]
        public string instagramLink { get; set; }
        [Url(ErrorMessage = "Invalid YouTube link format.")]
        public string youtubeLink {get; set; }

        public SocialMediaVM() { }
        public SocialMediaVM(string facebookLink, string twitterLink, string googleLink, string instagramLink, string youtubeLink)
        {
            this.facebookLink = facebookLink;
            this.twitterLink = twitterLink;
            this.googleLink = googleLink;
            this.instagramLink = instagramLink;
            this.youtubeLink = youtubeLink;
        }
    }
}
