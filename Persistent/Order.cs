using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Persistent
{
    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<ProductPosition> ProductPositions { get; set; }

        public ICollection<BathPlacePosition> BathPlacePositions { get; set; }

        public DateTime Modified { get; set; }

        public PaymentType Type { get; set; }

        public RoomType Room { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal TotalCost { get; set; }

        public bool Canceled { get; set; }

        public string Commentary { get; set; }
    }
}
