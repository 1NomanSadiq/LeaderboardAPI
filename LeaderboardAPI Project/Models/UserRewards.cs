using MongoDB.Bson;
public class UserRewards
{
    public ObjectId _id { get; set; }
    public ObjectId user_id { get; set; }
    public string reward { get; set; }
    public int prize { get; set; }
}