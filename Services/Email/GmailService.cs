using System.Net.Mail;
using System.Net;

namespace AspForum.Services.Email
{
	public class GmailService : IEmailService
	{
		private IConfiguration _configuration;
		private ILogger<GmailService> _logger;

		public GmailService(IConfiguration configuration, 
			ILogger<GmailService> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public bool Send(string mailTemplate, object model)
		{
			return SendAsync(mailTemplate, model).Result;
		}

		public async Task<bool> SendAsync(string mailTemplate, object model)
		{
			string? template = null;
			string[] filenames = new string[]
			{
				mailTemplate,
				mailTemplate + ".html",
				"Services/Email/Templates/" + mailTemplate,
				"Services/Email/Templates/" + mailTemplate + ".html"
			};
			foreach (var filename in filenames)
			{
				if (System.IO.File.Exists(filename))
				{
					template = System.IO.File.ReadAllText(filename);
					break;
				}
			}
			if (template is null)
			{
				throw new ArgumentException($"Template '{mailTemplate}' not found");
			}
			string siteName = _configuration["Site:Name"] ??
				throw new MissingFieldException("Missing configuration 'Site:Name'");
			string host = _configuration["Smtp:Gmail:Host"] ??
				throw new MissingFieldException("Missing configuration 'Smpt:Gmail.Host'");
			string email = _configuration["Smtp:Gmail:Email"] ??
				throw new MissingFieldException("Missing configuration 'Smpt:Gmail.Email'");
			string password = _configuration["Smtp:Gmail:Password"] ??
				throw new MissingFieldException("Missing configuration 'Smpt:Gmail.Password'");
			if (!int.TryParse(_configuration["Smtp:Gmail:Port"], out int port))
				throw new MissingFieldException("Missing or invalid configuration 'Smpt:Gmail.Port'");
			if (!bool.TryParse(_configuration["Smtp:Gmail:Ssl"], out bool ssl))
				throw new MissingFieldException("Missing or invalid configuration 'Smpt:Gmail.Ssl'");

			string? userEmail = null;
			// Fill template
			foreach (var prop in model.GetType().GetProperties())
			{
				if (prop.Name == "Email") userEmail = prop.GetValue(model)?.ToString();
				string placeholder = $"{{{{{prop.Name}}}}}";
				if (template.Contains(placeholder))
				{
					template = template.Replace(placeholder, prop.GetValue(model)?.ToString() ?? "");
				}
			}

			template = template.Replace("{{SiteName}}", siteName);
			if (userEmail is null)
			{
				throw new ArgumentException("No 'Email property in model");
			}
			// TODO: Check other {{\w+}} placeholders in template
			using SmtpClient smtpClient = new(host, port)
			{
				EnableSsl = ssl,
				Credentials = new NetworkCredential(email, password)
			};
			MailMessage mailMessage = new()
			{
				From = new MailAddress(email),
				Subject = siteName,
				Body = template,
				IsBodyHtml = true,
			};
			mailMessage.To.Add(userEmail);
			try
			{
				await smtpClient.SendMailAsync(mailMessage);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"Send Email exception '{ex.Message}'");
				return false;
			}
		}
	}
}
