using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Persistent
{
	public class Discount
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }

		public string Name { get; set; }

		[Column(TypeName = "decimal(8,2)")] public decimal Value { get; set; }

		public DateTime Modified { get; set; }
	}
}