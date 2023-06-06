using System.ComponentModel.DataAnnotations;

namespace AspForum.Models
{
	public class LoginViewModel
	{
		[Required]
		[Display(Name = "Username")]
		public string Username { get; set; } = null!;

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = null!;

		public bool RememberMe { get; set; }
	}
}
