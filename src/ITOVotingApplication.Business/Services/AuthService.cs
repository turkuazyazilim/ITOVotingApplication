using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Auth;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.User;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ITOVotingApplication.Business.Services
{
	public class AuthService : IAuthService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IConfiguration _configuration;

		public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_configuration = configuration;
		}

		public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
		{
			try
			{
				// Find user by username
				var user = await _unitOfWork.Users.Query()
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.Role)
					.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName && u.IsActive);

				if (user == null)
				{
					return ApiResponse<LoginResponseDto>.ErrorResult("Kullanıcı adı veya şifre hatalı.");
				}

				// Verify password
				if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
				{
					return ApiResponse<LoginResponseDto>.ErrorResult("Kullanıcı adı veya şifre hatalı.");
				}

				// Generate tokens
				var token = GenerateJwtToken(user);
				var refreshToken = GenerateRefreshToken();

				// Create response
				var userDto = _mapper.Map<UserDto>(user);
				var response = new LoginResponseDto
				{
					Token = token,
					RefreshToken = refreshToken,
					ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])),
					User = userDto
				};

				return ApiResponse<LoginResponseDto>.SuccessResult(response, "Giriş başarılı.");
			}
			catch (Exception ex)
			{
				return ApiResponse<LoginResponseDto>.ErrorResult($"Giriş hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto registerDto)
		{
			try
			{
				// Check if username exists
				var existingUser = await _unitOfWork.Users
					.SingleOrDefaultAsync(u => u.UserName == registerDto.UserName);

				if (existingUser != null)
				{
					return ApiResponse<UserDto>.ErrorResult("Bu kullanıcı adı zaten kullanılmaktadır.");
				}

				// Check if email exists
				existingUser = await _unitOfWork.Users
					.SingleOrDefaultAsync(u => u.Email == registerDto.Email);

				if (existingUser != null)
				{
					return ApiResponse<UserDto>.ErrorResult("Bu e-posta adresi zaten kullanılmaktadır.");
				}

				// Create new user
				var user = new User
				{
					UserName = registerDto.UserName,
					PasswordHash = CreatePasswordHash(registerDto.Password),
					Email = registerDto.Email,
					FirstName = registerDto.FirstName,
					LastName = registerDto.LastName,
					IsActive = true
				};

				await _unitOfWork.Users.AddAsync(user);
				await _unitOfWork.CompleteAsync();

				// Assign default role (User)
				var userRole = await _unitOfWork.Roles
					.SingleOrDefaultAsync(r => r.RoleDescription == "User" && r.IsActive);

				if (userRole != null)
				{
					var userRoleMapping = new UserRole
					{
						UserId = user.Id,
						RoleId = userRole.Id,
						IsActive = true
					};

					await _unitOfWork.UserRoles.AddAsync(userRoleMapping);
					await _unitOfWork.CompleteAsync();
				}

				var result = _mapper.Map<UserDto>(user);
				return ApiResponse<UserDto>.SuccessResult(result, "Kayıt başarılı.");
			}
			catch (Exception ex)
			{
				return ApiResponse<UserDto>.ErrorResult($"Kayıt hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
		{
			try
			{
				var user = await _unitOfWork.Users.GetByIdAsync(userId);

				if (user == null)
				{
					return ApiResponse<bool>.ErrorResult("Kullanıcı bulunamadı.");
				}

				// Verify current password
				if (!VerifyPasswordHash(changePasswordDto.CurrentPassword, user.PasswordHash))
				{
					return ApiResponse<bool>.ErrorResult("Mevcut şifre hatalı.");
				}

				// Check if new password and confirm password match
				if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
				{
					return ApiResponse<bool>.ErrorResult("Yeni şifre ve şifre tekrarı uyuşmuyor.");
				}

				// Update password
				user.PasswordHash = CreatePasswordHash(changePasswordDto.NewPassword);
				_unitOfWork.Users.Update(user);
				await _unitOfWork.CompleteAsync();

				return ApiResponse<bool>.SuccessResult(true, "Şifre başarıyla değiştirildi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<bool>.ErrorResult($"Şifre değiştirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
		{
			try
			{
				// In a real application, you would validate the refresh token from a stored location
				// For demo purposes, we'll generate a new token

				// This is a simplified implementation
				// In production, you should:
				// 1. Store refresh tokens in database
				// 2. Validate the refresh token
				// 3. Check expiration
				// 4. Generate new access token and refresh token

				return ApiResponse<LoginResponseDto>.ErrorResult("Refresh token özelliği henüz implementasyonu tamamlanmamıştır.");
			}
			catch (Exception ex)
			{
				return ApiResponse<LoginResponseDto>.ErrorResult($"Token yenileme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<bool>> LogoutAsync(int userId)
		{
			try
			{
				// In a real application, you would:
				// 1. Invalidate the refresh token
				// 2. Add the JWT to a blacklist (if implementing JWT blacklisting)
				// 3. Clear any server-side session data

				// For now, just return success as logout is handled client-side
				return ApiResponse<bool>.SuccessResult(true, "Çıkış başarılı.");
			}
			catch (Exception ex)
			{
				return ApiResponse<bool>.ErrorResult($"Çıkış hatası: {ex.Message}");
			}
		}

		// Helper Methods
		private string GenerateJwtToken(User user)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim("FullName", user.FullName)
			};

			// Add roles to claims
			foreach (var userRole in user.UserRoles.Where(ur => ur.IsActive))
			{
				claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleDescription));
			}

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])),
				Issuer = _configuration["JwtSettings:Issuer"],
				Audience = _configuration["JwtSettings:Audience"],
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		private string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumber);
				return Convert.ToBase64String(randomNumber);
			}
		}

		private string CreatePasswordHash(string password)
		{
			using (var sha256 = SHA256.Create())
			{
				var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(hashedBytes);
			}
		}

		private bool VerifyPasswordHash(string password, string storedHash)
		{
			var hash = CreatePasswordHash(password);
			return hash == storedHash;
		}
	}
}
