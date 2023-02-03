using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Points
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public bool approved { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string user_id { get; set; }
    public int point { get; set; }
}
