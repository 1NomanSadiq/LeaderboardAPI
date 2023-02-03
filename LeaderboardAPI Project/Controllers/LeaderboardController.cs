using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LeaderboardAPI;
using MongoDB.Bson;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class LeaderboardController : ControllerBase
{
    private readonly RewardsCalculator _rewardsCalculator;
    private readonly MongoDBConnection _mongoDBConnection;

    public LeaderboardController(RewardsCalculator rewardsCalculator, MongoDBConnection mongoDBConnection)
    {
        _rewardsCalculator = rewardsCalculator;
        _mongoDBConnection = mongoDBConnection;
    }

    [HttpPost("create")]
    public IEnumerable<Leaderboard> CreateLeaderboard(string month)
    {
        try
        {
            var points = _mongoDBConnection.GetPoints();
            var rankings = points.OrderByDescending(point => point.point)
                               .Select((point, index) => new { point.user_id, point.point, rank = index + 1 });
            return _rewardsCalculator.CreateLeaderboard(month, rankings);
        }

        catch (Exception ex)
        {
            Console.Write(ex.Message);
            return null;
        }
    }

    [HttpGet("list")]
    public IEnumerable<Leaderboard> ListLeaderboard(string month, string userId)
    {

        // Retrieve leaderboard
        var leaderboard = _rewardsCalculator.ListLeaderboard(month);
        // Apply filter by user ID
        leaderboard = leaderboard.Where(lb => lb.user_id == userId);

        return leaderboard;
    }

    [HttpGet("rewards")]
    public IEnumerable<UserRewards> ListUserRewards(string userId)
    {
        try
        {
            // Retrieve user rewards
            var userRewards = _rewardsCalculator.ListUserRewards(userId);
            return userRewards;
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            return null;
        }
    }
}