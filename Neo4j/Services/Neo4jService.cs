using Neo4jClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4j.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Neo4j.Models;
using Microsoft.AspNetCore.Identity;
using Neo4j.Services;

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

        public async Task<IEnumerable<SearchVM>> SearchUsersAsync(string searchTerm, string userId)
        {
            var users = new List<SearchVM>();

            var query = @"
                            MATCH (u:User), (a:User {id: $userId})
                            WHERE u.username CONTAINS $searchTerm AND NOT exists((a)-[:blocked]->(u))
                            OPTIONAL MATCH (u)<-[:follows|:friend_with]-(follower:User)
                            RETURN u AS user, COUNT(DISTINCT follower) AS followersCount, u.profileImage AS ProfileImage, u.id AS ID 
                        ";


            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { searchTerm, userId });
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

        public async Task<string> GetUserIdByEmailAsync(string email)
        {
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync("MATCH (u:User {email: $email}) RETURN u.id AS id", new { email });

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
                   u.language AS Languages";
            
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
                WHERE NOT exists((u)-[:blocked]->(friend))
                OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                RETURN friend AS Friend, 
                COUNT(DISTINCT follower) + COUNT(DISTINCT friendOfFriend) AS FollowersCount,
                       friend.profileImage AS ProfileImage, friend.id as ID 
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
                    var _id = record["ID"].As<string>();

                    friendNode.Properties.TryGetValue("username", out var username);
                    friendNode.Properties.TryGetValue("city", out var city);

                    var user = new SearchVM
                    {
                        ID = _id.ToString(),
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

        internal async Task<IEnumerable<SearchVM>> RecentlyAddedFriendsByIdAsync(string id)
        {
            var users = new List<SearchVM>();

            var query = @"
                            MATCH (u:User {id: $id})-[r:friend_with]->(friend:User)
                            WHERE NOT exists((u)-[:blocked]->(friend))
                            OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                            OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                            WHERE duration.between(r.since, date()).days <= 7
                            RETURN friend AS Friend, 
                                   COUNT(DISTINCT follower) + COUNT(DISTINCT friendOfFriend) AS FollowersCount,
                                   friend.profileImage AS ProfileImage, friend.id as ID 
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
                        ID = id.ToString(),
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

        internal async Task<IEnumerable<SearchVM>> FriendsFromHometownByIdAsync(string id)
        {
            var users = new List<SearchVM>();

            var query = @"
                        MATCH (u:User {id: $id})-[:friend_with]->(friend:User)
                        WHERE NOT exists((u)-[:blocked]->(friend))
                        OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                        OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                        WHERE u.country IS NOT NULL AND u.country = friend.country
                        RETURN friend AS Friend, 
                               COUNT(DISTINCT follower) + COUNT(DISTINCT friendOfFriend) AS FollowersCount,
                               friend.profileImage AS ProfileImage, friend.id as ID 
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
                        ID = id.ToString(),
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

        public async Task<IEnumerable<SearchVM>> CloseFriendsByIdAsync(string id)
        {
            var users = new List<SearchVM>();

            var query = @"
                MATCH (u:User {id: $id})-[:close_friend]->(friend:User)
                WHERE NOT exists((u)-[:blocked]->(friend))
                OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                WHERE (u)-[:close_friend]->(friend)
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
                        ID = id.ToString(),
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
                       friend.profileImage AS ProfileImage, friend.id as ID 
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
            
            return users;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var token = Guid.NewGuid().ToString();

            var query = @"
                        MATCH (u:User {email: $email})
                        SET u.resetToken = $token, u.tokenCreatedAt = datetime()
                        RETURN u";

            using var session = _driver.AsyncSession();
            await session.RunAsync(query, new { email = email, token });

            return token;
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var validateTokenQuery = @"
                        MATCH (u:User {email: $email})
                        WHERE u.resetToken = $token AND u.tokenCreatedAt > datetime() - duration({hours: 24})
                        RETURN u";

            using var session = _driver.AsyncSession();

            var validationResult = await session.RunAsync(validateTokenQuery, new { email, token });

            var records = await validationResult.ToListAsync();
            if (!records.Any())
            {
                return IdentityResult.Failed(new IdentityError { Description = "Token không hợp lệ hoặc đã hết hạn." });
            }

            UserService _userService = new UserService();
            string hashedPassword = _userService.RegisterUser(newPassword);

            var updatePasswordQuery = @"
                        MATCH (u:User {email: $email})
                        SET u.password = $hashedPassword, u.resetToken = NULL, u.tokenCreatedAt = NULL
                        RETURN u";

            await session.RunAsync(updatePasswordQuery, new { email, hashedPassword });

            return IdentityResult.Success;
        }

        public async Task AddFriendAsync(string userId, string otherUserId)
        {
            var query = @"
                MATCH (a:User {id: $userId}), (b:User {id: $otherUserId})
                CREATE (a)-[:friend_request {status: 'pending'}]->(b)
                CREATE (a)-[:follows]->(b)";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                await result.ConsumeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<bool> IsFriendRequestPendingAsync(string userId, string otherUserId)
        {
            var query = @"
                        MATCH (u:User {id: $userId})-[r:friend_request]->(friend:User {id: $otherUserId})
                        WHERE r.status = 'pending'
                        RETURN r";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                return await result.FetchAsync();
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

        public async Task CancelFriendRequestAsync(string userId, string otherUserId)
        {
            var query = @"
                MATCH (a:User {id: $userId})-[r]->(b:User {id: $otherUserId})
                DELETE r";
            
            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                await result.ConsumeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task AcceptFriendRequestAsync(string userId, string otherUserId)
        {
            var query = @"
                MATCH (a:User {id: $userId})<-[fr:friend_request]-(b:User {id: $otherUserId})
                SET fr.status = 'accepted'
                CREATE (a)-[:follows]->(b)
                CREATE (a)-[:friend_with {since: date()}]->(b),
                       (b)-[:friend_with {since: date()}]->(a)
                DELETE fr";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                await result.ConsumeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task RejectFriendRequestAsync(string userId, string otherUserId)
        {
            var query = @"
                MATCH (a:User {id: $userId})<-[r]-(b:User {id: $otherUserId})
                DELETE r";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                await result.ConsumeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task Unfriend_UnblockAsync(string userId, string friendId)
        {
            var query = @"
                MATCH (u:User {id: $userId})-[r]-(friend:User {id: $friendId})
                DELETE r";

            var session = _driver.AsyncSession();
            try
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new { userId, friendId });
                    await result.ConsumeAsync();
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
        }

        public async Task UnfollowAsync(string userId, string otherUserId)
        {
            var query = @"
                MATCH (u:User {id: $userId})-[r:follows]->(friend:User {id: $otherUserId})
                DELETE r";

            var session = _driver.AsyncSession();
            try
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new { userId, otherUserId });
                    await result.ConsumeAsync();
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
        }

        public async Task FollowAsync(string userId, string otherUserId)
        {
            var query = @"
                MATCH (a:User {id: $userId}), (b:User {id: $otherUserId})
                WHERE NOT exists((a)-[:follows]->(b))
                CREATE (a)-[:follows]->(b)";

            var session = _driver.AsyncSession();
            try
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new { userId, otherUserId });
                    await result.ConsumeAsync();
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
        }

        public async Task BlockAsync(string userId, string friendId)
        {
            var query = @"
                        MATCH (u:User {id: $userId}), (friend:User {id: $friendId})
                        OPTIONAL MATCH (u)-[r]-(friend)
                        DELETE r
                        MERGE (u)-[:blocked]->(friend)
                        MERGE (friend)-[:is_blocked]->(u)";

            var session = _driver.AsyncSession();
            try
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new { userId, friendId });
                    await result.ConsumeAsync();
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
        }

        public async Task<IEnumerable<SearchVM>> BlockedListByIdAsync(string id)
        {
            var users = new List<SearchVM>();

            var query = @"
                MATCH (u:User {id: $id})-[:blocked]->(friend:User)
                OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                WHERE (u)-[:blocked]->(friend)
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
                        ID = id.ToString(),
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

        public async Task<bool?> AreCloseFriendsAsync(string userId, string otherUserId)
        {
            if (userId == otherUserId)
            {
                return null;
            }

            var query = @"
                        MATCH (u:User {id: $userId})-[:close_friend]->(friend:User {id: $otherUserId})
                        RETURN COUNT(friend) > 0 AS areCloseFriends";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                var record = await result.SingleAsync();
                return record["areCloseFriends"].As<bool>();
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

        public async Task<bool?> HasFollowedAsync(string userId, string otherUserId)
        {
            if (userId == otherUserId)
            {
                return null;
            }

            var query = @"
                        MATCH (u:User {id: $userId})-[:follows]->(friend:User {id: $otherUserId})
                        RETURN COUNT(friend) > 0 AS hasFollowed";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                var record = await result.SingleAsync();
                return record["hasFollowed"].As<bool>();
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

        public async Task<bool?> HasBlockedAsync(string userId, string otherUserId)
        {
            if (userId == otherUserId)
            {
                return null;
            }

            var query = @"
                        MATCH (u:User {id: $userId})-[:blocked]->(friend:User {id: $otherUserId})
                        RETURN COUNT(friend) > 0 AS hasBlocked";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                var record = await result.SingleAsync();
                return record["hasBlocked"].As<bool>();
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

        public async Task<bool?> HasSentFriendRequestAsync(string userId, string otherUserId)
        {
            if (userId == otherUserId)
            {
                return null;
            }

            var query = @"
                        MATCH (u:User {id: $userId})-[:friend_request]->(friend:User {id: $otherUserId})
                        RETURN COUNT(friend) > 0 AS sent";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                var record = await result.SingleAsync();
                return record["sent"].As<bool>();
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

        public async Task<bool?> HasReceivedFriendRequestAsync(string userId, string otherUserId)
        {
            if (userId == otherUserId)
            {
                return null;
            }

            var query = @"
                        MATCH (u:User {id: $userId})<-[:friend_request]-(friend:User {id: $otherUserId})
                        RETURN COUNT(friend) > 0 AS received";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                var record = await result.SingleAsync();
                return record["received"].As<bool>();
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

        public async Task<SearchVM> GetFriendByIdAsync(string friendId)
        {
            var query = @"
                MATCH (f:User)
                WHERE f.id = $friendId
                RETURN f.id AS ID, f.name AS Name, f.city AS City, f.followersCount AS FollowersCount, f.profileImage AS ProfileImage";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.ExecuteReadAsync(async tx =>
                {
                    var cursor = await tx.RunAsync(query, new { friendId });
                    var record = await cursor.SingleAsync();

                    return new SearchVM
                    {
                        ID = record["ID"].As<string>(),
                        Name = record["Name"].As<string>(),
                        City = record["City"].As<string>(),
                        FollowersCount = record["FollowersCount"].As<int>(),
                        ProfileImage = record["ProfileImage"].As<string>()
                    };
                });
                return result;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<SearchVM>> GetFriendRequestsAsync(string userId)
        {
            var users = new List<SearchVM>();

            var query = @"
                MATCH (u:User {id: $id})<-[:friend_request]-(friend:User)
                OPTIONAL MATCH (friend)<-[:follows]-(follower:User)
                OPTIONAL MATCH (friend)<-[:friend_with]-(friendOfFriend:User)
                RETURN friend AS Friend, 
                       COUNT(DISTINCT follower) + COUNT(DISTINCT friendOfFriend) AS FollowersCount,
                       friend.profileImage AS ProfileImage, u.id as ID
                ";


            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId });
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
                        ID = id.ToString(),
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

        public async Task CloseFriendAsync(string userId, string otherUserId)
        {
            var query = @"
                MATCH (a:User {id: $userId})-[:friend_with]->(b:User {id: $otherUserId})
                CREATE (a)-[:close_friend {since: date()}]->(b)";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                await result.ConsumeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task RemoveCloseFriendAsync(string userId, string otherUserId)
        {
            var query = @"
                MATCH (a:User {id: $userId})-[r:close_friend]->(b:User {id: $otherUserId})
                DELETE r";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, otherUserId });
                await result.ConsumeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<List<UserModel>> GetUsersByUsernameAsync(string username)
        {
            var query = @"
            MATCH (u:User)
            WHERE u.username STARTS WITH $username
            RETURN u.username AS Username, u.firstname AS Firstname, u.lastname AS Lastname, u.profileImage AS ProfileImage";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { username });
                var users = new List<UserModel>();

                await result.ForEachAsync(record =>
                {
                    users.Add(new UserModel
                    {
                        Username = record["Username"].As<string>(),
                        Firstname = record["Firstname"].As<string>(),
                        Lastname = record["Lastname"].As<string>(),
                        ProfileImage = record["ProfileImage"].As<string>()
                    });
                });

                return users;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<bool> AddFamilyMemberAsync(string userId, string familyUsername, string relationship)
        {
            var query = @"
                       MATCH (u1:User {id: $userId}), (u2:User {username: $familyUsername})
                       RETURN u2";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, familyUsername });
                if (result != null)
                {
                    var createRelationshipQuery = @"
                                          MATCH (u1:User {id: $userId}), (u2:User {username: $familyUsername})
                                          MERGE (u1)<-[:IS_FAMILY {relationship: $relationship}]-(u2)";
                    await session.RunAsync(createRelationshipQuery, new { userId, familyUsername, relationship });
                    return true;
                }
                return false;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<List<FamilyMemberVM>> GetFamilyMembersAsync(string userId)
        {
            var query = @"
            MATCH (u1:User {id: $userId})-[r:IS_FAMILY]->(u2:User)
            RETURN u2.firstname AS Firstname, u2.lastname AS Lastname, u2.profileImage AS ProfileImage, r.relationship AS Relationship";

                var session = _driver.AsyncSession();
                try
                {
                    var result = await session.RunAsync(query, new { userId });
                    var familyMembers = new List<FamilyMemberVM>();

                    await result.ForEachAsync(record =>
                    {
                        var familyMember = new FamilyMemberVM
                        {
                            User = new UserModel
                            {
                                Firstname = record["Firstname"].As<string>(),
                                Lastname = record["Lastname"].As<string>(),
                                ProfileImage = record["ProfileImage"].As<string>(),
                            },
                            Relation = record["Relationship"].As<string>()
                        };
                        familyMembers.Add(familyMember);
                    });

                    return familyMembers;
                }
                finally
                {
                    await session.CloseAsync();
                }
        }

        public async Task<bool> AddFamilyRequestAsync(string userId, string familyUsername, string relationship)
        {
            var query = @"
                MATCH (u1:User {id: $userId}), (u2:User {username: $familyUsername})
                MERGE (u1)-[:FAMILY_REQUEST {relationship: $relationship, status: 'pending'}]->(u2)
                RETURN u1, u2";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId, familyUsername, relationship });
                return result != null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<bool> ConfirmFamilyRequestAsync(string requestId)
        {
            var query = @"
                    MATCH (requester:User {id: $requestId})-[r:FAMILY_REQUEST]->(u:User)
                    WHERE r.status = 'pending'
                    SET r.status = 'confirmed'
                    RETURN r";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { requestId });
                return result != null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }


        public async Task<bool> DeleteFamilyRequestAsync(string requestId)
        {
            var query = @"
                    MATCH (u1:User {id: $requestId})-[r:FAMILY_REQUEST]->(u2:User)
                    DELETE r
                    RETURN COUNT(r) = 0";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { requestId });
                return result != null;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<List<FamilyRequestVM>> GetFamilyRequestsAsync(string userId)
        {
            var query = @"
            MATCH (u:User {id: $userId})<-[r:FAMILY_REQUEST]-(requester:User)
            RETURN requester.id AS requesterId, requester.username AS requesterName, requester.profileImage AS requesterProfileImage, r.relationship AS requesterRelationship";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query, new { userId });
                var requests = new List<FamilyRequestVM>();

                await result.ForEachAsync(record =>
                {
                    var familyRequest = new FamilyRequestVM
                    {
                        RequesterId = record["requesterId"].As<string>(),
                        RequesterName = record["requesterName"].As<string>(),
                        RequesterProfileImage = record["requesterProfileImage"].As<string>(),
                        RequesterRelationship = record["requesterRelationship"].As<string>()
                    };
                    requests.Add(familyRequest);
                });

                return requests;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<IEnumerable<string>> GetGendersAsync()
        {
            var genders = new List<string>();

            var query = @"
                  MATCH (u:User)
                  RETURN DISTINCT u.gender AS Gender";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query);
                await result.ForEachAsync(record =>
                {
                    var gender = record["Gender"].As<string>();
                    genders.Add(gender);
                });
            }
            finally
            {
                await session.CloseAsync();
            }

            return genders;
        }

        public async Task<IEnumerable<string>> GetCitiesAsync()
        {
            var cities = new List<string>();

            var query = @"
                  MATCH (u:User)
                  RETURN DISTINCT u.city AS City";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query);
                await result.ForEachAsync(record =>
                {
                    var city = record["City"].As<string>();
                    cities.Add(city);
                });
            }
            finally
            {
                await session.CloseAsync();
            }

            return cities;
        }

        public async Task<IEnumerable<string>> GetMaritalStaAsync()
        {
            var maritals = new List<string>();

            var query = @"
                  MATCH (u:User)
                  RETURN DISTINCT u.maritalStatus AS MaritalStatus";

            var session = _driver.AsyncSession();
            try
            {
                var result = await session.RunAsync(query);
                await result.ForEachAsync(record =>
                {
                    var marital = record["MaritalStatus"].As<string>();
                    maritals.Add(marital);
                });
            }
            finally
            {
                await session.CloseAsync();
            }

            return maritals;
        }

        public async Task<IEnumerable<FilterVM>> FilterUsersAsync(string gender, string city, string maritalStatus, string userId)
        {
            var users = new List<FilterVM>();
            var session = _driver.AsyncSession();
            try
            {
                var query = "MATCH (u:User) ";

                var conditions = new List<string>();

                if (!string.IsNullOrEmpty(gender) && gender != "all")
                {
                    conditions.Add("u.gender = $gender");
                }

                if (!string.IsNullOrEmpty(city) && city != "all")
                {
                    conditions.Add("u.city = $city");
                }

                if (!string.IsNullOrEmpty(maritalStatus) && maritalStatus != "all")
                {
                    conditions.Add("u.maritalStatus = $maritalStatus");
                }

                conditions.Add($"NOT (u)<-[:blocked]-(:User {{id: $userId}})");

                if (conditions.Count > 0)
                {
                    query += "WHERE " + string.Join(" AND ", conditions) + " ";
                }

                query += "OPTIONAL MATCH (u)<-[:follows|:friend_with]-(follower:User) ";
                query += "RETURN u AS user, COUNT(DISTINCT follower) AS followersCount, u.profileImage AS ProfileImage, u.id AS ID";

                Console.WriteLine($"Cypher Query: {query}");
                Console.WriteLine($"Parameters: gender={gender}, city={city}, maritalStatus={maritalStatus}, userId={userId}");

                var result = await session.RunAsync(query, new { gender, city, maritalStatus, userId });

                await result.ForEachAsync(record =>
                {
                    var userNode = record["user"].As<INode>();
                    var followersCount = record["followersCount"].As<int>();
                    var profileImage = record["ProfileImage"].As<string>();
                    var id = record["ID"].As<string>();

                    userNode.Properties.TryGetValue("username", out var username);
                    userNode.Properties.TryGetValue("city", out var cityProperty);
                    userNode.Properties.TryGetValue("gender", out var genderProperty);
                    userNode.Properties.TryGetValue("maritalStatus", out var maritalProperty);

                    if (id == null)
                    {
                        return; 
                    }

                    var user = new FilterVM
                    {
                        ID = id,
                        Name = username?.ToString(),
                        City = cityProperty?.ToString(),
                        Gender = genderProperty?.ToString(),
                        MaritalStatus = maritalProperty?.ToString(),
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
