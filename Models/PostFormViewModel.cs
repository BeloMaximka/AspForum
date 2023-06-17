using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AspForum.Models
{
	public class PostFormViewModel
	{
		[Required]
		[FromForm(Name = "topicid")]
		public Guid TopicId { get; set; }

		[FromForm(Name = "replyid")]
		public Guid? ReplyId { get; set; }

		[Required]
		[FromForm(Name = "Content")]
		public string Content { get; set; } = null!;
	}
}
