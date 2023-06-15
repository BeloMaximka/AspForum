namespace AspForum.Models
{
	public class PostViewModel
	{
		public string AuthorName { get; set; } = null!;
		public string AuthorId { get; set; } = null!;
		public string? AuthorAvatarURL { get; set; } = null!;
		public string Content { get; set; } = null!;
		public string CreationDateString { get; set; } = null!;
		public PostViewModel? Reply { get; set; }
	}
}
