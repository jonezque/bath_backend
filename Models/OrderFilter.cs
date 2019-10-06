using System;

namespace api.Models
{
	public class OrderFilter
	{
		public RoomFilter Room { get; set; }
		public PaymentFilter Payment { get; set; }
		public StatusFilter Status { get; set; }
		public DateFilter Date { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}

	public enum PaymentFilter
	{
		Cash,
		Card,
		Both
	}

	public enum StatusFilter
	{
		Ok,
		Cancel,
		Both
	}

	public enum DateFilter
	{
		Day,
		Interval
	}

	public enum RoomFilter
	{
		Male,
		Female,
		Both
	}
}