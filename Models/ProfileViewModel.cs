using AspForum.Models.Forum;

namespace AspForum.Models
{
	public class ProfileViewModel
	{
		public string UserName { get; set; } = null!;
		public string? AvatarUrl { get; set; }
		public string Role { get; set; } = null!;

		public List<ProfileRecentPostViewModel> RecentPosts { get; set; } = null!;
	}
}
