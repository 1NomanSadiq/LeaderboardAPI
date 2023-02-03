using MongoDB.Bson;
public class Leaderboard
{
    public ObjectId _id { get; set; }
    public string user_id { get; set; }
    public int point { get; set; }
    public int rank { get; set; }
}