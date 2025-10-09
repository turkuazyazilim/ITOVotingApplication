using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Auth;
using ITOVotingApplication.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ITOVotingApplication.Web.Controllers
{
	public class AuthController : Controller
	{
		private readonly IAuthService _authService;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthController> _logger;

		public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
		{
			_authService = authService;
			_configuration = configuration;
			_logger = logger;
		}

		// GET: /Auth/Login
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Login(string returnUrl = null)
		{
			// Eğer kullanıcı zaten giriş yapmışsa role göre yönlendir
			if (User.Identity.IsAuthenticated)
			{
				if (User.IsInRole("Saha Görevlisi"))
				{
					return RedirectToAction("Index", "FieldWorker");
				}
				return RedirectToAction("Index", "Dashboard");
			}

			ViewBag.ReturnUrl = returnUrl;
			return View(new LoginViewModel());
		}

		// POST: /Auth/Login (MVC Form Submit için)
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var loginDto = new LoginDto
				{
					UserName = model.UserName,
					Password = model.Password
				};

				var loginResponse = await _authService.LoginAsync(loginDto);

				if (loginResponse != null && loginResponse.Success && loginResponse.Data?.User != null)
				{
					// Cookie authentication için claims oluştur
					var claims = new List<Claim>
					{
						new Claim(ClaimTypes.NameIdentifier, loginResponse.Data.User.Id.ToString()),
						new Claim(ClaimTypes.Name, loginResponse.Data.User.UserName),
						new Claim(ClaimTypes.Email, loginResponse.Data.User.Email ?? ""),
						new Claim(ClaimTypes.MobilePhone, loginResponse.Data.User.PhoneNumber ?? ""),
						new Claim("FullName", $"{loginResponse.Data.User.FirstName} {loginResponse.Data.User.LastName}")
					};

					// Rolleri ekle
					if (loginResponse.Data.User.Roles != null && loginResponse.Data.User.Roles.Any())
					{
						foreach (var role in loginResponse.Data.User.Roles)
						{
							claims.Add(new Claim(ClaimTypes.Role, role));
						}
					}

					var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
					var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

					// Cookie ile giriş yap
					var authProperties = new AuthenticationProperties
					{
						IsPersistent = model.RememberMe,
						ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
					};

					await HttpContext.SignInAsync(
						CookieAuthenticationDefaults.AuthenticationScheme,
						claimsPrincipal,
						authProperties);

					// Token'ı session'a kaydet (opsiyonel - API çağrıları için kullanılabilir)
					HttpContext.Session.SetString("JWTToken", loginResponse.Data.Token);

					// ReturnUrl varsa oraya, yoksa role göre yönlendir
					if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					{
						return Redirect(returnUrl);
					}

					// Role göre yönlendirme
					if (loginResponse.Data.User.Roles != null && loginResponse.Data.User.Roles.Contains("Saha Görevlisi"))
					{
						return RedirectToAction("Index", "FieldWorker");
					}

					return RedirectToAction("Index", "Dashboard");
				}

				ModelState.AddModelError("", loginResponse?.Message ?? "Kullanıcı adı veya şifre hatalı!");
				ViewBag.ReturnUrl = returnUrl;
				return View(model);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Login error");
				ModelState.AddModelError("", "Giriş yapılırken bir hata oluştu!");
				ViewBag.ReturnUrl = returnUrl;
				return View(model);
			}
		}

		// POST: /api/auth/login (AJAX/API için)
		[HttpPost("api/auth/login")]
		[AllowAnonymous]
		public async Task<IActionResult> ApiLogin([FromBody] LoginDto loginDto)
		{
			try
			{
				if (loginDto == null || string.IsNullOrEmpty(loginDto.UserName) || string.IsNullOrEmpty(loginDto.Password))
				{
					return BadRequest(new
					{
						success = false,
						message = "Kullanıcı adı ve şifre zorunludur!"
					});
				}

				var loginResponse = await _authService.LoginAsync(loginDto);

				if (loginResponse != null && loginResponse.Data.User != null)
				{
					return Ok(new
					{
						success = true,
						data = new
						{
							token = loginResponse.Data.Token,
							refreshToken = loginResponse.Data.RefreshToken,
							expiresAt = loginResponse.Data.ExpiresAt,
							user = new
							{
								id = loginResponse.Data.User.Id,
								username = loginResponse.Data.User.UserName,
								firstName = loginResponse.Data.User.FirstName,
								lastName = loginResponse.Data.User.LastName,
								fullName = $"{loginResponse.Data.User.FirstName} {loginResponse.Data.User.LastName}",
								email = loginResponse.Data.User.Email,
								roles = loginResponse.Data.User.Roles ?? new List<string>()
							}
						},
						message = "Giriş başarılı"
					});
				}

				return Ok(new
				{
					success = false,
					message = "Kullanıcı adı veya şifre hatalı!"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "API Login error for user: {UserName}", loginDto?.UserName);
				return Ok(new
				{
					success = false,
					message = "Giriş yapılırken bir hata oluştu!"
				});
			}
		}

		// POST: /api/auth/register (Kayıt için)
		[HttpPost("api/auth/register")]
		[AllowAnonymous]
		public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(new
					{
						success = false,
						message = "Geçersiz veri!",
						errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
					});
				}

				var result = await _authService.RegisterAsync(registerDto);

				if (result.Success)
				{
					return Ok(new
					{
						success = true,
						message = "Kayıt başarılı! Giriş yapabilirsiniz."
					});
				}

				return Ok(new
				{
					success = false,
					message = "Kayıt işlemi başarısız! Kullanıcı adı zaten kullanılıyor olabilir."
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Register error");
				return Ok(new
				{
					success = false,
					message = "Kayıt yapılırken bir hata oluştu!"
				});
			}
		}

		// POST: /api/auth/change-password (Şifre değiştirme)
		[HttpPost("api/auth/change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
		{
			try
			{
				if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
				{
					return BadRequest(new
					{
						success = false,
						message = "Yeni şifreler eşleşmiyor!"
					});
				}

				var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

				var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

				if (result.Success)
				{
					return Ok(new
					{
						success = true,
						message = "Şifre başarıyla değiştirildi!"
					});
				}

				return Ok(new
				{
					success = false,
					message = "Mevcut şifre hatalı!"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Change password error");
				return Ok(new
				{
					success = false,
					message = "Şifre değiştirirken bir hata oluştu!"
				});
			}
		}

		// POST: /api/auth/refresh-token (Token yenileme)
		[HttpPost("api/auth/refresh-token")]
		[AllowAnonymous]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
		{
			try
			{
				var result = await _authService.RefreshTokenAsync(request.RefreshToken);

				if (result != null)
				{
					return Ok(new
					{
						success = true,
						data = new
						{
							token = result.Data.Token,
							refreshToken = result.Data.RefreshToken,
							expiresAt = result.Data.ExpiresAt
						}
					});
				}

				return Unauthorized(new
				{
					success = false,
					message = "Geçersiz refresh token!"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Refresh token error");
				return Ok(new
				{
					success = false,
					message = "Token yenileme başarısız!"
				});
			}
		}

		// GET: /Auth/Logout
		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			try
			{
				// Clear authentication cookie
				await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				
				// Clear all session data
				HttpContext.Session.Clear();
				
				// Clear any response cookies
				foreach (var cookie in Request.Cookies.Keys)
				{
					Response.Cookies.Delete(cookie);
				}
				
				_logger.LogInformation("User logged out successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during logout");
			}
			
			return RedirectToAction("Login");
		}

		// POST: /api/auth/logout (API için)
		[HttpPost("api/auth/logout")]
		[Authorize]
		public async Task<IActionResult> ApiLogout()
		{
			try
			{
				var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

				// Refresh token'ı geçersiz kıl (eğer bu özellik varsa)
				await _authService.LogoutAsync(userId);

				return Ok(new
				{
					success = true,
					message = "Çıkış başarılı"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "API Logout error");
				return Ok(new
				{
					success = true,
					message = "Çıkış yapıldı"
				});
			}
		}

		// GET: /Auth/AccessDenied
		[HttpGet]
		public IActionResult AccessDenied()
		{
			return View();
		}

		// GET: /Auth/Register (Kayıt sayfası)
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register()
		{
			if (User.Identity.IsAuthenticated)
			{
				if (User.IsInRole("Saha Görevlisi"))
				{
					return RedirectToAction("Index", "FieldWorker");
				}
				return RedirectToAction("Index", "Dashboard");
			}

			return View();
		}
	}

	// Request modelleri
	public class RefreshTokenRequest
	{
		public string RefreshToken { get; set; }
	}
}