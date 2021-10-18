using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Models.Dto
{
    public class CreateIconDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public bool DayIcon { get; set; }
        public byte[] FileContent { get; set; } = null!;
    }
}
