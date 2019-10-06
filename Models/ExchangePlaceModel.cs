using api.Persistent;

namespace api.Models
{
	public class ExchangePlaceModel
	{
		public string From { get; set; }
		public string To { get; set; }
		public RoomType Room { get; set; }
	}
}