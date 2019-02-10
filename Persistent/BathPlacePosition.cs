using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Persistent
{
    public class BathPlacePosition
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? DiscountValue { get; set; }

        public string DiscountName { get; set; }

        public BathPlace BathPlace { get; set; }
        public int BathPlaceId { get; set; }

        public int OrderId { get; set; }


        public BathPlaceStatus Status { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }

        public int Duration { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Cost { get; set; }
    }
}
