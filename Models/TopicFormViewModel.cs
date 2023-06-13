using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AspForum.Models
{
    public class TopicFormViewModel
    {
        [Required]
        [FromForm(Name = "themeid")]
        public Guid ThemeId { get; set; }
        [Required]
        [FromForm(Name = "title")]
        public string Title { get; set; } = null!;
        [Required]
        [FromForm(Name = "description")]
        public string Description { get; set; } = null!;
    }
}
