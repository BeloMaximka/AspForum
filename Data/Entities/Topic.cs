namespace AspForum.Data.Entities
{
	public class Topic
	{
		public Guid Id { get; set; }
		public Guid ThemeId { get; set; }
		public Guid AuthorId { get; set; }
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;
		public DateTime CreatedDt { get; set; }
		public DateTime? DeletedDt { get; set; }
		public User Author { get; set; } = null!;

		public List<Rate> Rates { get; set; } = null!;
	}
}
