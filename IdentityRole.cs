using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.AspNet.Identity {

    public class IdentityRole : IRole {

        public IdentityRole(string roleName)
        {
            
        }
        /// <summary>
        /// Unique key for the user
        /// </summary>
        /// <value>The identifier.</value>
        /// <returns>The unique key for the user</returns>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        /// <value>The name of the role.</value>
        public string Name { get; set; }
        
    }
}