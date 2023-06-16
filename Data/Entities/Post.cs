namespace AspForum.Data.Entities
{
	public class Post
	{
		public Guid Id { get; set; }
		public Guid TopicId { get; set; }
		public Guid? ReplyId { get; set; }
		public Guid AuthorId { get; set; }
		public string Content { get; set; } = null!;
		public DateTime CreatedDt { get; set; }
		public DateTime? DeletedDt { get; set; }
		public User Author { get; set; } = null!;
		public Post? Reply { get; set; } = null!;
		public List<Rate> Rates { get; set; } = null!;
	}
}
