using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class PriceModel
    {
        public IEnumerable<PriceDto> Prices { get; set; }
    }

    public class PriceDto {
        public int Id { get; set; }
        public decimal Price { get; set; }
    }
}
