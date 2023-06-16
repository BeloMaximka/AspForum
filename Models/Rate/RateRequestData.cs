namespace AspForum.Models.Rate
{
	public class RateRequestData
	{
		public Guid ItemId { get; set; }
		public int Rating { get; set; }
		public string Type { get; set; } = null!;
	}
}
