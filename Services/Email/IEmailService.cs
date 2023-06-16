namespace AspForum.Services.Email
{
	public interface IEmailService
	{
		bool Send(string mailTemplate, object model);
		Task<bool> SendAsync(string mailTemplate, object model);
	}
}
