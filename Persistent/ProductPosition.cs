﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Persistent
{
	public class ProductPosition
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }

		public int Count { get; set; }

		public int OrderId { get; set; }
		public Product Product { get; set; }

		public int ProductId { get; set; }

		[Column(TypeName = "decimal(8,2)")] public decimal ProductPrice { get; set; }

		[Column(TypeName = "decimal(8,2)")] public decimal TotalPrice { get; set; }
	}
}