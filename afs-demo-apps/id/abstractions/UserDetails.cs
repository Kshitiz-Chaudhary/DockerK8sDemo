using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace afs.jwt.abstractions
{
    public class UserDetails
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string password_hash { get; set; }
        public List<string> roles { get; set; }

        public bool ValidatePassword(string passwordText)
        {
            using (var algorithm = SHA256.Create())
            {
                var encodedBytes = Encoding.UTF8.GetBytes(passwordText);
                var hashedBytes = algorithm.ComputeHash(encodedBytes);  
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();  

                return String.Equals(password_hash, hash, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}