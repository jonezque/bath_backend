using api.Persistent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class CancelOrderModel
    {
        public DateTime Date { get; set; }
        public IEnumerable<int> OrderIds { get; set; }
        public string Reason { get; set; }
        public RoomType Room { get; set; }
    }
}
