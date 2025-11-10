using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.User;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITOVotingApplication.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IEmailService _emailService;
        private readonly IUserInvitationService _invitationService;

        public UserController(IUserService userService, IUnitOfWork unitOfWork, IMapper mapper, IWhatsAppService whatsAppService, IEmailService emailService, IUserInvitationService invitationService)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _whatsAppService = whatsAppService;
            _emailService = emailService;
            _invitationService = invitationService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetUsers([FromQuery] PagedRequest? request)
        {
            try
            {
                // Create new request if null or set defaults
                request ??= new PagedRequest();
                
                if (request.PageSize <= 0) request.PageSize = 500;
                if (request.PageNumber <= 0) request.PageNumber = 1;
                request.SearchTerm ??= "";
                request.SortBy ??= "";

                var result = await _userService.GetAllAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Kullanıcılar yüklenirken hata oluştu", data = new { items = new List<object>() } });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ApiResponse<UserDto>> GetByIdAsync(int id)
        {
            return await _userService.GetByIdAsync(id);
        }

        [HttpPost]
        public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto)
        {
            return await _userService.CreateAsync(dto);
        }

        [HttpPut("{id:int}")]
        public async Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserDto dto)
        {
            // Set the ID from route parameter
            dto.Id = id;
            return await _userService.UpdateAsync(dto);
        }

        [HttpDelete("{id:int}")]
        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Kullanıcı bulunamadı.");
                }

                // Soft delete
                user.IsActive = false;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                return ApiResponse<bool>.SuccessResult(true, "Kullanıcı başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Kullanıcı silme hatası: {ex.Message}");
            }
        }

        [HttpGet("active")]
        public async Task<ApiResponse<List<UserDto>>> GetActiveUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.Query()
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                var result = _mapper.Map<List<UserDto>>(users);
                return ApiResponse<List<UserDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserDto>>.ErrorResult($"Aktif kullanıcı listesi getirme hatası: {ex.Message}");
            }
        }

        [HttpGet("roles")]
        public async Task<ApiResponse<List<RoleDto>>> GetRolesAsync()
        {
            try
            {
                var roles = await _unitOfWork.Roles.Query()
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.RoleDescription)
                    .ToListAsync();

                var result = roles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    RoleDescription = r.RoleDescription,
                    IsActive = r.IsActive
                }).ToList();

                return ApiResponse<List<RoleDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<RoleDto>>.ErrorResult($"Roller getirme hatası: {ex.Message}");
            }
        }

        [HttpGet("statistics")]
        public async Task<ApiResponse<UserStatisticsDto>> GetStatisticsAsync()
        {
            try
            {
                var statistics = new UserStatisticsDto
                {
                    TotalUsers = await _unitOfWork.Users.Query().CountAsync(),
                    ActiveUsers = await _unitOfWork.Users.Query().Where(u => u.IsActive).CountAsync(),
                    InactiveUsers = await _unitOfWork.Users.Query().Where(u => !u.IsActive).CountAsync(),

                    // Son eklenen kullanıcılar
                    RecentlyAdded = await _unitOfWork.Users.Query()
                        .Take(5)
                        .Select(u => new { u.FirstName, u.LastName, u.UserName })
                        .ToListAsync()
                };

                return ApiResponse<UserStatisticsDto>.SuccessResult(statistics);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserStatisticsDto>.ErrorResult($"İstatistik getirme hatası: {ex.Message}");
            }
        }

        [HttpPost("send-registration-link")]
        public async Task<ApiResponse<object>> SendRegistrationLinkAsync(SendRegistrationLinkDto dto)
        {
            try
            {
                // Get current user ID
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return ApiResponse<object>.ErrorResult("Kullanıcı kimliği bulunamadı");
                }

                // Create invitation token
                var invitationResult = await _invitationService.CreateInvitationAsync(
                    dto.ContactMethod == "email" ? dto.Email : null,
                    dto.ContactMethod == "phone" ? dto.Phone : null,
                    currentUserId,
                    dto.FieldReferenceCategoryId,
                    dto.FieldReferenceSubCategoryId);

                if (!invitationResult.Success)
                {
                    return ApiResponse<object>.ErrorResult($"Davet oluşturulamadı: {invitationResult.Message}");
                }

                // Generate invitation link with token
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
                var registrationLink = $"{baseUrl}/invitation/{invitationResult.Data}";

                if (dto.ContactMethod == "email")
                {
                    // Send email with registration link
                    var emailResult = await _emailService.SendRegistrationLinkAsync(dto.Email, registrationLink);

                    if (emailResult.Success)
                    {
                        // Return structured data for email display
                        var responseData = new
                        {
                            contactMethod = "email",
                            email = dto.Email,
                            registrationLink = registrationLink
                        };

                        return ApiResponse<object>.SuccessResult(responseData, "E-posta başarıyla gönderildi!");
                    }
                    else
                    {
                        return ApiResponse<object>.ErrorResult($"E-posta gönderilemedi: {emailResult.Message}");
                    }
                }
                else if (dto.ContactMethod == "phone")
                {
                    // Prepare WhatsApp message
                    var whatsappResult = await _whatsAppService.SendRegistrationLinkAsync(dto.Phone, registrationLink);

                    if (whatsappResult.Success)
                    {
                        // Return structured data for modal display
                        var responseData = new
                        {
                            contactMethod = "phone",
                            phoneNumber = dto.Phone,
                            message = whatsappResult.Data,
                            registrationLink = registrationLink
                        };

                        return ApiResponse<object>.SuccessResult(responseData, "WhatsApp mesajı hazırlandı!");
                    }
                    else
                    {
                        return ApiResponse<object>.ErrorResult($"WhatsApp mesajı hazırlanamadı: {whatsappResult.Message}");
                    }
                }

                // Invalid contact method
                return ApiResponse<object>.ErrorResult("Geçersiz iletişim yöntemi!");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult($"Link gönderme hatası: {ex.Message}");
            }
        }
    }

    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public object RecentlyAdded { get; set; }
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string RoleDescription { get; set; }
        public bool IsActive { get; set; }
    }

    public class SendRegistrationLinkDto
    {
        public string ContactMethod { get; set; } // "email" or "phone"
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? FieldReferenceCategoryId { get; set; }
        public int? FieldReferenceSubCategoryId { get; set; }
    }
}