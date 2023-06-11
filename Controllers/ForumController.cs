using AspForum.Data;
using AspForum.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspForum.Controllers
{
	public class ForumController : Controller
	{
		private readonly ApplicationContext _context;

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

		public async Task<IActionResult> CreateSection(SectionFormViewModel model)
		{
			if (ModelState.IsValid)
			{
				_context.Sections.Add(new Data.Entities.Section()
				{
					Title = model.Title,
				});
				await _context.SaveChangesAsync();
			}
			return RedirectToAction("Index");
		}
	}
}
