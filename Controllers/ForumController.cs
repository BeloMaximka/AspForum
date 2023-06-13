using AspForum.Data;
using AspForum.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AspForum.Data.Entities;

namespace AspForum.Controllers
{
	public class ForumController : Controller
	{
		private readonly ApplicationContext _context;
		private readonly ILogger<ForumController> _logger;
		private readonly UserManager<User> _userManager;

		public ForumController(ApplicationContext context, 
			ILogger<ForumController> logger,
			UserManager<User> userManager)
		{
			_context = context;
			_logger = logger;
			_userManager = userManager;
		}

		public IActionResult Index()
		{
			ForumIndexViewModel model = new();
			model.Sections = _context.Sections.Select(s => new SectionViewModel()
			{
				Id = s.Id.ToString(),
				Title = s.Title,
				Themes = _context.Themes.Where(t => t.SectionId == s.Id).ToList()
			}).ToList();
			return View(model);
		}

		public async Task<IActionResult> Theme([FromRoute] Guid id)
		{
			var theme = await _context.Themes.FindAsync(id);
			if(theme != null )
			{
                ThemeViewModel model = new()
                {
                    Id = theme.Id.ToString(),
                    Title = theme.Title,
                    Topics = await _context.Topics
						.Where(t => t.ThemeId == id)
						.Select(t => new TopicViewModel()
						{
							Id = t.Id,
							Title = t.Title,
							AuthorName = t.Author.UserName,
							CreationDateString = t.CreatedDt.ToShortDateString(),
						}).ToListAsync()
                };
                return View(model);
            }
			return RedirectToAction("PageNotFound", "Home");
		}
		public async Task<IActionResult> Topic([FromRoute] Guid id)
		{
			var topic = await _context.Topics.Include(p => p.Author).FirstAsync(t => t.Id == id);
			if (topic != null)
			{
				return View(topic);
			}
			return RedirectToAction("PageNotFound", "Home");
		}

		[HttpPost]
		public async Task<IActionResult> CreateSection(SectionFormViewModel model)
		{
			if (ModelState.IsValid && User.Identity is not null && User.Identity.IsAuthenticated )
			{
				_context.Sections.Add(new Section()
				{
					Title = model.Title,
				});
				try
				{
					await _context.SaveChangesAsync();
				}
				catch (Exception e)
				{
					_logger.LogError(e.Message);
				}
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> CreateTheme(ThemeFormViewModel model)
		{
			if (ModelState.IsValid && User.Identity is not null && User.Identity.IsAuthenticated)
			{
				_context.Themes.Add(new Theme()
				{
					SectionId = model.SectionId,
					Title = model.Title,
					Description = model.Description,
				});
				try
				{
					await _context.SaveChangesAsync();
				}
				catch (Exception e)
				{
					_logger.LogError(e.Message);
				}
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> CreateTopic(TopicFormViewModel model)
		{
			if (ModelState.IsValid && User.Identity is not null && User.Identity.IsAuthenticated)
			{
				Guid id = Guid.NewGuid();
				_context.Topics.Add(new Topic()
				{
					Id = id,
					ThemeId = model.ThemeId,
					AuthorId = (await _userManager.FindByNameAsync(User.Identity.Name)).Id,
					Title = model.Title,
					Description = model.Description,
					CreatedDt = DateTime.UtcNow
				});
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction("Topic", new { id});
				}
				catch (Exception e)
				{
					_logger.LogError(e.Message);
				}
			}
			return RedirectToAction("Index");
		}
	}
}