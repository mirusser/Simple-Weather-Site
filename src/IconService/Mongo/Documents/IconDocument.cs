using Convey.Types;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Mongo.Documents
{
    public class IconDocument : IIdentifiable<Guid>
    {
        [BsonId]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "DayIcon is required")]
        public bool DayIcon { get; set; }
    }
}
