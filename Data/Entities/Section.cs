namespace AspForum.Data.Entities
{
	public class Section
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = null!;
		public DateTime? DeletedDt { get; set; }
	}
}
