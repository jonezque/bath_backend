using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Persistent
{
    public class BathPlace
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public BathPlacePrice Price { get; set; }

        public int PriceId { get; set; }

        public DateTime Modified { get; set; }

        public PlaceType Type { get; set; }

        public RoomType Room { get; set; }

        public string Commentary { get; set; }
    }
}
