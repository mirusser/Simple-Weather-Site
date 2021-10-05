using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SignalRServer.Mongo.Documents
{
    public class WeatherHistoryConnection
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement(nameof(ConnectionId))]
        [BsonRepresentation(BsonType.String)]
        [Required(ErrorMessage = "ConnectionId is required")]
        public string ConnectionId { get; set; } = null!;
    }
}