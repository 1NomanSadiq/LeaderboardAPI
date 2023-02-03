using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LeaderboardAPI;

namespace LeaderboardApp
{
    public class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("System.Diagnostics.Trace.UseGlobalLock", false);
            var host = CreateWebHostBuilder(args)
            .UseSetting("detailedErrors", "true") // enable detailed errors
            .CaptureStartupErrors(true) // capture startup errors
            .Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    //Getting the required services
                    var rewardsCalculator = services.GetRequiredService<RewardsCalculator>();
                    var mongoDBConnection = services.GetRequiredService<MongoDBConnection>();
                    var _leaderboardController = services.GetRequiredService<LeaderboardController>();

                    //Current Month in the format MMYYYY
                    DateTime currentDate = DateTime.Now;
                    var currentMonth = currentDate.ToString("MMyyyy");

                    Console.WriteLine("\n\nPlease Wait...");

                    //Creating the leaderboard for the current month and distributing the rewards
                    rewardsCalculator.CalculateRewards(currentMonth);

                    //Getting the leadboard then printing it. User id, rank, total points information will be displayed.
                    var leaderboard = rewardsCalculator.ListLeaderboard(currentMonth);
                    Console.WriteLine("\n\nLeaderboard:");
                    foreach (var item in leaderboard)
                    {
                        Console.WriteLine(item.user_id + " - " + item.rank + " - " + item.point);
                    }

                    //Filtering leaderboard according to the user id
                    var leaderboardByUser = _leaderboardController.ListLeaderboard(currentMonth, "628bd088f833c9b49877a580");
                    Console.WriteLine("\n\nLeaderboard for user 628bd088f833c9b49877a580 :");
                    foreach (var item in leaderboardByUser)
                    {
                        Console.WriteLine(item.user_id + " - " + item.rank + " - " + item.point);
                    }

                    //All Rewards
                    var rewards = rewardsCalculator.ListAllRewards();
                    Console.WriteLine("\n\nRewards:");
                    foreach (var item in rewards)
                    {
                        Console.WriteLine(item.user_id + " - " + item.reward + " - " + item.prize);
                    }

                    //Filtering rewards according to the user id
                    var userRewards = rewardsCalculator.ListUserRewards("628b514c1ba3abddabf15fe3");
                    Console.WriteLine("\n\nRewards for user 628b514c1ba3abddabf15fe3:");
                    foreach (var item in userRewards)
                    {
                        Console.WriteLine(item.reward + " - " + item.prize + "\n\n");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            host.Run();
        }
    }
}