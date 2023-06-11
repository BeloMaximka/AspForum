using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AspForum.Models
{
	public class SectionFormViewModel
	{
		[Required]
		[FromForm(Name = "title")]
		public string Title { get; set; } = null!;
	}
}
