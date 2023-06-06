using Microsoft.AspNetCore.Identity;

namespace AspForum.Data.Entities
{
	public class User : IdentityUser
	{
		public string? AvatarUrl { get; set; }
	}
}
