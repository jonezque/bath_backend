using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Persistent
{
	public class ServicePosition
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }

		public Master Master { get; set; }
		public int MasterId { get; set; }
		public int OrderId { get; set; }
		public Service Service { get; set; }
		public int ServiceId { get; set; }
		public decimal ServiceCost { get; set; }
		public decimal AddonsCost { get; set; }
		public decimal TotalCost { get; set; }
		public DateTime Time { get; set; }
	}
}