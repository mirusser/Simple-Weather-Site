using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Models.Dto
{
    public class CreateIconDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool DayIcon { get; set; }
    }
}
