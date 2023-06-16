namespace AspForum.Models.EmailTemplates
{
    public class СonfirmEmailChange
    {
        public string UserName { get; set; } = null!;
        public string ChangeEmailLink { get; set; } = null!;
        public string OldEmail { get; set; } = null!;
		public string Email { get; set; } = null!;
	}
}
