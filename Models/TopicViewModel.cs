﻿namespace AspForum.Models
{
	public class TopicViewModel
	{
		public Guid Id { get; set; }
		public string AuthorId { get; set; } = null!;
		public string AuthorRole { get; set; } = null!;
		public string AuthorName { get; set; } = null!;
		public string? AuthorAvatarURL { get; set; } = null!;
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;
		public string CreationDateString { get; set; } = null!;
		public List<PostViewModel> Posts { get; set; } = null!;

		public int? UserRating { get; set; }
		public int Likes { get; set; }
		public int Dislikes { get; set; }
	}
}
