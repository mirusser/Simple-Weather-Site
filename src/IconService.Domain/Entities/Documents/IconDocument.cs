using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace IconService.Domain.Entities.Documents
{
    public class IconDocument
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("Name")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; } = null!;

        [BsonElement("Description")]
        [BsonRepresentation(BsonType.String)]
        public string Description { get; set; } = null!;

        [BsonElement("Icon")]
        [BsonRepresentation(BsonType.String)]
        public string Icon { get; set; } = null!;

        [BsonElement("DayIcon")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool DayIcon { get; set; }

        [BsonElement("FileContent")]
        [BsonRepresentation(BsonType.Binary)]
        public byte[] FileContent { get; set; } = null!;
    }
}