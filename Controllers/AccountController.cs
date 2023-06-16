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
using Microsoft.EntityFrameworkCore;
using AspForum.Data;
using AspForum.Services.Email;
using Microsoft.AspNetCore.Authorization;
using AspForum.Models.Account;
using AspForum.Models.Home;

namespace AspForum.Controllers
{
	public class AccountController : Controller
	{
		private readonly string[] allowedExtensions = { ".png", ".jpeg", ".jpg" };
		private readonly ApplicationContext _context;
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<Role> _roleManager;
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger<AccountController> _logger;
		private readonly IEmailService _emailService;

		public AccountController(
            UserManager<User> userManager,
			SignInManager<User> signInManager,
			ILogger<AccountController> logger,
			ApplicationContext context,
			RoleManager<Role> roleManager,
			IEmailService emailService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
			_context = context;
			_roleManager = roleManager;
			_emailService = emailService;
		}

        #region Pages
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EmailConfirmed()
        {
            return View();
        }

        public async Task<IActionResult> Profile([FromRoute] Guid id)
        {
            User? user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(t => t.Id == id);
            if (user == null)
            {
                return RedirectToAction("PageNotFound", "Home");
            }

            ProfileViewModel model = new()
            {
                UserName = user.UserName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Roles[0].Name
            };
            return View(model);
        }

        public IActionResult Manage()
        {
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            {
                return View();
            }
            return RedirectToAction("PageNotFound", "Home");
        }

        public IActionResult SuccessfulRegistration()
        {
            return View();
        }

		public IActionResult EmailChange()
		{
			return View();
		}

		public IActionResult EmailChangeSuccess()
		{
			return View();
		}
		#endregion

		#region ResitrationLoginLogout
		[HttpPost]
        public async Task<ResultWithManyErrorMessages> Register(RegisterViewModel model)
        {
            ResultWithManyErrorMessages result = new();
            if (ModelState.IsValid && model.IsAgree)
            {
                User user = new User { Email = model.Email, UserName = model.Username, };
                var userResult = await _userManager.CreateAsync(user, model.Password);

                if (userResult.Succeeded)
                {
                    await FinishRegistration(user);
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
            if (info == null || info.Principal.Identity is null)
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
                        await FinishRegistration(user);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

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
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ResultWithManyErrorMessages> RequestChangeEmail(string newEmail)
        {
            ResultWithManyErrorMessages result = new();
            if(!ModelState.IsValid)
            {
                result.Errors.Add("All fields are reqiured.");
                return result;
            }
            if (User.Identity is not null && User.Identity.IsAuthenticated &&
                await _userManager.FindByNameAsync(User.Identity.Name) is User user)
            {
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
                var callbackUrl = Url.Action(
                    "SendEmailChangeConfirmation",
                    "Account",
                    new { userId = user.Id, NewEmail = newEmail, code },
                    protocol: HttpContext.Request.Scheme);
                if (callbackUrl == null)
                {
                    result.Errors.Add("Unexpected error.");
                    return result;
                }
                Models.EmailTemplates.ChangeEmailRequestModel model = new()
                {
                    Email = user.Email,
                    NewEmail = newEmail,
                    UserName = user.UserName,
                    ChangeEmailLink = callbackUrl
                };
                await _emailService.SendAsync("ChangeEmailRequest", model);
                result.Success = true;
            }
            else
            {
                result.Errors.Add("Unauthorized");
            }
            return result;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ChangeEmail(string userId, string newEmail, string changeCode, string confirmationCode)
        {
            if (await _userManager.FindByIdAsync(userId) is User user)
            {
				if (!await _userManager.VerifyUserTokenAsync(user, 
                    _userManager.Options.Tokens.EmailConfirmationTokenProvider, 
                    UserManager<User>.ConfirmEmailTokenPurpose, confirmationCode))
                {
					ErrorMessageViewModel model = new()
					{
						ErrorMessage = "Confirmation code is invalid"
					};
					return RedirectToAction("ErrorMessage", "Home", model);
				}
				
                var result = await _userManager.ChangeEmailAsync(user, newEmail, changeCode);
                if(!result.Succeeded)
                {
                    ErrorMessageViewModel model = new()
                    {
                        ErrorMessage = "Change code is invalid"
                    };
                    return RedirectToAction("ErrorMessage", "Home", model);
                }
                
                result = await _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user));
                if (!result.Succeeded)
                {
                    ErrorMessageViewModel model = new()
                    {
                        ErrorMessage = "Unexpected error"
                    };
                    return RedirectToAction("ErrorMessage", "Home", model);
                }
				await _signInManager.RefreshSignInAsync(user);
				return RedirectToAction("EmailChangeSuccess");
            }
            else
            {
                ErrorMessageViewModel model = new()
                {
                    ErrorMessage = "User id is invalid"
                };
                return RedirectToAction("ErrorMessage", "Home", model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendEmailChangeConfirmation(string userId, string newEmail, string code)
        {
            if(await _userManager.FindByIdAsync(userId) is User user)
            {
                if (await _userManager.VerifyUserTokenAsync(user, 
                    _userManager.Options.Tokens.ChangeEmailTokenProvider, 
                    UserManager<User>.GetChangeEmailTokenPurpose(newEmail), code))
                {
                    var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ChangeEmail",
                        "Account",
                        new { userId = user.Id, NewEmail = newEmail, ChangeCode = code, ConfirmationCode = confirmationCode },
                        protocol: HttpContext.Request.Scheme);
                    if (callbackUrl == null)
                    {
                        ErrorMessageViewModel errorModel = new()
                        {
                            ErrorMessage = "Unexpected error."
                        };
                        return RedirectToAction("ErrorMessage", "Home", errorModel);
                    }
                    Models.EmailTemplates.СonfirmEmailChange model = new()
                    {
                        OldEmail = user.Email,
                        Email = newEmail,
                        UserName = user.UserName,
                        ChangeEmailLink = callbackUrl
                    };
                    await _emailService.SendAsync("ConfirmEmailChange", model);
                    return RedirectToAction("EmailChange");
                }
                else
                {
                    ErrorMessageViewModel model = new()
                    {
                        ErrorMessage = "Code is invalid"
                    };
                    return RedirectToAction("ErrorMessage", "Home", model);
                }
            }
            else
            {
                ErrorMessageViewModel model = new()
                {
                    ErrorMessage = "User id is invalid"
                };
                return RedirectToAction("ErrorMessage", "Home", model);
            }
        }

        [HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string code)
		{
			// TODO BETTER ERROR HANDLING
			if (userId == null || code == null)
			{
				return RedirectToAction("PageNotFound", "Home");
			}
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return RedirectToAction("PageNotFound", "Home");
			}
			var result = await _userManager.ConfirmEmailAsync(user, code);
			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, "User");
				await _userManager.RemoveFromRoleAsync(user, "NotConfirmed");
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("EmailConfirmed");
			}
			else
			{
				return RedirectToAction("PageNotFound", "Home");
			}
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ResultWithManyErrorMessages> ChangePassword(ChangePasswordFormViewModel model)
        {
            ResultWithManyErrorMessages result = new();
            if(!ModelState.IsValid)
            {
                result.Errors.Add("All field are required");
                return result;
            }
            if (User.Identity is not null && User.Identity.IsAuthenticated &&
                await _userManager.FindByNameAsync(User.Identity.Name) is User user)
            {
                // Verify new passwords match
                if(model.NewPassword != model.NewPasswordRepeat)
                {
                    result.Errors.Add("New password mismatch");
                    return result;
                }

                // Try to change password
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        result.Errors.Add(error.Description);
                    }
                    return result;
                }

                result.Success = true;
            }
            else
            {
                result.Errors.Add("Not authorized.");
            }
            return result;
        }

		[HttpPost]
		public async Task<IActionResult> UploadAvatar(IFormFile file)
		{
			if (User.Identity is not null && User.Identity.IsAuthenticated && file != null)
			{
				User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
				string? ext = Path.GetExtension(file.FileName);
				if (user is not null && ext is not null && 
					file.Length <= 1048576 && allowedExtensions.Any(e => e == ext)) // 1 MB
				{
					try
					{
						string shortPath = $"{User.Identity.Name}{ext}";
						string path = $"wwwroot/img/avatars/{shortPath}";

						string oldPath = $"wwwroot/img/avatars/{user.AvatarUrl}";
						if (user.AvatarUrl is not null && System.IO.File.Exists(oldPath))
						{
							System.IO.File.Delete(oldPath);
						}

						using FileStream fs = new(path, FileMode.Create);
						await file.CopyToAsync(fs);

						user.AvatarUrl = shortPath;
						await _context.SaveChangesAsync();

						var claims = await _userManager.GetClaimsAsync(user);
						var avatarClaim = claims.FirstOrDefault(c => c.Type == "Avatar");
						if (avatarClaim != null)
						{
							await _userManager.RemoveClaimAsync(user, avatarClaim);
						}
						await _userManager.AddClaimAsync(user, new Claim("Avatar", shortPath));
						await _userManager.UpdateAsync(user);
						await _signInManager.RefreshSignInAsync(user);
					}
					catch (Exception e)
					{
						_logger.LogError(e.Message);
					}
				}
			}
			return RedirectToAction("Manage");
		}

        // Add claims, send email confirmation letter
        private async Task FinishRegistration(User user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
            };
            if (user.AvatarUrl != null)
            {
                claims.Add(new Claim("Avatar", user.AvatarUrl));
            }
            await _userManager.AddClaimsAsync(user, claims);
            await _userManager.AddToRoleAsync(user, "NotConfirmed");

            // Email confirmation
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, code = code },
                protocol: HttpContext.Request.Scheme);
            if (callbackUrl == null)
            {
                return; // TODO ERROR HANDLING
            }
            Models.EmailTemplates.ConfirmEmailModel model = new()
            {
                Email = user.Email,
                UserName = user.UserName,
                ConfirmLink = callbackUrl
            };
            await _emailService.SendAsync("ConfirmEmail", model);
        }
    }
}
