using AspForum.Data.Entities;
using AspForum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AspForum.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger<AccountController> _logger;
		private readonly IEmailSender _emailSender;

		public AccountController(UserManager<User> userManager, 
			SignInManager<User> signInManager, 
			ILogger<AccountController> logger, 
			IEmailSender emailSender)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
			_emailSender = emailSender;
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
		public IActionResult ExternalLogin(string provider, string? returnUrl)
		{
			returnUrl = "/";
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, $"/Account/ExternalLoginRedirect?returnUrl=%2F");
			return new ChallengeResult(provider, properties);
		}

		[HttpGet]
		public async Task<IActionResult> ExternalLoginRedirect(string? returnUrl, string? remoteError)
		{
			returnUrl = returnUrl ?? Url.Content("~/");
			if (remoteError != null)
			{
				//ErrorMessage = $"Error from external provider: {remoteError}";
				return LocalRedirect(returnUrl);
			}
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				//ErrorMessage = "Error loading external login information.";
				return LocalRedirect(returnUrl);
			}

			// Sign in the user with this external login provider if the user already has a login.
			var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
			if (result.Succeeded)
			{
				_logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
				return LocalRedirect(returnUrl);
			}
			if (result.IsLockedOut)
			{
				return LocalRedirect(returnUrl);
			}
			else
			{
				User user = new User { Email = info.Principal.Claims.First(c => c.Type == ClaimTypes.Email).Value, UserName = info.Principal.Identity.Name, };
				var userResult = await _userManager.CreateAsync(user);
				if (userResult.Succeeded)
				{
					userResult = await _userManager.AddLoginAsync(user, info);
					if (userResult.Succeeded)
					{
						_logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

						/*
						var userId = await _userManager.GetUserIdAsync(user);
						var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
						code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
						var callbackUrl = Url.Page(
							"/Account/ConfirmEmail",
							pageHandler: null,
							values: new { area = "Identity", userId = userId, code = code },
							protocol: Request.Scheme);

						await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
							$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
						*/
						await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
						return RedirectToAction("SuccessfulRegistration");
					}
				}
				foreach (var error in userResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return LocalRedirect(returnUrl);
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
