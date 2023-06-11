using AspForum.Data.Entities;

namespace AspForum.Models
{
	public class SectionViewModel
	{
		public string Id { get; set; } = null!;
		public string Title { get; set; } = null!;

		public List<Theme> Themes { get; set; } = null!;
	}
}
