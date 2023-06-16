namespace AspForum.Models
{
	public class PostViewModel
	{
		public string Id { get; set; } = null!;
		public string AuthorName { get; set; } = null!;
		public string AuthorId { get; set; } = null!;
		public string AuthorRole { get; set; } = null!;
		public string? AuthorAvatarURL { get; set; } = null!;
		public string Content { get; set; } = null!;
		public string CreationDateString { get; set; } = null!;
		public PostViewModel? Reply { get; set; }

		public int? UserRating { get; set;}
		public int Likes { get; set;}
		public int Dislikes { get; set;}
	}
}
