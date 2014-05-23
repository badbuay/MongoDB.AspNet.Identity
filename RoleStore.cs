using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDB.AspNet.Identity {

    /// <summary>
    ///     Class RoleStore.
    /// </summary>
    /// <typeparam name="TRole">The type of the t role.</typeparam>
    public class RoleStore<TRole> : IRoleStore<TRole> where TRole : IdentityRole
    {
        #region Private Methods & Variables

        /// <summary>
        ///     The database
        /// </summary>
        private readonly MongoDatabase _db;

        /// <summary>
        ///     The _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The AspNetUsers collection name
        /// </summary>
        private const string CollectionName = "AspNetRoles";

        /// <summary>
        ///     Gets the database from connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>MongoDatabase.</returns>
        /// <exception cref="System.Exception">No database name specified in connection string</exception>
        private MongoDatabase GetDatabaseFromSqlStyle(string connectionString)
        {
            var conString = new MongoConnectionStringBuilder(connectionString);
            MongoClientSettings settings = MongoClientSettings.FromConnectionStringBuilder(conString);
            MongoServer server = new MongoClient(settings).GetServer();
            if (conString.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return server.GetDatabase(conString.DatabaseName);
        }

        /// <summary>
        ///     Gets the database from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>MongoDatabase.</returns>
        private MongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            var client = new MongoClient(url);
            MongoServer server = client.GetServer();
            if (url.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return server.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        /// <summary>
        ///     Uses connectionString to connect to server and then uses databae name specified.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dbName">Name of the database.</param>
        /// <returns>MongoDatabase.</returns>
        private MongoDatabase GetDatabase(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            return server.GetDatabase(dbName);
        }

        #endregion

        #region Constructors
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleStore" /> class. Uses DefaultConnection name if none was
        ///     specified.
        /// </summary>
        public RoleStore() : this("DefaultConnection")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleStore" /> class. Uses name from ConfigurationManager or a
        ///     mongodb:// Url.
        /// </summary>
        /// <param name="connectionNameOrUrl">The connection name or URL.</param>
        public RoleStore(string connectionNameOrUrl)
        {
            if (connectionNameOrUrl.ToLower().StartsWith("mongodb://"))
            {
                _db = GetDatabaseFromUrl(new MongoUrl(connectionNameOrUrl));
            }
            else
            {
                string connStringFromManager =
                    ConfigurationManager.ConnectionStrings[connectionNameOrUrl].ConnectionString;
                if (connStringFromManager.ToLower().StartsWith("mongodb://"))
                {
                    _db = GetDatabaseFromUrl(new MongoUrl(connStringFromManager));
                }
                else
                {
                    _db = GetDatabaseFromSqlStyle(connStringFromManager);
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleStore" /> class. Uses name from ConfigurationManager or a
        ///     mongodb:// Url.
        ///     Database can be specified separately from connection server.
        /// </summary>
        /// <param name="connectionNameOrUrl">The connection name or URL.</param>
        /// <param name="dbName">Name of the database.</param>
        public RoleStore(string connectionNameOrUrl, string dbName)
        {
            if (connectionNameOrUrl.ToLower().StartsWith("mongodb://"))
            {
                _db = GetDatabase(connectionNameOrUrl, dbName);
            }
            else
            {
                _db = GetDatabase(ConfigurationManager.ConnectionStrings[connectionNameOrUrl].ConnectionString, dbName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser}"/> class using a already initialized Mongo Database.
        /// </summary>
        /// <param name="mongoDatabase">The mongo database.</param>
        public RoleStore(MongoDatabase mongoDatabase)
        {
            _db = mongoDatabase;
        }


            /// <summary>
        ///     Initializes a new instance of the <see cref="RoleStore" /> class.
        /// </summary>
        /// <param name="connectionName">Name of the connection from ConfigurationManager.ConnectionStrings[].</param>
        /// <param name="useMongoUrlFormat">if set to <c>true</c> [use mongo URL format].</param>
        [Obsolete("Use RoleStore(connectionNameOrUrl)")]
        public RoleStore(string connectionName, bool useMongoUrlFormat)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            if (useMongoUrlFormat)
            {
                var url = new MongoUrl(connectionString);
                _db = GetDatabaseFromUrl(url);
            }
            else
            {
                _db = GetDatabaseFromSqlStyle(connectionString);
            }
        }

        #endregion

        #region Methods

        public Task CreateAsync(TRole role)
        {
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException("role");

            _db.GetCollection<TRole>(CollectionName).Insert(role);

            return Task.FromResult(role);
        }

        public Task<TRole> FindByIdAsync(string roleId)
        {
            ThrowIfDisposed();
            var role = _db.GetCollection<TRole>(CollectionName).FindOne((Query.EQ("_id", ObjectId.Parse(roleId))));
            return Task.FromResult(role);   
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }

         /// <summary>
        ///     Throws if disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"></exception>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }      

        /// <summary>
        ///     Deletes the role asynchronous.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">role</exception>
        public Task DeleteAsync(TRole role) {
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException("role");

            _db.GetCollection(CollectionName).Remove((Query.EQ("_id", ObjectId.Parse(role.Id))));
            return Task.FromResult(true);
        }

        /// <summary>
        ///     Finds the by name asynchronous.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TRole> FindByNameAsync(string roleName) {
            ThrowIfDisposed();

            TRole role = _db.GetCollection<TRole>(CollectionName).FindOne((Query.EQ("Name", roleName)));
            return Task.FromResult(role);
        }

        /// <summary>
        ///     Updates the role asynchronous.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">role</exception>
        public Task UpdateAsync(TRole role) {
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException("role");

            _db.GetCollection<TRole>(CollectionName)
                .Update(Query.EQ("_id", ObjectId.Parse(role.Id)), Update.Replace(role), UpdateFlags.Upsert);

            return Task.FromResult(role);
        }

        #endregion
    }
}