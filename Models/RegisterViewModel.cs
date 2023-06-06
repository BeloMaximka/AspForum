using System.ComponentModel.DataAnnotations;

namespace AspForum.Models
{
	public class RegisterViewModel
	{
		[Required]
		public string Username { get; set; } = null!;

		[Required]
		[EmailAddress]
		public string Email { get; set; } = null!;

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; } = null!;

		[Required]
		[Compare("Password", ErrorMessage = "Password mismatch")]
		[DataType(DataType.Password)]
		public string PasswordConfirm { get; set; } = null!;

		[Required]
		public bool IsAgree { get; set; }

	}
}
