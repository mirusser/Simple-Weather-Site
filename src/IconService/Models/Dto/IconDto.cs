using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Models.Dto
{
    public class IconDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool DayIcon { get; set; }
        public byte[] FileContent { get; set;}
    }
}
