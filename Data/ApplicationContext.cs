using AspForum.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Security.Cryptography;

namespace AspForum.Data
{
	public class ApplicationContext : IdentityDbContext<User, Role, Guid>
	{
		public DbSet<Section> Sections { get; set; }
		public DbSet<Theme> Themes { get; set; }
		public DbSet<Topic> Topics { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<Rate> Rates { get; set; }
		public ApplicationContext(DbContextOptions<ApplicationContext> options)
			: base(options)
		{
			Database.EnsureCreated();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Rate>()
				.HasKey(
				nameof(Rate.ItemId),
				nameof(Rate.UserId));

			modelBuilder.Entity<Topic>()
			.HasOne(s => s.Author)
			.WithMany()
			.HasForeignKey(s => s.AuthorId);

			modelBuilder.Entity<Post>()
				.HasOne(p => p.Author)
				.WithMany()
				.HasForeignKey(p => p.AuthorId);

			modelBuilder.Entity<Post>()
				.HasOne(p => p.Reply)
				.WithMany()
				.HasForeignKey(p => p.ReplyId);

			modelBuilder.Entity<User>()
				.HasMany(e => e.Roles)
				.WithMany(e => e.Users)
				.UsingEntity<IdentityUserRole<Guid>>(
					j => j.HasOne<Role>().WithMany().HasForeignKey(ur => ur.RoleId),
					j => j.HasOne<User>().WithMany().HasForeignKey(ur => ur.UserId),
					j =>
						{
							j.ToTable("AspNetUserRoles");
							j.HasKey(ur => new { ur.UserId, ur.RoleId });
						}
				);
		}
	}
}
