using Microsoft.AspNetCore.Identity;

namespace AspForum.Data.Entities
{
	public class Role : IdentityRole<Guid>
	{
		public List<User> Users { get; set; } = null!;
		public Role(string roleName) : base(roleName) {}
		public Role() { }
	}
}
