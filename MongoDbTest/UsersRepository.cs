/*
 * Author: Nikola Živković
 * Blog: rubikscode.net
 * Company: Vega IT Sourcing
 * Year: 2017
 */

using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDb
{
    /// <summary>
    /// Class used to access Mongo DB.
    /// </summary>
    public class UsersRepository
    {
        private IMongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<User> _usersCollection;

        public UsersRepository(string connectionString)
        {
            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase("blog");
            _usersCollection = _database.GetCollection<User>("users");
        }

        /// <summary>
        /// Checking is connection to the database established.
        /// </summary>
        public bool CheckConnection()
        {
            try
            {
                _database.ListCollections();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returning all data from 'users' collection.
        /// </summary>
        public async Task<List<User>> GetAllUsers()
        {
            return await _usersCollection.Find(new BsonDocument()).ToListAsync();
        }

        /// <summary>
        /// Returning all users with the defined value of defined field.
        /// </summary>
        public async Task<List<User>> GetUsersByField(string fieldName, string fieldValue)
        {
            var filter = Builders<User>.Filter.Eq(fieldName, fieldValue);
            var result = await _usersCollection.Find(filter).ToListAsync();

            return result;
        }

        /// <summary>
        /// Returning defined number of users.
        /// </summary>
        public async Task<List<User>> GetUsers(int startingFrom, int count)
        {
            var result = await _usersCollection.Find(new BsonDocument()).Skip(startingFrom).Limit(count).ToListAsync();

            return result;
        }

        /// <summary>
        /// Inserting passed user into the database.
        /// </summary>
        public async Task InsertUser(User user)
        {
            await _usersCollection.InsertOneAsync(user);
        }

        /// <summary>
        /// Removing user with defined _id.
        /// </summary>
        /// <returns>
        /// True - If user was deleted.
        /// False - If user was not deleted.
        /// </returns>
        public async Task<bool> DeleteUserById(ObjectId id)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);
            var result = await _usersCollection.DeleteOneAsync(filter);
            return result.DeletedCount != 0;
        }

        /// <summary>
        /// Removing all data from 'users' collection. 
        /// </summary>
        /// <returns>
        /// Number of deleted users.
        /// </returns>
        public async Task<long> DeleteAllUsers()
        {
            var filter = new BsonDocument();
            var result = await _usersCollection.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// Updating user.
        /// </summary>
        /// <param name="_id">User id.</param>
        /// <param name="udateFieldName">Field that should be updated.</param>
        /// <param name="updateFieldValue">New value for the field.</param>
        /// <returns>
        /// True - If user is updated.
        /// False - If user is not updated.
        /// </returns>
        public async Task<bool> UpdateUser(ObjectId id, string udateFieldName, string updateFieldValue)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);
            var update = Builders<User>.Update.Set(udateFieldName, updateFieldValue);

            var result = await _usersCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount != 0;
        }

        /// <summary>
        /// Creates index on defined field.
        /// </summary>
        public async Task CreateIndexOnCollection(IMongoCollection<BsonDocument> collection, string field)
        {
            var keys = Builders<BsonDocument>.IndexKeys.Ascending(field);
            await collection.Indexes.CreateOneAsync(keys);
        }
    }
}
