using Neo4jClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4j.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace MyMVCApp.Services
{
    public class Neo4jService
    {
        private readonly IDriver _driver;

        public Neo4jService(string uri, string username, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
        }

        public async Task<string> CreateUserAsync(string username, string email, string password, DateOnly dob, string gender,
                           string? firstname = null, string? lastname = null, string? mobile = null,
                           string? address = null, string? country = null, string? city = null,
                           List<string>? interestedIn = null, List<string>? languages = null,
                           string? profileDescription = null, string? profileImage = null,
                           string? maritalStatus = null, string? facebookLink = null,
                           string? twitterLink = null, string? googleLink = null,
                           string? instagramLink = null, string? youtubeLink = null)
        {
            var session = _driver.AsyncSession();
            try
            {
                var id = Guid.NewGuid().ToString();

                int age = DateTime.Today.Year - dob.Year;
                if (dob > DateOnly.FromDateTime(DateTime.Today)) age--;

                var result = await session.RunAsync(
                    @"CREATE (u:User {
                        id: $id,
                        username: $username,
                        password: $password,
                        email: $email,
                        dob: $dob,
                        gender: $gender,
                        firstname: coalesce($firstname, '') ,
                        lastname: coalesce($lastname, '') ,
                        age: $age,
                        mobile: coalesce($mobile, '') ,
                        address: coalesce($address, '') ,
                        country: coalesce($country, '') ,
                        city: coalesce($city, '') ,
                        interestedIn: coalesce($interestedIn, []) ,
                        languages: coalesce($languages, []) ,
                        profileDescription: coalesce($profileDescription, '') ,
                        profileImage: coalesce($profileImage, '') ,
                        maritalStatus: coalesce($maritalStatus, '') ,
                        facebookLink: coalesce($facebookLink, '') ,
                        twitterLink: coalesce($twitterLink, '') ,
                        googleLink: coalesce($googleLink, '') ,
                        instagramLink: coalesce($instagramLink, '') ,
                        youtubeLink: coalesce($youtubeLink, '')
                    }) RETURN u.id AS id",
                    new
                    {
                        id,
                        username,
                        password,
                        email,
                        dob,
                        gender,
                        firstname,
                        lastname,
                        age,
                        mobile,
                        address,
                        country,
                        city,
                        interestedIn,
                        languages,
                        profileDescription,
                        profileImage,
                        maritalStatus,
                        facebookLink,
                        twitterLink,
                        googleLink,
                        instagramLink,
                        youtubeLink
                    });

                var record = await result.SingleAsync();
                return record["id"].As<string>();
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync("MATCH (u:User {email: $email}) RETURN u", new { email });

                return await result.FetchAsync(); 
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<SearchVM>> SearchUsersAsync(string searchTerm)
        {
            var users = new List<SearchVM>();

            var query = @"
                            MATCH (u:User)
                            WHERE u.username CONTAINS $searchTerm
                            OPTIONAL MATCH (u)<-[:follows|:friend_with]-(follower:User)
                            RETURN u AS user, COUNT(DISTINCT follower) AS followersCount, u.profileImage AS ProfileImage, u.id AS ID 
                        ";


            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { searchTerm });
                await result.ForEachAsync(record =>
                {
                    var userNode = record["user"].As<INode>();
                    var followersCount = record["followersCount"].As<int>();
                    var profileImage = record["ProfileImage"].As<string>();
                    var id = record["ID"].As<string>();

                    userNode.Properties.TryGetValue("username", out var username);
                    userNode.Properties.TryGetValue("city", out var city);

                    var user = new SearchVM
                    {
                        ID = id?.ToString(),
                        Name = username?.ToString(), 
                        City = city?.ToString(), 
                        FollowersCount = followersCount,
                        ProfileImage = profileImage
                    };

                    users.Add(user);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }


            return users;
        }

        public async Task<bool?> AreFriendsAsync(string userId, string otherUserId)
        {
            if (userId == otherUserId)
            {
                return null;
            }

            var query = @"
                        MATCH (u:User {id: $userId})-[:friend_with]->(friend:User {id: $otherUserId})
                        RETURN COUNT(friend) > 0 AS areFriends";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                var record = await result.SingleAsync();
                return record["areFriends"].As<bool>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
            finally
            {
                await session.CloseAsync();
            }
        }


        public async Task<string?> GetHashedPassword(string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync("MATCH (u:User {email: $email}) RETURN u.password AS password", new { email });

                if (await result.FetchAsync())
                {
                    return result.Current["password"].As<string>();
                }
                return null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<string?> GetHashedPasswordById(string id)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync("MATCH (u:User {id: $id}) RETURN u.password AS password", new { id });

                if (await result.FetchAsync())
                {
                    return result.Current["password"].As<string>();
                }
                return null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<string> GetUserName(string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync("MATCH (u:User {email: $email}) RETURN u.username AS username", new { email });

                if (await result.FetchAsync())
                {
                    return result.Current["username"].As<string>();
                }
                return null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<string?> GetUserIdByEmailAsync(string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(
                    @"MATCH (u:User {email: $email})
              RETURN u.id AS id",
                    new { email });

                var records = await result.ToListAsync();
                return records.Count > 0 ? records[0]["id"].As<string>() : null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<UpdatePersonalInformationVM?> GetPersonalInformationByIdAsync(string id)
        {
            UpdatePersonalInformationVM user = null;
            var query = @"
            MATCH (u:User {id: $id})
            RETURN u.firstname AS Firstname,
                    u.lastname AS Lastname,
                    u.username AS Username,
                    u.address AS Address,
                    u.country AS Country,
                    u.city AS City,
                    u.gender AS Gender,
                    u.dob AS Dob,
                    u.age AS Age,
                    u.maritalStatus AS MaritalStatus,
                    u.profileDescription AS ProfileDescription,
                    u.profileImage AS ProfileImage
            ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                var records = await result.ToListAsync();

                if (records.Count > 0)
                {
                    var record = records.First();
                    user = new UpdatePersonalInformationVM
                    {
                        Firstname = record["Firstname"].As<string>(),
                        Lastname = record["Lastname"].As<string>(),
                        Username = record["Username"].As<string>(),
                        Address = record["Address"].As<string>(),
                        Country = record["Country"].As<string>(),
                        City = record["City"].As<string>(),
                        Gender = record["Gender"].As<string>(),
                        Dob = DateOnly.Parse(record["Dob"].As<string>()),
                        Age = record["Age"].As<int>(),
                        MaritalStatus = record["MaritalStatus"].As<string>(),
                        ProfileDescription = record["ProfileDescription"].As<string>(),
                        ProfileImage = record["ProfileImage"].As<string>()
                    };
                }
            }
            finally
            {
                await session.CloseAsync();
            }

            return user;
        }

        public async Task<bool> UpdatePersonalInformationAsync(UpdatePersonalInformationVM model)
        {
            var query = @"
            MATCH (u:User {username: $username})
            SET u.firstname = $firstname,
                u.lastname = $lastname,
                u.address = $address,
                u.country = $country,
                u.city = $city,
                u.gender = $gender,
                u.dob = $dob,
                u.age = $age,
                u.maritalStatus = $maritalStatus,
                u.profileDescription = $profileDescription,
                u.profileImage = $profileImage
            RETURN u
            ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new
                {
                    username = model.Username,
                    firstname = model.Firstname,
                    lastname = model.Lastname,
                    address = model.Address,
                    country = model.Country,
                    city = model.City,
                    gender = model.Gender,
                    dob = model.Dob.ToString("yyyy-MM-dd"), 
                    age = model.Age,
                    maritalStatus = model.MaritalStatus,
                    profileDescription = model.ProfileDescription,
                    profileImage = model.ProfileImage
                });

               
                return await result.ConsumeAsync() != null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<string?> GetUserNameByIdAsync(string id)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(
                    @"MATCH (u:User {id: $id})
              RETURN u.username AS username",
                    new { id });

                var records = await result.ToListAsync();
                return records.Count > 0 ? records[0]["username"].As<string>() : null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<string?> GetCurrentProfileImageAsync(string username)
        {
            await using var session = _driver.AsyncSession();

            try
            {
                
                var query = "MATCH (u:User {username: $username}) RETURN u.profileImage AS profileImage";
                var result = await session.RunAsync(query, new { username });

               
                var records = await result.ToListAsync();

                
                return records.FirstOrDefault()?["profileImage"]?.As<string>();
            }
            finally
            {
              
                await session.CloseAsync();
            }

        }

        public async Task<bool> UpdatePasswordAsync(string id, string newPassword)
        {
            var session = _driver.AsyncSession();
            try
            {
                return await session.WriteTransactionAsync(async tx =>
                {
                    var query = "MATCH (u:User {id: $id}) SET u.password = $newPassword RETURN u";
                    var result = await tx.RunAsync(query, new { id, newPassword });

                    if (await result.FetchAsync())
                    {
                        return true;
                    }

                    return false;
                });
            }
            finally
            {
                await session.CloseAsync(); 
            }
        }

        public async Task<bool> UpdateContactInformationAsync(string id, string phoneNumber, string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                return await session.WriteTransactionAsync(async tx =>
                {
                    var query = "MATCH (u:User {id: $id}) SET u.mobile = $phoneNumber, u.email = $email RETURN u";
                    var result = await tx.RunAsync(query, new { id, phoneNumber, email });

                    if(await result.FetchAsync())
                    {
                        return true;
                    }

                    return false;
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<ManageContactVM?> GetContactInformationByIdAsync(string id)
        {
            ManageContactVM user = null;
            var query = @"
            MATCH (u:User {id: $id})
            RETURN u.mobile AS PhoneNumber,
                    u.email AS Email
            ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                var records = await result.ToListAsync();

                if (records.Count > 0)
                {
                    var record = records.First();
                    user = new ManageContactVM
                    {
                        phoneNumber = record["PhoneNumber"].As<string>(),
                        email = record["Email"].As<string>(),
                    };
                }
            }
            finally
            {
                await session.CloseAsync();
            }

            return user;
        }

        public async Task<AccountSettingVM> GetAccountSettingByIdAsync(string id)
        {
            AccountSettingVM user = null;

            var query = @"
            MATCH (u:User {id: $id})
            RETURN u.username AS UserName,
                   u.email AS Email,
                   u.language AS Languages

            ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                var records = await result.ToListAsync();

                if (records.Count > 0)
                {
                    var record = records.First();
                    user = new AccountSettingVM
                    {
                        userName = record["UserName"].As<string>(),
                        email = record["Email"].As<string>(),
                        languages = record["Languages"].As<List<string>>()
                    };
                }
            }
            finally
            {
                await session.CloseAsync();
            }

            return user;
        }

        public async Task<bool> UpdateAccountSettingAsync(string id, string username, string email, List<string> languages)
        {
            var session = _driver.AsyncSession();
            try
            {
                return await session.WriteTransactionAsync(async tx =>
                {
                    var query = @"
                    MATCH (u:User {id: $id})
                    WITH u, u.language AS currentLanguages, $languages AS newLanguages
                    SET u.username = $username,
                        u.email = $email,
                        u.language = CASE 
                            WHEN currentLanguages IS NULL THEN newLanguages 
                            ELSE currentLanguages + [lang IN newLanguages WHERE NOT lang IN currentLanguages]
                        END
                    RETURN u";



                    var result = await tx.RunAsync(query, new { id, username, email, languages });

                    if (await result.FetchAsync())
                    {
                        return true;
                    }

                    return false;
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<SocialMediaVM> GetSocialMediaByIdAsync(string id)
        {
            SocialMediaVM user = null;

            var query = @"
            MATCH (u:User {id: $id})
            RETURN u.facebookLink AS Facebook,
                   u.googleLink AS Google,
                   u.instagramLink AS Instagram,
                   u.twitterLink AS Twitter,
                   u.youtubeLink AS Youtube
            ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                var records = await result.ToListAsync();

                if (records.Count > 0)
                {
                    var record = records.First();
                    user = new SocialMediaVM
                    {
                        facebookLink = record["Facebook"].As<string>(),
                        googleLink = record["Google"].As<string>(),
                        instagramLink = record["Instagram"].As<string>(),
                        twitterLink = record["Twitter"].As<string>(),
                        youtubeLink = record["Youtube"].As<string>()
                    };
                }
            }
            finally
            {
                await session.CloseAsync();
            }

            return user;
        }

        public async Task<bool> UpdateSocialMediaAsync(string id, string facebook, string google, string instagram, string twitter, string youtube)
        {
            var session = _driver.AsyncSession();
            try
            {
                return await session.WriteTransactionAsync(async tx =>
                {
                    var query = "MATCH (u:User {id: $id}) SET u.facebookLink = $facebook, u.googleLink = $google, u.instagramLink = $instagram, u.twitterLink = $twitter, u.youtubeLink = $youtube RETURN u";
                    var result = await tx.RunAsync(query, new { id, facebook, google, instagram, twitter, youtube });

                    if (await result.FetchAsync())
                    {
                        return true;
                    }

                    return false;
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<ProfileVM> GetProfileByIdAsync(string id)
        {
            ProfileVM user = null;

            var query = @"
                        MATCH (u:User {id: $id})
                        OPTIONAL MATCH (u)-[:friend_with]->(f:User)
                        OPTIONAL MATCH (u)-[:follows]->(follow:User)
                        RETURN u.profileImage AS ProfileImage,
                               u.username AS Username,
                               u.facebookLink AS Facebook,
                               u.googleLink AS Google,
                               u.instagramLink AS Instagram,
                               u.twitterLink AS Twitter,
                               u.youtubeLink AS Youtube,
                               count(f) AS followers,
                               count(follow) AS following, 
                               u.id AS ID
                        ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                var records = await result.ToListAsync();

                if (records.Count > 0)
                {
                    var record = records.First();
                    user = new ProfileVM
                    {
                        username = record["Username"].As<string>(),
                        profileImage = record["ProfileImage"].As<string>(),
                        facebookLink = record["Facebook"].As<string>(),
                        googleLink = record["Google"].As<string>(),
                        instagramLink = record["Instagram"].As<string>(),
                        twitterLink = record["Twitter"].As<string>(),
                        youtubeLink = record["Youtube"].As<string>(),
                        followers = record["followers"].As<int>(), 
                        following = record["following"].As<int>(),
                        ID = record["ID"].As<string>()
                    };
                }
            }
            finally
            {
                await session.CloseAsync();
            }

            return user;
        }

        public async Task<ContactInformationVM> GetContactAndBasicInfoByIdAsync(string id)
        {
            ContactInformationVM user = null;

            var query = @"
                        MATCH (u:User {id: $id})
                        RETURN u.instagramLink AS Instagram,
                               u.address AS Address,
                               u.email AS Email,                
                               u.mobile AS Mobile,             
                               u.dob AS Dob,                    
                               u.gender AS Gender,             
                               u.language AS Languages                               
                    ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                var records = await result.ToListAsync();

                if (records.Count > 0)
                {
                    var record = records.First();
                    user = new ContactInformationVM
                    {
                        email = record["Email"].As<string>(),
                        address = record["Address"].As<string>(),
                        mobile = record["Mobile"].As<string>(),
                        instagramLink = record["Instagram"].As<string>(),
                        Dob = DateOnly.Parse(record["Dob"].As<string>()),
                        gender = record["Gender"].As<string>(),
                        languages = record["Languages"].As<List<string>>()
                    };

                }
            }
            finally
            {
                await session.CloseAsync();
            }

            return user;
        }

        public async Task<DetailAboutVM> GetDetailAboutByIdAsync(string id)
        {
            DetailAboutVM user = null;

            var query = @"
                        MATCH (u:User {id: $id})
                        RETURN u.firstname AS FirstName,
                               u.lastname AS LastName,
                               u.age AS Age,                
                               u.country AS Country,             
                               u.username AS Username
                    ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                var records = await result.ToListAsync();

                if (records.Count > 0)
                {
                    var record = records.First();
                    user = new DetailAboutVM
                    {
                        userName = record["Username"].As<string>(),
                        country = record["Country"].As<string>(),
                        age = record["Age"].As<int>(),
                        lastName = record["LastName"].As<string>(),
                        firstName = record["FirstName"].As<string>()
                    };

                }
            }
            finally
            {
                await session.CloseAsync();
            }

            return user;
        }

        public async Task<IEnumerable<SearchVM>> FriendListByIdAsync(string id)
        {
            var users = new List<SearchVM>();

            var query = @"
                MATCH (u:User {id: $id})-[:friend_with]->(friend:User)
                OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                RETURN friend AS Friend, 
                       COUNT(DISTINCT follower) + COUNT(DISTINCT friendOfFriend) AS FollowersCount,
                       friend.profileImage AS ProfileImage, u.id as ID 
            ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                await result.ForEachAsync(record =>
                {
                    var friendNode = record["Friend"].As<INode>();
                    var followersCount = record["FollowersCount"].As<int>();
                    var profileImage = record["ProfileImage"].As<string>();
                    var id = record["ID"].As<string>();

                    friendNode.Properties.TryGetValue("username", out var username);
                    friendNode.Properties.TryGetValue("city", out var city);

                    var user = new SearchVM
                    {
                        ID = id?.ToString(),
                        Name = username?.ToString(),
                        City = city?.ToString(),
                        FollowersCount = followersCount,
                        ProfileImage = profileImage 
                    };

                    users.Add(user);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }

            return users;
        }

        internal async Task<IEnumerable<SearchVM>?> RecentlyAddedFriendsByIdAsync(string id)
        {
            var users = new List<SearchVM>();

            var query = @"
                            MATCH (u:User {id: $id})-[r:friend_with]->(friend:User)
                            OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                            OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                            WHERE duration.between(r.since, date()).days <= 7
                            RETURN friend AS Friend, 
                                   COUNT(DISTINCT follower) + COUNT(DISTINCT friendOfFriend) AS FollowersCount,
                                   friend.profileImage AS ProfileImage, u.id as ID 
                        ";



            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                await result.ForEachAsync(record =>
                {
                    var friendNode = record["Friend"].As<INode>();
                    var followersCount = record["FollowersCount"].As<int>();
                    var profileImage = record["ProfileImage"].As<string>();
                    var id = record["ID"].As<string>();

                    friendNode.Properties.TryGetValue("username", out var username);
                    friendNode.Properties.TryGetValue("city", out var city);

                    var user = new SearchVM
                    {
                        ID = id?.ToString(),
                        Name = username?.ToString(),
                        City = city?.ToString(),
                        FollowersCount = followersCount,
                        ProfileImage = profileImage
                    };

                    users.Add(user);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }

            return users;
        }

        internal async Task<IEnumerable<SearchVM>?> FriendsFromHometownByIdAsync(string id)
        {
            var users = new List<SearchVM>();

            var query = @"
                        MATCH (u:User {id: $id})-[:friend_with]->(friend:User)
                        OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                        OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                        WHERE u.country = friend.country
                        RETURN friend AS Friend, 
                               COUNT(DISTINCT follower) + COUNT(DISTINCT friendOfFriend) AS FollowersCount,
                               friend.profileImage AS ProfileImage, u.id as ID 
                    ";



            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                await result.ForEachAsync(record =>
                {
                    var friendNode = record["Friend"].As<INode>();
                    var followersCount = record["FollowersCount"].As<int>();
                    var profileImage = record["ProfileImage"].As<string>();
                    var id = record["ID"].As<string>();

                    friendNode.Properties.TryGetValue("username", out var username);
                    friendNode.Properties.TryGetValue("city", out var city);

                    var user = new SearchVM
                    {
                        ID = id?.ToString(),
                        Name = username?.ToString(),
                        City = city?.ToString(),
                        FollowersCount = followersCount,
                        ProfileImage = profileImage
                    };

                    users.Add(user);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }

            return users;
        }

        public async Task<IEnumerable<SearchVM>> FriendListTimeLineByIdAsync(string id)
        {
            var users = new List<SearchVM>();

            var query = @"
                MATCH (u:User {id: $id})-[:friend_with]->(friend:User)
                OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                RETURN friend AS Friend, 
                       COUNT(DISTINCT follower) + COUNT(DISTINCT friendOfFriend) AS FollowersCount,
                       friend.profileImage AS ProfileImage, u.id as ID 
                LIMIT 9
            ";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { id });
                await result.ForEachAsync(record =>
                {
                    var friendNode = record["Friend"].As<INode>();
                    var followersCount = record["FollowersCount"].As<int>();
                    var profileImage = record["ProfileImage"].As<string>();
                    var id = record["ID"].As<string>();

                    friendNode.Properties.TryGetValue("username", out var username);
                    friendNode.Properties.TryGetValue("city", out var city);

                    var user = new SearchVM
                    {
                        ID = id?.ToString(),
                        Name = username?.ToString(),
                        City = city?.ToString(),
                        FollowersCount = followersCount,
                        ProfileImage = profileImage 
                    };

                    users.Add(user);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }

            return users;
        }
    }
}
