using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AspForum.Models
{
	public class ThemeFormViewModel
	{
		[Required]
		[FromForm(Name = "sectionid")]
		public Guid SectionId { get; set; }
		[Required]
		[FromForm(Name = "title")]
		public string Title { get; set; } = null!;
		[Required]
		[FromForm(Name = "description")]
		public string Description { get; set; } = null!;
	}
}
