namespace AspForum.Models.EmailTemplates
{
	public class ChangeEmailRequestModel
	{
		public string UserName { get; set; } = null!;
		public string ChangeEmailLink { get; set; } = null!;
		public string NewEmail { get; set; } = null!;
		public string Email { get; set; } = null!;
	}
}
