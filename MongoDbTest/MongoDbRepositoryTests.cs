/*
 * Author: Nikola Živković
 * Blog: rubikscode.net
 * Company: Vega IT Sourcing
 * Year: 2017
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Bson;

namespace MongoDb
{
    /// <summary>
    /// Testing MongoDbRepository class.
    /// </summary>
    /// <notes>
    /// In order for these tests to pass, you must have mongo server runing on localhost:27017.
    /// If you need more info on how to do so check this blog post:
    /// https://rubikscode.net/2017/07/24/mongo-db-basics-part-1/
    /// </notes>
    [TestClass]
    public class MongoDbRepositoryTests
    {
        UsersRepository _mongoDbRepo = new UsersRepository("mongodb://localhost:27017");

        [TestInitialize]
        public async Task Initialize()
        {
            var user = new User()
            {
                Name = "Nikola",
                Age = 30,
                Blog = "rubikscode.net",
                Location = "Beograd"
            };
            await _mongoDbRepo.InsertUser(user);

            user = new User()
            {
                Name = "Vanja",
                Age = 27,
                Blog = "eventroom.net",
                Location = "Beograd"
            };
            await _mongoDbRepo.InsertUser(user);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _mongoDbRepo.DeleteAllUsers();
        }

        [TestMethod]
        public void CheckConnection_DbAvailable_ConnectionEstablished()
        {
            var connected = _mongoDbRepo.CheckConnection();
            Assert.IsTrue(connected);
        }

        /// <summary>
        /// Test is ignored, because it lasts 30 seconds.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void CheckConnection_DbNotAvailable_ConnectionFailed()
        {
            var mongoDbRepo = new UsersRepository("mongodb://localhost:27016");
            var connected = mongoDbRepo.CheckConnection();
            Assert.IsFalse(connected);
        }

        [TestMethod]
        public async Task GetAllUsers_ReadAllUsers_CountIsExpected()
        {
            var users = await _mongoDbRepo.GetAllUsers();
            Assert.AreEqual(2, users.Count);
        }

        [TestMethod]
        public async Task GetUserByField_GetUserByNameAndUserExists_UserReturned()
        {
            var users = await _mongoDbRepo.GetUsersByField("name", "Nikola");
            Assert.AreEqual(1, users.Count);
        }

        [TestMethod]
        public async Task GetUserByField_GetUserByBlogAndUserExists_UserReturned()
        {
            var users = await _mongoDbRepo.GetUsersByField("blog", "rubikscode.net");
            Assert.AreEqual(1, users.Count);
        }

        [TestMethod]
        public async Task GetUserByField_GetUserByNameAndUserDoesntExists_UserNotReturned()
        {
            var users = await _mongoDbRepo.GetUsersByField("name", "Napoleon");
            Assert.AreEqual(0, users.Count);
        }

        [TestMethod]
        public async Task GetUserByField_WrongField_UserNotReturned()
        {
            var users = await _mongoDbRepo.GetUsersByField("badFieldName", "value");
            Assert.AreEqual(0, users.Count);
        }

        [TestMethod]
        public async Task GetUserCount_JustFirstElement_Success()
        {
            var users = await _mongoDbRepo.GetUsers(0, 1);
            Assert.AreEqual(1, users.Count);
        }

        [TestMethod]
        public async Task InsertUser_UserInserted_CountIsExpected()
        {
            var user = new User()
            {
                Name = "Simona",
                Age = 0,
                Blog = "babystuff.com",
                Location = "Beograd"
            };

            var users = await _mongoDbRepo.GetAllUsers();
            var countBeforeInsert = users.Count;

            await _mongoDbRepo.InsertUser(user);

            users = await _mongoDbRepo.GetAllUsers();
            Assert.AreEqual(countBeforeInsert + 1, users.Count);
        }

        [TestMethod]
        public async Task DeleteUserById_UserDeleted_GoodReturnValue()
        {
            var user = new User()
            {
                Name = "Simona",
                Age = 0,
                Blog = "babystuff.com",
                Location = "Beograd"
            };

            await _mongoDbRepo.InsertUser(user);

            var deleteUser = await _mongoDbRepo.GetUsersByField("name", "Simona");
            var result = await _mongoDbRepo.DeleteUserById(deleteUser.Single().Id);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteUserById_UserDoesntExist_NothingIsDeleted()
        {
            var result = await _mongoDbRepo.DeleteUserById(ObjectId.Empty);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task DeleteAllUsers_DelitingEverything_Sucess()
        {
            var result = await _mongoDbRepo.DeleteAllUsers();

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public async Task UpdateUser_UpdateTopLevelField_UserModified()
        {
            var users = await _mongoDbRepo.GetUsersByField("name", "Nikola");
            var user = users.FirstOrDefault();

            await _mongoDbRepo.UpdateUser(user.Id, "blog", "Rubik's Code");

            users = await _mongoDbRepo.GetUsersByField("name", "Nikola");
            user = users.FirstOrDefault();

            Assert.AreEqual("Rubik's Code", user.Blog);
        }

        [TestMethod]
        public async Task UpdateUser_UpdateTopLevelField_GoodReturnValue()
        {
            var users = await _mongoDbRepo.GetUsersByField("name", "Nikola");
            var user = users.FirstOrDefault();

            var result = await _mongoDbRepo.UpdateUser(user.Id, "blog", "Rubik's Code");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task UpdateUser_TryingToUpdateNonExistingUser_GoodReturnValue()
        {
            var result = await _mongoDbRepo.UpdateUser(ObjectId.Empty, "blog", "Rubik's Code");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UpdateUser_ExtendingWithNewField_GoodReturnValue()
        {
            var users = await _mongoDbRepo.GetUsersByField("name", "Nikola");
            var user = users.FirstOrDefault();

            var result = await _mongoDbRepo.UpdateUser(user.Id, "address", "test address");

            Assert.IsTrue(result);
        }
    }
}
