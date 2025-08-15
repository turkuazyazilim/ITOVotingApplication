using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Auth;
using ITOVotingApplication.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITOVotingApplication.Web.Controllers
{
	public class AccountController : Controller
	{
		private readonly IAuthService _authService;
		private readonly ILogger<AccountController> _logger;

		public AccountController(IAuthService authService, ILogger<AccountController> logger)
		{
			_authService = authService;
			_logger = logger;
		}

		// GET: /Account/Login
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Login(string returnUrl = null)
		{
			// Store the return URL to redirect after successful login
			
			ViewData["ReturnUrl"] = returnUrl;

			// If user is already authenticated, redirect to appropriate page
			if (User.Identity.IsAuthenticated)
			{
				if (User.IsInRole("Admin"))
				{
					return RedirectToAction("Index", "Admin");
				}
				else if (User.IsInRole("SandikGorevlisi") || User.IsInRole("BallotOfficer"))
				{
					return RedirectToAction("Index", "Vote");
				}

				return RedirectToAction("Index", "Home");
			}

			return View(new LoginViewModel());
		}

		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var loginDto = new LoginDto
			{
				UserName = model.UserName,
				Password = model.Password
			};

			var result = await _authService.LoginAsync(loginDto);

			if (result.Success)
			{
				// Create claims for the authenticated user
				var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, result.Data.User.Id.ToString()),
			new Claim(ClaimTypes.Name, result.Data.User.UserName),
			new Claim(ClaimTypes.Email, result.Data.User.Email),
			new Claim("FullName", result.Data.User.FirstName + " " + result.Data.User.LastName)
		};

				// Add role claims
				if (result.Data.User.Roles != null)
				{
					foreach (var role in result.Data.User.Roles)
					{
						claims.Add(new Claim(ClaimTypes.Role, role));
					}
				}

				// Create the identity and principal
				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

				// Sign in the user with cookie authentication
				await HttpContext.SignInAsync(
					CookieAuthenticationDefaults.AuthenticationScheme,
					claimsPrincipal,
					new AuthenticationProperties
					{
						IsPersistent = model.RememberMe,
						ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
					});

				_logger.LogInformation($"User {model.UserName} logged in successfully.");

				// Redirect based on user role - ÖNEMLİ: RedirectToAction kullanın
				if (result.Data.User.Roles != null && result.Data.User.Roles.Contains("Admin"))
				{
					return RedirectToAction("Index", "Dashboard");
				}
				else if (result.Data.User.Roles != null &&
						(result.Data.User.Roles.Contains("SandikGorevlisi") ||
						 result.Data.User.Roles.Contains("BallotOfficer")))
				{
					return RedirectToAction("Index", "Vote");
				}

				// Redirect to return URL or dashboard
				if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}

				return RedirectToAction("Index", "Dashboard");
			}

			// If login failed, show error
			ModelState.AddModelError(string.Empty, result.Message ?? "Kullanıcı adı veya şifre hatalı.");
			return View(model);
		}

		// POST: /Account/Logout
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			_logger.LogInformation($"User {User.Identity.Name} logged out.");

			return RedirectToAction("Login", "Account");
		}

		// GET: /Account/AccessDenied
		[HttpGet]
		public IActionResult AccessDenied()
		{
			return View();
		}

		// GET: /Account/Register (Optional - if registration is needed)
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register()
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}

			return View(new RegisterViewModel());
		}

		// POST: /Account/Register
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var registerDto = new RegisterDto
			{
				UserName = model.UserName,
				Password = model.Password,
				Email = model.Email,
				FirstName = model.FirstName,
				LastName = model.LastName
			};

			var result = await _authService.RegisterAsync(registerDto);

			if (result.Success)
			{
				_logger.LogInformation($"New user {model.UserName} registered successfully.");

				// Optionally auto-login after registration
				return RedirectToAction("Login", new { message = "Kayıt başarılı! Giriş yapabilirsiniz." });
			}

			ModelState.AddModelError(string.Empty, result.Message ?? "Kayıt işlemi başarısız.");
			return View(model);
		}
	}
}