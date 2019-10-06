using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Persistent
{
	public class BathPlacePrice
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }

		[Column(TypeName = "decimal(8,2)")] public decimal Price { get; set; }

		public RoomType Room { get; set; }
		public PlaceType Type { get; set; }
	}
}