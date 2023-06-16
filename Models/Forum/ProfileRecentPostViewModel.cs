namespace AspForum.Models.Forum
{
	public class ProfileRecentPostViewModel
	{
		public string Content { get; set; } = null!;
		public string CreationDateString { get; set; } = null!;
		public string TopicId { get; set; } = null!;
		public string TopicTitle { get; set; } = null!;
	}
}
