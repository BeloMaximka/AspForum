using AspForum.Data.Entities;

namespace AspForum.Models
{
	public class ThemeViewModel
	{
		public string Id { get; set; } = null!;
		public string Title { get; set; } = null!;

		public List<TopicViewModel> Topics { get; set; } = null!;
	}
}
