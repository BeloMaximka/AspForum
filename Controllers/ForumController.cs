using AspForum.Data;
using AspForum.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspForum.Controllers
{
	public class ForumController : Controller
	{
		private readonly ApplicationContext _context;
		private readonly ILogger<ForumController> _logger;

		public ForumController(ApplicationContext context)
		{
			_context = context;
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

		[HttpPost]
		public async Task<IActionResult> CreateSection(SectionFormViewModel model)
		{
			if (ModelState.IsValid && User.Identity.IsAuthenticated )
			{
				_context.Sections.Add(new Data.Entities.Section()
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
			if (ModelState.IsValid && User.Identity.IsAuthenticated)
			{
				_context.Themes.Add(new Data.Entities.Theme()
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
	}
}