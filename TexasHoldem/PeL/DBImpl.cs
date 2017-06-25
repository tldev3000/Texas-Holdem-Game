﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Security.Cryptography;
using Backend.User;
using Database.Repositories;
using System.Drawing;
using System.IO;

namespace Database 
{
    public class DBImpl : IDB
    {
        int SALT_SIZE = 16;
        string connectionString;
        
        private byte[] getRandomSalt()
        {
            var salt = new byte[SALT_SIZE];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }
            return salt; 
        }

        ISystemUserRepository systemUserRepository;
        public DBImpl(){
            //this.connectionString = Properties.Settings.Default.TablesConnectionString;
            //Console.WriteLine("Database path: " + this.connectionString);

            systemUserRepository = new SystemUserRepository();
        }
        
        //public DataTable uploadSystemUser()
        //{
        //    Console.WriteLine("Database path: " + this.connectionString);
        //    DataTable systemUserTable = new DataTable();
        //    string queryTable = "SELECT * FROM SystemUsers";
        //    using (connection = new SqlConnection(connectionString))
        //    using (adapter = new SqlDataAdapter(queryTable, connection))
        //    {
        //        adapter.Fill(systemUserTable);

        //        //userList = systemUserTable;
        //    }

        //    return systemUserTable;
        //}

        ///// <summary>
        ///// check if user with that name exists
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns>true if the user with that name exists</returns>
        //public bool isUserExist(string name)
        //{
        //    SqlConnection connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();
        //    SqlDataReader reader;

        //    cmd.CommandText = "SELECT Id FROM SystemUsers WHERE UserName = @name LIMIT 1";
        //    cmd.CommandType = CommandType.Text;
        //    cmd.Connection = connection;
        //    cmd.Parameters.AddWithValue("@name", name);

        //    connection.Open();
        //    reader = cmd.ExecuteReader();
        //    bool ans = reader.HasRows;
        //    connection.Close();
        //    return ans;
        //}

        //var systemUser = new Database.Domain.SystemUser { UserName = "Apple", Image = "Fruits", Email = "cookie", Password = Encoding.UTF8.GetBytes("Password"), Salt = Encoding.UTF8.GetBytes("Salt") };
        //ISystemUserRepository repository = new SystemUserRepository();
        //repository.Add(systemUser);
        //    Database.Domain.SystemUser user = repository.GetByName("Apple");
        //user.Email = "notACookie";
        //    repository.Update(user);
        //    Database.Domain.SystemUser user2 = repository.GetById(user.Id);
        //repository.Remove(user2);

        public List<object> getLeaderboardsByParam(string param)
        {
            IList<Database.Domain.SystemUser> list = systemUserRepository.GetByRestrictions(new Dictionary<string,string>(), param, false, 20);

            var leaderBoardInfo = new List<object>();
            foreach(Database.Domain.SystemUser user in list)
            {
                var newRow = new
                {
                    highestCash = user.HighestCashInGame,
                    playerName = user.UserName,
                    totalGrossProfit = user.TotalGrossProfit,
                    gamesPlayed = user.GamesPlayed
                };

                leaderBoardInfo.Add(newRow);
            }
            return leaderBoardInfo;
            //SqlConnection connection    = new SqlConnection(connectionString);
            //SqlCommand cmd              = new SqlCommand();
            //SqlDataReader reader;
            //cmd.CommandText             = "SELECT highetsCashInAGame, userName, gamesPlayed, totalGrossProfit " +
            //                              "FROM SystemUsers order by @param desc limit 20";
            //cmd.CommandType             = CommandType.Text;
            //cmd.Connection              = connection;

            //cmd.Parameters.AddWithValue("@param", param);

            //connection.Open();
            //reader                      = cmd.ExecuteReader();
            //var leaderBoardInfo         = new List<object> ();
            //while (reader.Read())
            //{
            //    var newRow = new
            //    {
            //        highestCash         = (int)reader["highetsCashInAGame"],
            //        playerName          = (string)reader["userName"],
            //        totalGrossProfit    = (int)reader["totalGrossProfit"],
            //        gamesPlayed         = (int)reader["gamesPlayed"]
            //    };

            //    leaderBoardInfo.Add(newRow);
            //}
            //connection.Close();
            //return leaderBoardInfo;
        }

        /// <summary>
        /// Register a new user to the system.
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="image"></param>
        /// <returns>true if the user has been added</returns>
        public void RegisterUser(string UserName, string password, string email, Image image)
        {
            string filePath = String.Join("_", Guid.NewGuid(), UserName);
            string imagesDirectory = Path.Combine(Environment.CurrentDirectory, "Images", filePath);

            // Save image to disc. (produces error but saves it anyway. we will just wrap it with a 'try' clause.
            try
            {
                image.Save(imagesDirectory);
            }
            catch { }


            Domain.SystemUser user = new Domain.SystemUser();
            user.Salt = generateSalt();
            user.Password = GetMd5Hash(password + user.Salt);
            user.Email = email;
            user.Image = imagesDirectory;

            systemUserRepository.Add(user);
            
            
            ////password = GetMd5Hash(string.Concat(new string[] { password, salt }));
            //SqlConnection connection = new SqlConnection(connectionString);
            //SqlCommand cmd = new SqlCommand();

            //cmd.CommandText = "INSERT SystemUsers (UserName,password,email,image,salt) " +
            //                        "VALUES (@UserName,HASHBYTES(\'MD5\', CONCAT(@password,@salt)),@email,@image,@salt)";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = connection;
            //cmd.Parameters.AddWithValue("@UserName", UserName);
            //cmd.Parameters.AddWithValue("@password", password);
            //cmd.Parameters.AddWithValue("@email", email);
            //cmd.Parameters.AddWithValue("@image", image);
            //cmd.Parameters.AddWithValue("@salt", getRandomSalt());

            //connection.Open();
            //bool ans = cmd.ExecuteNonQuery() > 0;
            //connection.Close();
            //return ans;
        }

        /// <summary>
        /// Edit user profile by ID, if you don't want to change some of the fields just put null there.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="UserName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="image"></param>
        /// <param name="moneyToAdd">a delta, can also be negative</param>
        /// <param name="rankToAdd">a delta, can also be negative</param>
        /// <param name="playedAnotherGame"></param>
        /// <returns>true if user has been edited succesfully</returns>
        public void EditUserById(int Id, string UserName, string password, string email, string image, int? moneyToAdd, int? rankToAdd, bool playedAnotherGame)
        {
            Database.Domain.SystemUser user = systemUserRepository.GetById(Id);
            if (UserName != null)
                user.UserName = UserName;
            if(password != null)
            {
                user.Salt = generateSalt();
                user.Password = GetMd5Hash(password + user.Salt);
            }
            if (email != null)
                user.Email = email;
            if (image != null)
                user.Email = email;
            if (moneyToAdd != null)
                user.Money = Math.Max(0, user.Money + (int) moneyToAdd);
            if(rankToAdd != null)
                user.Rank = Math.Max(0, user.Rank + (int)rankToAdd);
            if (playedAnotherGame)
                user.GamesPlayed++;

            systemUserRepository.Update(user);

            //SqlConnection connection = new SqlConnection(connectionString);
            //SqlCommand cmd = new SqlCommand();
            //int psikCount = -1 +
            //(UserName == null ? 0 : 1) +
            //(password == null ? 0 : 1) +
            //(email == null ? 0 : 1) +
            //(image == null ? 0 : 1) +
            //(money == null ? 0 : 1) +
            //(rankToAdd == null ? 0 : 1) +
            //(playedAnotherGame ? 1 : 0);

            //cmd.CommandText = "Update SystemUsers SET " +
            //    (UserName == null ? "" : "UserName=@UserName" + (psikCount-- > 0 ? "," : "")) +
            //    (password == null ? "" : "password=HASHBYTES(\'MD5\', CONCAT(@password,@salt)),salt=@salt" + (psikCount-- > 0 ? "," : "")) +
            //    (email == null ? "" : "email=@email" + (psikCount-- > 0 ? "," : "")) +
            //    (image == null ? "" : "image=@image" + (psikCount-- > 0 ? "," : "")) +
            //    (money == null ? "" : "money=money+@money" + (psikCount-- > 0 ? "," : "")) +
            //    (rankToAdd == null ? "" : "rank=(CASE WHEN rank+@rankToAdd > 0 THEN rank+@rankToAdd ELSE 0 END)" + (psikCount-- > 0 ? "," : "")) +
            //    (!playedAnotherGame ? "" : "gamesPlayed=gamesPlayed+1") +
            //     " WHERE Id=@Id";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = connection;
            //cmd.Parameters.AddWithValue("@Id", Id);
            //if (UserName != null) cmd.Parameters.AddWithValue("@UserName", UserName);
            //if (password != null) cmd.Parameters.AddWithValue("@password", password);
            //if (email != null) cmd.Parameters.AddWithValue("@email", email);
            //if (image != null) cmd.Parameters.AddWithValue("@image", image);
            //if (password != null) cmd.Parameters.AddWithValue("@salt", getRandomSalt());
            //if (money != null) cmd.Parameters.AddWithValue("@money", money);
            //if (rankToAdd != null) cmd.Parameters.AddWithValue("@rankToAdd", rankToAdd);


            //connection.Open();
            //bool ans = cmd.ExecuteNonQuery() > 0;
            //connection.Close();
            //return ans;
        }
        public void EditUserLeaderBoardsById(int Id, int? highestCashInGame, int? totalGrossProfit)
        {
            Database.Domain.SystemUser user = systemUserRepository.GetById(Id);
            if (highestCashInGame != null)
                user.HighestCashInGame = Math.Max(user.HighestCashInGame, (int) highestCashInGame);
            if (totalGrossProfit != null)
                user.TotalGrossProfit += (int) totalGrossProfit;
            
            systemUserRepository.Update(user);

            //SqlConnection connection = new SqlConnection(connectionString);
            //SqlCommand cmd = new SqlCommand();
            //int psikCount = -1 +
            //(highetsCashInAGame == null ? 0 : 1) +
            //(totalGrossProfit == null ? 0 : 1);

            //cmd.CommandText = "Update SystemUsers SET " +
            //    (highetsCashInAGame == null ? "" : "highetsCashInAGame=" +
            //    "(CASE WHEN highetsCashInAGame<@highetsCashInAGame " +
            //    "THEN @highetsCashInAGame ELSE highetsCashInAGame " +
            //    "END)" + (psikCount-- > 0 ? "," : "")) +
            //    (totalGrossProfit == null ? "" : "totalGrossProfit= totalGrossProfit+@totalGrossProfit" + (psikCount-- > 0 ? "," : "")) +
            //    " WHERE Id=@Id";

            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = connection;
            //cmd.Parameters.AddWithValue("@Id", Id);
            //if (highetsCashInAGame != null) cmd.Parameters.AddWithValue("@highetsCashInAGame", highetsCashInAGame);
            //if (totalGrossProfit != null) cmd.Parameters.AddWithValue("@totalGrossProfit", totalGrossProfit);

            //connection.Open();
            //bool ans = cmd.ExecuteNonQuery() > 0;
            //connection.Close();
            //return ans;
        }
        /// <summary>
        /// Login mechanism
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="password"></param>
        /// <returns>if success returns the id of the user, else returns -1</returns>
        public int Login(string UserName, string password)
        {
            Database.Domain.SystemUser user = systemUserRepository.GetByName(UserName);
            if (user == null)
                return -1;

            if (VerifyMd5Hash(password + user.Salt, user.Password))
                return user.Id;

            return -1;
            
            //SqlConnection connection = new SqlConnection(connectionString);
            //SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            //cmd.CommandText = "SELECT Id FROM SystemUsers WHERE UserName=@UserName AND password=HASHBYTES(\'MD5\', CONCAT(@password,salt))";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = connection;
            //cmd.Parameters.AddWithValue("@UserName", UserName);
            //cmd.Parameters.AddWithValue("@password", password);

            //connection.Open();
            //reader = cmd.ExecuteReader();
            //if (!reader.HasRows || !reader.Read())
            //    return -1;

            //int ans = (int)reader["Id"];
            //connection.Close();
            //return ans;
        }

        public List<Object> getUsersDetails()
        {
            IList<Database.Domain.SystemUser> list =
                systemUserRepository.GetByRestrictions(new Dictionary<string, string>(), null, false, null);

            List<object> userDetails = new List<object>();
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT highetsCashInAGame, userName, totalGrossProfit FROM SystemUsers";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;

            connection.Open();
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var details = new
                {
                    HighestCash = (int)reader["highetsCashInAGame"],
                    playerName = (string)reader["userName"],
                    grossProfit = (int)reader["totalGrossProfit"]
                };
                userDetails.Add(details);
            }
            connection.Close();
            return userDetails;


            //List<object> userDetails = new List<object>();
            //SqlConnection connection = new SqlConnection(connectionString);
            //SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            //cmd.CommandText = "SELECT highetsCashInAGame, userName, totalGrossProfit FROM SystemUsers";
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = connection;

            //connection.Open();
            //reader = cmd.ExecuteReader();
            //while (reader.Read())
            //{
            //    var details = new
            //    {
            //        HighestCash = (int)reader["highetsCashInAGame"],
            //        playerName = (string)reader["userName"],
            //        grossProfit = (int)reader["totalGrossProfit"]
            //    };
            //    userDetails.Add(details);
            //}
            //connection.Close();
            //return userDetails;
        }


        public List<SystemUser> getAllSystemUsers()
        {
            List<SystemUser> users = new List<SystemUser>();
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT Id,UserName,email,image,money,rank,gamesPlayed FROM SystemUsers";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;

            connection.Open();
            reader = cmd.ExecuteReader();
            while (reader.Read())
                users.Add(new SystemUser(int.Parse(reader["Id"].ToString()), reader["UserName"].ToString(), reader["email"].ToString(), reader["image"].ToString(), int.Parse(reader["money"].ToString()), int.Parse(reader["rank"].ToString()), int.Parse(reader["gamesPlayed"].ToString())));
            
            connection.Close();
            return users;
        }
        public SystemUser getUserById(int Id)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT UserName,email,image,money,rank,gamesPlayed FROM SystemUsers WHERE Id=@Id";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);

            connection.Open();
            reader = cmd.ExecuteReader();
            if (!reader.HasRows || !reader.Read())
                return null;
            SystemUser su = new SystemUser(Id, reader["UserName"].ToString(), reader["email"].ToString(), reader["image"].ToString(), int.Parse(reader["money"].ToString()), int.Parse(reader["rank"].ToString()), int.Parse(reader["gamesPlayed"].ToString()));
            
            connection.Close();

            // Try to get the image from the database.
            try
            {
                // Get the user's profile picture file from memory.
                var returnedImage = Image.FromFile(su.userImage);

                // Convert user's profile picture into byte array in order to send over TCP 
                su.userImageByteArray = imageToByteArray(returnedImage);
            }
            catch { }

            return su;
        }
        public SystemUser getUserByName(string name)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT Id,email,image,money,rank,gamesPlayed FROM SystemUsers WHERE UserName=@UserName";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@UserName", name);

            connection.Open();

            reader = cmd.ExecuteReader();
            if (!reader.HasRows || !reader.Read())
                return null;
            //            string s = reader["email"].ToString();
            SystemUser su = new SystemUser(int.Parse(reader["Id"].ToString()), name, reader["email"].ToString(), reader["image"].ToString(), int.Parse(reader["money"].ToString()), int.Parse(reader["rank"].ToString()), int.Parse(reader["gamesPlayed"].ToString()));

            connection.Close();

            // Try to get the image from the database.
            try
            {
                // Get the user's profile picture file from memory.
                var returnedImage = Image.FromFile(su.userImage);

                // Convert user's profile picture into byte array in order to send over TCP 
                su.userImageByteArray = imageToByteArray(returnedImage);
            }
            catch { }
            
            return su;
        }
        public SystemUser getUserByEmail(string email)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT Id,UserName,image,money,rank,gamesPlayed FROM SystemUsers WHERE email=@email";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@email", email);

            connection.Open();
            reader = cmd.ExecuteReader();
            if (!reader.HasRows || !reader.Read())
                return null;
            SystemUser su = new SystemUser(int.Parse(reader["Id"].ToString()), reader["UserName"].ToString(), email, reader["image"].ToString(), int.Parse(reader["money"].ToString()), int.Parse(reader["rank"].ToString()), int.Parse(reader["gamesPlayed"].ToString()));

            connection.Close();
            // Try to get the image from the database.
            try
            {
                // Get the user's profile picture file from memory.
                var returnedImage = Image.FromFile(su.userImage);

                // Convert user's profile picture into byte array in order to send over TCP 
                su.userImageByteArray = imageToByteArray(returnedImage);
            }
            catch { }
            return su;
        }
        public bool deleteUser(int Id)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "DELETE FROM SystemUsers WHERE Id=@Id";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", Id);

            connection.Open();
            bool ans = cmd.ExecuteNonQuery() > 0;
            connection.Close();
            return ans;
        }
        public bool deleteUsers()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "DELETE FROM SystemUsers";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;

            connection.Open();
            bool ans = cmd.ExecuteNonQuery() > 0;
            connection.Close();
            return ans;
        }
        //public void editUserName(int userID, string userName)
        //{
        //    string queryUpdate = "UPDATE SystemUsers SET UserName = @UserName " +
        //                          "WHERE Id = @userID ";
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand command = new SqlCommand(queryUpdate, connection);
        //    using (connection)
        //    using (command)
        //    {
        //        connection.Open();

        //        command.Parameters.AddWithValue("@userID", userID);
        //        command.Parameters.AddWithValue("@UserName", userName);

        //        command.ExecuteScalar();
        //    }

        //}

        //public string getEnterMessage(string stringCommand)
        //{
        //    string ans;
        //    DataTable messagesTableEnter = new DataTable();
        //    connection = new SqlConnection(connectionString);
        //    string queryMessage = "SELECT M.MessageEnter FROM MessageEnter M" +
        //                            "WHERE M.command = @command ";
        //    SqlCommand command = new SqlCommand(queryMessage, connection);
        //    adapter = new SqlDataAdapter(command);

        //    using (connection)
        //    using (command)
        //    using (adapter)
        //    {
        //        //connection.open();
        //        command.Parameters.AddWithValue("@command", stringCommand);
        //        //command.ExecuteNonQuery();
        //        adapter.Fill(messagesTableEnter);
        //        ans = messagesTableEnter.ToString();
        //    }
        //    //I think it should all presented in messageBox.Show
        //    return ans;
        //}

        public string getEnterMessage(string stringCommand)
        {
            string ans;
            DataTable messagesTableEnter = new DataTable();
            connection = new SqlConnection(connectionString);
            string queryMessage = "SELECT M.MessageEnter FROM MessageEnter M" +
                                    "WHERE M.command = @command ";
            SqlCommand command = new SqlCommand(queryMessage, connection);
            adapter = new SqlDataAdapter(command);

            using (connection)
            using (command)
            using (adapter)
            {
                //connection.open();
                command.Parameters.AddWithValue("@command", stringCommand);
                //command.ExecuteNonQuery();
                adapter.Fill(messagesTableEnter);
                ans = messagesTableEnter.ToString();
            }
            //I think it should all presented in messageBox.Show
            return ans;
        }

        private string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        bool VerifyMd5Hash(string input, string hash)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Hash the input.
                string hashOfInput = GetMd5Hash(input);

                // Create a StringComparer an compare the hashes.
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                if (0 == comparer.Compare(hashOfInput, hash))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private Random random = new Random();
        public string generateSalt()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 32)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
