using System.Collections.Generic;
using MongoDB.Driver;

namespace LeaderboardAPI
{
    public class MongoDBConnection
    {
        // The MongoClient class is used to establish a connection to the MongoDB cluster
        private readonly MongoClient _client;
        // The IMongoDatabase interface provides methods to interact with the database
        private readonly IMongoDatabase _database;

        // constructor that takes in the connection string and database name
        public MongoDBConnection(string connectionString, string databaseName)
        {
            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);
        }

        // Method for retrieving user points from the database
        public IEnumerable<Points> GetPoints()
        {
            var pointsCollection = _database.GetCollection<Points>("points");
            var points = pointsCollection.Find(point => point.approved == true).ToEnumerable();
            return points;
        }

        public IMongoDatabase Database
        {
            get { return _database; }
        }

        // properties for accessing the points, leaderboard and userRewards collections in the database
        public IMongoCollection<Points> Points => _database.GetCollection<Points>("points");

        public IMongoCollection<Leaderboard> Leaderboard => _database.GetCollection<Leaderboard>("leaderboard");
        public IMongoCollection<UserRewards> UserRewards => _database.GetCollection<UserRewards>("userRewards");
    }
}