using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LeaderboardAPI
{
    public class RewardsCalculator
    {
        private readonly MongoDBConnection _dbConnection;

        private const string FirstPrize = "First Prize";
        private const string SecondPrize = "Second Prize";
        private const string ThirdPrize = "Third Prize";
        private const string Top100Prize = "$25";
        private const string ConsolationPrize = "Consolation Prize";
        private const int ConsolationPrizeAmount = 12500;

        public RewardsCalculator(MongoDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Method for calculating and distributing rewards
        public void CalculateRewards(string month)
        {
            Regex monthFormat = new Regex(@"^(0[1-9]|1[012])[0-9]{4}$");
            if (!monthFormat.IsMatch(month))
            {
                throw new ArgumentException("Month must be in the format MMYYYY");
            }
            _dbConnection.Points.Indexes.CreateOne(new CreateIndexModel<Points>(Builders<Points>.IndexKeys
            .Descending(x => x.point)));

            // Retrieve approved user point data for the current month
            var points = _dbConnection.Points.Find(Builders<Points>.Filter.Eq(p => p.approved, true)).ToList();

            // Calculate rankings based on point data
            var rankings = points.OrderByDescending(point => point.point)
            .Select((point, index) => new { point.user_id, point.point, rank = index + 1 });

            // Remove any existing rewards
            _dbConnection.UserRewards.DeleteMany(Builders<UserRewards>.Filter.Empty);

            // Distribute rewards based on rankings
            var userRewards = new List<UserRewards>();
            foreach (var ranking in rankings)
            {
                var userReward = new UserRewards
                {
                    user_id = ObjectId.Parse(ranking.user_id),
                };
                switch (ranking.rank)
                {
                    case 1:
                        userReward.reward = FirstPrize;
                        break;
                    case 2:
                        userReward.reward = SecondPrize;
                        break;
                    case 3:
                        userReward.reward = ThirdPrize;
                        break;
                    case int n when (n <= 100):
                        userReward.reward = Top100Prize;
                        userReward.prize = 25;
                        break;
                    case int n when (n <= 1000):
                        userReward.reward = ConsolationPrize;
                        userReward.prize = ConsolationPrizeAmount / 1000;
                        break;
                }
                userRewards.Add(userReward);
            }
            _dbConnection.UserRewards.InsertMany(userRewards);
            // Create leaderboard
            CreateLeaderboard(month, rankings);
        }
        // Method for creating the leaderboard
        public IEnumerable<Leaderboard> CreateLeaderboard(string month, IEnumerable<dynamic> rankings)
        {
            // Create a new collection for the current month's leaderboard
            var leaderboardCollection = _dbConnection.Database.GetCollection<Leaderboard>("Leaderboard_" + month);

            // Check if the leaderboard for the current month already exists
            var leaderboardExists = leaderboardCollection.CountDocuments(Builders<Leaderboard>.Filter.Empty) > 0;
            if (!leaderboardExists)
            {
                // If the leaderboard does not exist, insert the rankings into the new collection
                var leaderboardList = rankings.Select(ranking => new Leaderboard
                {
                    user_id = ranking.user_id,
                    point = ranking.point,
                    rank = ranking.rank
                });
                leaderboardCollection.InsertMany(leaderboardList);
                return leaderboardList;
            }
            else
            {
                Console.Write("Leaderboard of the requested month already exists");
                return leaderboardCollection.Find(Builders<Leaderboard>.Filter.Empty).ToList();
            }
        }

        public IEnumerable<UserRewards> ListAllRewards()
        {
            // Retrieve all rewards
            var allRewards = _dbConnection.UserRewards.Find(Builders<UserRewards>.Filter.Empty).ToList();
            return allRewards;
        }


        // Method for listing user rewards
        public IEnumerable<UserRewards> ListUserRewards(string userId)
        {
            // Retrieve rewards for the specified user
            var userRewards = _dbConnection.UserRewards.Find(reward => reward.user_id == ObjectId.Parse(userId)).ToList();
            return userRewards;
        }

        // Method for listing the leaderboard
        public IEnumerable<Leaderboard> ListLeaderboard(string month)
        {
            // Validate input
            Regex monthFormat = new Regex(@"^(0[1-9]|1[012])[0-9]{4}$");
            if (!monthFormat.IsMatch(month))
            {
                throw new ArgumentException("Month must be in the format MMYYYY");
            }
            try
            {
                // Retrieve the leaderboard for the specified month
                var leaderboard = _dbConnection.Database.GetCollection<Leaderboard>("Leaderboard_" + month)
                .Find(Builders<Leaderboard>.Filter.Empty).ToList();
                if (!leaderboard.Any())
                {
                    Console.WriteLine("No Leaderboard exists to list");
                }

                return leaderboard;
            }
            catch (Exception ex)
            {
                // Log the exception message
                Console.WriteLine(ex.Message);
                // Return a response indicating that an error occurred while trying to retrieve the leaderboard
                return null;
            }
        }
    }
}


