using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.User;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public UserController(IUserService userService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            try
            {
                var user = await _unitOfWork.Users.Query()
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
    }

    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public object RecentlyAdded { get; set; }
    }
}