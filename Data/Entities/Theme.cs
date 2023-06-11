namespace AspForum.Data.Entities
{
	public class Theme
	{
		public Guid Id { get; set; }
		public Guid SectionId { get; set; }
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;
		public DateTime? DeletedDt { get; set; }
	}
}
