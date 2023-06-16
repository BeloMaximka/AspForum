namespace AspForum.Models.EmailTemplates
{
	public class ConfirmEmailModel
	{
		public string UserName { get; set; } = null!;
		public string Email { get; set; } = null!;
		public string ConfirmLink { get; set; } = null!;
	}
}
