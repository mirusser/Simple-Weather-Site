using Convey.Types;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Mongo.Documents
{
    public class IconDocument : IIdentifiable<string>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        [BsonElement("Description")]
        [BsonRepresentation(BsonType.String)]
        public string Description { get; set; }

        [BsonElement("Icon")]
        [BsonRepresentation(BsonType.String)]
        public string Icon { get; set; }

        [BsonElement("DayIcon")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool DayIcon { get; set; }

        [BsonElement("FileContent")]
        [BsonRepresentation(BsonType.Binary)]
        public byte[] FileContent { get; set; }
    }
}