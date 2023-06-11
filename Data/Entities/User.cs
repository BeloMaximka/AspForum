﻿using Microsoft.AspNetCore.Identity;

namespace AspForum.Data.Entities
{
	public class User : IdentityUser<Guid>
	{
		public string? AvatarUrl { get; set; }
	}
}
