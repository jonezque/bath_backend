using System;
using System.Collections.Generic;
using api.Persistent;

namespace api.Models
{
	public class OrderModel
	{
		public IEnumerable<BathPlaceModel> Places { get; set; }

		public IEnumerable<ProductPositionModel> Products { get; set; }

		public DateTime Date { get; set; }

		public PaymentType Type { get; set; }
		public RoomType Room { get; set; }
	}
}