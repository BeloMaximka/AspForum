using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AspForum.Models
{
	public class PostFormViewModel
	{
		[Required]
		[FromForm(Name = "topicid")]
		public Guid TopicId { get; set; }
		[Required]
		[FromForm(Name = "Content")]
		public string Content { get; set; } = null!;
	}
}
