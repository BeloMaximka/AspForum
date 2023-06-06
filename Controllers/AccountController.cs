using AspForum.Data.Entities;
using AspForum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspForum.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;

		public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult SuccessfulRegistration()
		{
			return View();
		}

		[HttpPost]
		public async Task<RegistrationResult> Register(RegisterViewModel model)
		{
			RegistrationResult result = new();
			if (ModelState.IsValid && model.IsAgree)
			{
				User user = new User { Email = model.Email, UserName = model.Username, };
				var userResult = await _userManager.CreateAsync(user, model.Password);

				if (userResult.Succeeded)
				{
					await _signInManager.SignInAsync(user, false);
					result.Success = true;
					return result;
				}
				foreach (var error in userResult.Errors)
				{
					result.Errors.Add(error.Description);
				}
			}
			return result;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<LoginResult> Login(LoginViewModel model)
		{
			LoginResult response = new();
			if (ModelState.IsValid)
			{
				var result =
					await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
				if (result.Succeeded)
				{
					response.Success = true;
				}
				else
				{
					response.Message = "Incorrect username or password";
				}
			}
			return response;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
	}
}
