using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Sample.WebApi.TestService.TestModels;

public class TestModel
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [DataMember]
    [BsonElement("name")]
    public string Name { get; set; }

    [DataMember]
    [BsonElement("someUpdateString")]
    public string SomeUpdateString { get; set; } = "Prop_for_update";

    [DataMember]
    [BsonElement("someUpdateNumber")]
    public int SomeUpdateNumber { get; set; } = 100;
    
    [DataMember]
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [DataMember]
    [BsonElement("updateAt")]
    public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
}