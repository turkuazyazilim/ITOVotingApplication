using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.User;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ITOVotingApplication.Business.Services
{
	public class UserService : IUserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UserService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public async Task<ApiResponse<int>> GetActiveUserCountAsync()
		{
			try
			{
				var count = await _unitOfWork.Users
					.Query()
					.Where(u => u.IsActive)
					.CountAsync();

				return ApiResponse<int>.SuccessResult(count);
			}
			catch (Exception ex)
			{
				return ApiResponse<int>.ErrorResult($"Aktif kullanıcı sayısı getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<int>> GetTotalUserCountAsync()
		{
			try
			{
				var count = await _unitOfWork.Users
					.Query()
					.CountAsync();

				return ApiResponse<int>.SuccessResult(count);
			}
			catch (Exception ex)
			{
				return ApiResponse<int>.ErrorResult($"Toplam kullanıcı sayısı getirme hatası: {ex.Message}");
			}
		}
		public async Task<ApiResponse<UserDto>> GetByIdAsync(int id)
		{
			try
			{
				var user = await _unitOfWork.Users.Query()
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.Role)
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.FieldReferenceCategory)
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.FieldReferenceSubCategory)
					.FirstOrDefaultAsync(u => u.Id == id);

				if (user == null)
				{
					return ApiResponse<UserDto>.ErrorResult("Kullanıcı bulunamadı.");
				}

				var result = _mapper.Map<UserDto>(user);
				return ApiResponse<UserDto>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<UserDto>.ErrorResult($"Kullanıcı getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<PagedResult<UserDto>>> GetAllAsync(PagedRequest request)
		{
			try
			{
				var query = _unitOfWork.Users.Query()
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.Role)
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.FieldReferenceCategory)
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.FieldReferenceSubCategory)
					.AsQueryable();

				// Search filter
				if (!string.IsNullOrWhiteSpace(request.SearchTerm))
				{
					query = query.Where(u =>
						u.UserName.Contains(request.SearchTerm) ||
						u.FirstName.Contains(request.SearchTerm) ||
						u.LastName.Contains(request.SearchTerm) ||
						u.Email.Contains(request.SearchTerm));
				}

				// Sorting
				if (!string.IsNullOrWhiteSpace(request.SortBy))
				{
					query = request.SortBy.ToLower() switch
					{
						"username" => request.IsDescending ?
							query.OrderByDescending(u => u.UserName) :
							query.OrderBy(u => u.UserName),
						"name" => request.IsDescending ?
							query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName) :
							query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
						"email" => request.IsDescending ?
							query.OrderByDescending(u => u.Email) :
							query.OrderBy(u => u.Email),
						_ => query.OrderBy(u => u.Id)
					};
				}
				else
				{
					query = query.OrderBy(u => u.UserName);
				}

				var totalCount = await query.CountAsync();

				var users = await query
					.Skip((request.PageNumber - 1) * request.PageSize)
					.Take(request.PageSize)
					.ToListAsync();

				var result = new PagedResult<UserDto>
				{
					Items = _mapper.Map<List<UserDto>>(users),
					TotalCount = totalCount,
					PageNumber = request.PageNumber,
					PageSize = request.PageSize
				};

				return ApiResponse<PagedResult<UserDto>>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<PagedResult<UserDto>>.ErrorResult($"Kullanıcı listesi getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto)
		{
			try
			{
				// Check if username exists
				var existingUser = await _unitOfWork.Users
					.SingleOrDefaultAsync(u => u.UserName == dto.UserName);

				if (existingUser != null)
				{
					return ApiResponse<UserDto>.ErrorResult("Bu kullanıcı adı zaten kullanılmaktadır.");
				}

				// Check if email exists
				existingUser = await _unitOfWork.Users
					.SingleOrDefaultAsync(u => u.Email == dto.Email);

				if (existingUser != null)
				{
					return ApiResponse<UserDto>.ErrorResult("Bu e-posta adresi zaten kullanılmaktadır.");
				}

				// Create user
				var user = new User
				{
					UserName = dto.UserName,
					PasswordHash = CreatePasswordHash(dto.Password),
					Email = dto.Email,
					PhoneNumber = dto.PhoneNumber,
					FirstName = dto.FirstName,
					LastName = dto.LastName,
					IsActive = true
				};

				await _unitOfWork.Users.AddAsync(user);
				await _unitOfWork.CompleteAsync();

				// Assign roles
				if (dto.RoleIds != null && dto.RoleIds.Any())
				{
					foreach (var roleId in dto.RoleIds)
					{
						var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
						if (role != null && role.IsActive)
						{
							var userRole = new UserRole
							{
								UserId = user.Id,
								RoleId = roleId,
								IsActive = true,
								FieldReferenceCategoryId = dto.FieldReferenceCategoryId,
								FieldReferenceSubCategoryId = dto.FieldReferenceSubCategoryId
							};
							await _unitOfWork.UserRoles.AddAsync(userRole);
						}
					}
					await _unitOfWork.CompleteAsync();
				}

				var createdUser = await _unitOfWork.Users.Query()
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.Role)
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.FieldReferenceCategory)
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.FieldReferenceSubCategory)
					.FirstOrDefaultAsync(u => u.Id == user.Id);

				var result = _mapper.Map<UserDto>(createdUser);
				return ApiResponse<UserDto>.SuccessResult(result, "Kullanıcı başarıyla oluşturuldu.");
			}
			catch (Exception ex)
			{
				return ApiResponse<UserDto>.ErrorResult($"Kullanıcı oluşturma hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<UserDto>> UpdateAsync(UpdateUserDto dto)
		{
			try
			{
				var user = await _unitOfWork.Users.Query()
					.Include(u => u.UserRoles)
					.FirstOrDefaultAsync(u => u.Id == dto.Id);

				if (user == null)
				{
					return ApiResponse<UserDto>.ErrorResult("Kullanıcı bulunamadı.");
				}

				// Check if email is being changed and if it's already in use
				if (user.Email != dto.Email)
				{
					var existingUser = await _unitOfWork.Users
						.SingleOrDefaultAsync(u => u.Email == dto.Email && u.Id != dto.Id);

					if (existingUser != null)
					{
						return ApiResponse<UserDto>.ErrorResult("Bu e-posta adresi başka bir kullanıcı tarafından kullanılmaktadır.");
					}
				}

				// Update user properties
				user.Email = dto.Email;
				user.FirstName = dto.FirstName;
				user.LastName = dto.LastName;
				user.IsActive = dto.IsActive;
				user.PhoneNumber = dto.PhoneNumber;

				// Update roles
				if (dto.RoleIds != null)
				{
					// Remove existing roles
					var existingRoles = user.UserRoles.ToList();
					foreach (var existingRole in existingRoles)
					{
						_unitOfWork.UserRoles.Remove(existingRole);
					}

					// Add new roles
					foreach (var roleId in dto.RoleIds)
					{
						var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
						if (role != null && role.IsActive)
						{
							var userRole = new UserRole
							{
								UserId = user.Id,
								RoleId = roleId,
								IsActive = true
							};
							await _unitOfWork.UserRoles.AddAsync(userRole);
						}
					}
				}

				_unitOfWork.Users.Update(user);
				await _unitOfWork.CompleteAsync();

				var updatedUser = await _unitOfWork.Users.Query()
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.Role)
					.FirstOrDefaultAsync(u => u.Id == user.Id);

				var result = _mapper.Map<UserDto>(updatedUser);
				return ApiResponse<UserDto>.SuccessResult(result, "Kullanıcı başarıyla güncellendi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<UserDto>.ErrorResult($"Kullanıcı güncelleme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<bool>> DeleteAsync(int id)
		{
			try
			{
				var user = await _unitOfWork.Users.GetByIdAsync(id);

				if (user == null)
				{
					return ApiResponse<bool>.ErrorResult("Kullanıcı bulunamadı.");
				}

				// Check if user has created any vote transactions
				var hasVoteTransactions = await _unitOfWork.VoteTransactions
					.Query()
					.AnyAsync(v => v.CreatedUserId == id);

				if (hasVoteTransactions)
				{
					// Soft delete
					user.IsActive = false;
					_unitOfWork.Users.Update(user);
				}
				else
				{
					// Hard delete
					_unitOfWork.Users.Remove(user);
				}

				await _unitOfWork.CompleteAsync();
				return ApiResponse<bool>.SuccessResult(true, "Kullanıcı başarıyla silindi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<bool>.ErrorResult($"Kullanıcı silme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<UserDto>> GetByUserNameAsync(string userName)
		{
			try
			{
				var user = await _unitOfWork.Users.Query()
					.Include(u => u.UserRoles)
						.ThenInclude(ur => ur.Role)
					.FirstOrDefaultAsync(u => u.UserName == userName);

				if (user == null)
				{
					return ApiResponse<UserDto>.ErrorResult("Kullanıcı bulunamadı.");
				}

				var result = _mapper.Map<UserDto>(user);
				return ApiResponse<UserDto>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<UserDto>.ErrorResult($"Kullanıcı getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<bool>> AssignRoleAsync(int userId, List<int> roleIds)
		{
			try
			{
				var user = await _unitOfWork.Users.Query()
					.Include(u => u.UserRoles)
					.FirstOrDefaultAsync(u => u.Id == userId);

				if (user == null)
				{
					return ApiResponse<bool>.ErrorResult("Kullanıcı bulunamadı.");
				}

				// Remove existing roles
				var existingRoles = user.UserRoles.ToList();
				foreach (var existingRole in existingRoles)
				{
					_unitOfWork.UserRoles.Remove(existingRole);
				}

				// Add new roles
				foreach (var roleId in roleIds)
				{
					var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
					if (role != null && role.IsActive)
					{
						var userRole = new UserRole
						{
							UserId = userId,
							RoleId = roleId,
							IsActive = true
						};
						await _unitOfWork.UserRoles.AddAsync(userRole);
					}
				}

				await _unitOfWork.CompleteAsync();
				return ApiResponse<bool>.SuccessResult(true, "Roller başarıyla atandı.");
			}
			catch (Exception ex)
			{
				return ApiResponse<bool>.ErrorResult($"Rol atama hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<List<string>>> GetUserRolesAsync(int userId)
		{
			try
			{
				var userRoles = await _unitOfWork.UserRoles.Query()
					.Include(ur => ur.Role)
					.Where(ur => ur.UserId == userId && ur.IsActive)
					.Select(ur => ur.Role.RoleDescription)
					.ToListAsync();

				return ApiResponse<List<string>>.SuccessResult(userRoles);
			}
			catch (Exception ex)
			{
				return ApiResponse<List<string>>.ErrorResult($"Kullanıcı rolleri getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<bool>> ValidateUserAsync(string userName, string password)
		{
			try
			{
				var user = await _unitOfWork.Users
					.SingleOrDefaultAsync(u => u.UserName == userName && u.IsActive);

				if (user == null)
				{
					return ApiResponse<bool>.SuccessResult(false);
				}

				var isValid = VerifyPasswordHash(password, user.PasswordHash);
				return ApiResponse<bool>.SuccessResult(isValid);
			}
			catch (Exception ex)
			{
				return ApiResponse<bool>.ErrorResult($"Kullanıcı doğrulama hatası: {ex.Message}");
			}
		}

		// Helper Methods
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