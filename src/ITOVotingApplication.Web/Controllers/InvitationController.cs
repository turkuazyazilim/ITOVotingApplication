using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.User;
using ITOVotingApplication.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITOVotingApplication.Web.Controllers
{
    [AllowAnonymous]
    public class InvitationController : Controller
    {
        private readonly IUserInvitationService _invitationService;
        private readonly IUserService _userService;
        private readonly ITuratelSmsService _turatelSmsService;
        private readonly ILogger<InvitationController> _logger;

        // In-memory storage for SMS codes (in production, use Redis or database)
        private static readonly Dictionary<string, SmsVerificationData> _smsVerificationStore = new();

        public InvitationController(
            IUserInvitationService invitationService,
            IUserService userService,
            ITuratelSmsService turatelSmsService,
            ILogger<InvitationController> logger)
        {
            _invitationService = invitationService;
            _userService = userService;
            _turatelSmsService = turatelSmsService;
            _logger = logger;
        }

        [HttpGet("invitation/{token}")]
        public async Task<IActionResult> Register(string token)
        {
            try
            {
                // Validate the invitation token
                var validationResult = await _invitationService.ValidateInvitationTokenAsync(token);

                if (!validationResult.Success)
                {
                    TempData["ErrorMessage"] = validationResult.Message;
                    return View("InvitationExpired");
                }

                // Create the registration model with invitation data
                var model = new InvitationRegisterViewModel
                {
                    Token = token,
                    Email = validationResult.Data?.Email,
                    PhoneNumber = validationResult.Data?.PhoneNumber,
                    FieldReferenceCategoryId = validationResult.Data?.FieldReferenceCategoryId,
                    FieldReferenceSubCategoryId = validationResult.Data?.FieldReferenceSubCategoryId
                };

                return View("InvitationRegister", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invitation registration for token: {Token}", token);
                TempData["ErrorMessage"] = "Davet linki işlenirken hata oluştu";
                return View("InvitationExpired");
            }
        }

        [HttpPost("invitation/{token}")]
        public async Task<IActionResult> RegisterJson(string token, [FromBody] InvitationRegisterRequest request)
        {
            try
            {
                // Re-validate the token
                var validationResult = await _invitationService.ValidateInvitationTokenAsync(token);
                if (!validationResult.Success)
                {
                    return Json(new { success = false, message = validationResult.Message });
                }

                // Basic validation
                var validationErrors = ValidateRegistrationRequest(request);
                if (validationErrors.Any())
                {
                    return Json(new { success = false, message = string.Join("; ", validationErrors) });
                }

                // Create the user using UserService
                var createUserDto = new CreateUserDto
                {
                    UserName = request.UserName,
                    Password = request.Password,
                    Email = request.Email ?? validationResult.Data?.Email,
                    PhoneNumber = request.PhoneNumber ?? validationResult.Data?.PhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    FieldReferenceCategoryId = request.FieldReferenceCategoryId,
                    FieldReferenceSubCategoryId = request.FieldReferenceSubCategoryId,
                    RoleIds = new List<int> { 4 } // Default to "User" role (assuming ID 2)
                };

                var createResult = await _userService.CreateAsync(createUserDto);

                if (!createResult.Success)
                {
                    return Json(new { success = false, message = createResult.Message });
                }

                // Mark the invitation as used
                await _invitationService.MarkInvitationAsUsedAsync(token, createResult.Data.Id);

                return Json(new { success = true, message = "Hesabınız başarıyla oluşturuldu!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user from invitation: {Token}", token);
                return Json(new { success = false, message = "Hesap oluşturulurken hata oluştu" });
            }
        }

        private List<string> ValidateRegistrationRequest(InvitationRegisterRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.FirstName))
                errors.Add("Ad zorunludur");

            if (string.IsNullOrWhiteSpace(request.LastName))
                errors.Add("Soyad zorunludur");

            if (string.IsNullOrWhiteSpace(request.UserName))
                errors.Add("Kullanıcı adı zorunludur");

            if (string.IsNullOrWhiteSpace(request.Password))
                errors.Add("Şifre zorunludur");

            if (request.Password != request.ConfirmPassword)
                errors.Add("Şifreler eşleşmiyor");

            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(request.Email))
                    errors.Add("Geçerli bir e-posta adresi girin");
            }

            return errors;
        }

        [HttpPost("invitation/send-sms")]
        public async Task<IActionResult> SendSmsCode([FromBody] SendSmsCodeRequest request)
        {
            try
            {
                // Validate phone number
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return Json(new { success = false, message = "Telefon numarası gereklidir" });
                }

                // Generate 6-digit verification code
                var verificationCode = GenerateVerificationCode();

                // Send SMS via Turatel
                var smsResult = await _turatelSmsService.SendVerificationCodeAsync(request.PhoneNumber, verificationCode);

                if (!smsResult.Success || smsResult.Data == null || !smsResult.Data.IsSuccess)
                {
                    _logger.LogWarning("SMS sending failed for phone {PhoneNumber}. Error: {Error}",
                        request.PhoneNumber, smsResult.Message);
                    return Json(new { success = false, message = smsResult.Message ?? "SMS gönderilemedi" });
                }

                // Store verification code with expiry (5 minutes)
                var key = $"{request.PhoneNumber}:{request.Token}";
                _smsVerificationStore[key] = new SmsVerificationData
                {
                    Code = verificationCode,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    PhoneNumber = request.PhoneNumber
                };

                _logger.LogInformation("SMS verification code sent to {PhoneNumber}. PacketId: {PacketId}",
                    request.PhoneNumber, smsResult.Data.PacketId);

                return Json(new
                {
                    success = true,
                    message = "SMS doğrulama kodu gönderildi",
                    packetId = smsResult.Data.PacketId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS code to {PhoneNumber}", request.PhoneNumber);
                return Json(new { success = false, message = "SMS gönderilirken bir hata oluştu" });
            }
        }

        [HttpPost("invitation/verify-sms")]
        public IActionResult VerifySmsCode([FromBody] VerifySmsCodeRequest request)
        {
            try
            {
                var key = $"{request.PhoneNumber}:{request.Token}";

                // Check if verification code exists
                if (!_smsVerificationStore.TryGetValue(key, out var storedData))
                {
                    return Json(new { success = false, message = "Doğrulama kodu bulunamadı. Lütfen yeni kod isteyin." });
                }

                // Check if code is expired
                if (DateTime.UtcNow > storedData.ExpiryTime)
                {
                    _smsVerificationStore.Remove(key);
                    return Json(new { success = false, message = "Doğrulama kodu süresi dolmuş. Lütfen yeni kod isteyin." });
                }

                // Verify code
                if (storedData.Code != request.Code)
                {
                    return Json(new { success = false, message = "Doğrulama kodu hatalı" });
                }

                // Code is correct, remove it from store
                _smsVerificationStore.Remove(key);

                _logger.LogInformation("SMS verification successful for phone {PhoneNumber}", request.PhoneNumber);

                return Json(new { success = true, message = "SMS doğrulama başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying SMS code for {PhoneNumber}", request.PhoneNumber);
                return Json(new { success = false, message = "Doğrulama sırasında bir hata oluştu" });
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }

    public class InvitationRegisterRequest
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? FieldReferenceCategoryId { get; set; }
        public int? FieldReferenceSubCategoryId { get; set; }
    }

    public class SendSmsCodeRequest
    {
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
    }

    public class VerifySmsCodeRequest
    {
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public string Code { get; set; }
    }

    public class SmsVerificationData
    {
        public string Code { get; set; }
        public DateTime ExpiryTime { get; set; }
        public string PhoneNumber { get; set; }
    }
}