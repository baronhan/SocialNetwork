namespace Neo4j.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Firstname { get; set; } 
        public string? Lastname { get; set; } 
        public DateOnly Dob { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }     
        public string? Mobile { get; set; }   
        public string? Address { get; set; }  
        public string? Country { get; set; } 
        public string? City { get; set; }      
        public string? Gender { get; set; }   
        public List<string>? InterestedIn { get; set; }
        public List<string>? Languages { get; set; }    
        public string? ProfileDescription { get; set; } 
        public string? ProfileImage { get; set; }    
        public string? MaritalStatus { get; set; }      
        public string? FacebookLink { get; set; }     
        public string? TwitterLink { get; set; }       
        public string? GoogleLink { get; set; }       
        public string? InstagramLink { get; set; }    
        public string? YoutubeLink { get; set; }
        public int FriendsNum { get; set; }

        public UserModel() { }

        public UserModel(string id, string username, string password, DateOnly dob, int age,
                         string? firstname = null, string? lastname = null,
                         string? email = null, string? mobile = null,
                         string? address = null, string? country = null,
                         string? city = null, string? gender = null,
                         List<string>? interestedIn = null,
                         List<string>? languages = null,
                         string? profileDescription = null,
                         string? profileImage = null,
                         string? maritalStatus = null,
                         string? facebookLink = null,
                         string? twitterLink = null,
                         string? googleLink = null,
                         string? instagramLink = null,
                         string? youtubeLink = null)
        {
            Id = id;
            Username = username;
            Password = password;
            Dob = dob;
            Age = age;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Mobile = mobile;
            Address = address;
            Country = country;
            City = city;
            Gender = gender;
            InterestedIn = interestedIn ?? new List<string>();
            Languages = languages ?? new List<string>();
            ProfileDescription = profileDescription;
            ProfileImage = profileImage;
            MaritalStatus = maritalStatus;
            FacebookLink = facebookLink;
            TwitterLink = twitterLink;
            GoogleLink = googleLink;
            InstagramLink = instagramLink;
            YoutubeLink = youtubeLink;
            FriendsNum = 0;
        }

    }
}
