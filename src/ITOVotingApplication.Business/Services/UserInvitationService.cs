using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ITOVotingApplication.Business.Services
{
    public class UserInvitationService : IUserInvitationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserInvitationService> _logger;

        public UserInvitationService(IUnitOfWork unitOfWork, ILogger<UserInvitationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> CreateInvitationAsync(string? email, string? phoneNumber, int createdByUserId)
        {
            try
            {
                // Generate unique token
                var token = GenerateSecureToken();
                var createdDate = DateTime.UtcNow;
                var expiryDate = createdDate.AddDays(1); // 1 day expiration

                var invitation = new UserInvitation
                {
                    Token = token,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    CreatedDate = createdDate,
                    ExpiryDate = expiryDate,
                    IsUsed = false,
                    CreatedByUserId = createdByUserId
                };

                await _unitOfWork.UserInvitations.AddAsync(invitation);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Created invitation token {Token} for email: {Email}, phone: {Phone}",
                    token, email, phoneNumber);

                return ApiResponse<string>.SuccessResult(token, "Davet linki başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invitation for email: {Email}, phone: {Phone}", email, phoneNumber);
                return ApiResponse<string>.ErrorResult("Davet linki oluşturulurken hata oluştu");
            }
        }

        public async Task<ApiResponse<UserInvitation>> ValidateInvitationTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return ApiResponse<UserInvitation>.ErrorResult("Geçersiz davet linki");
                }

                var invitation = await _unitOfWork.UserInvitations.Query()
                    .Include(i => i.CreatedByUser)
                    .FirstOrDefaultAsync(i => i.Token == token);

                if (invitation == null)
                {
                    return ApiResponse<UserInvitation>.ErrorResult("Davet linki bulunamadı");
                }

                if (invitation.IsUsed)
                {
                    return ApiResponse<UserInvitation>.ErrorResult("Bu davet linki daha önce kullanılmış");
                }

                if (invitation.ExpiryDate < DateTime.UtcNow)
                {
                    return ApiResponse<UserInvitation>.ErrorResult("Davet linkinin süresi dolmuş");
                }

                return ApiResponse<UserInvitation>.SuccessResult(invitation, "Davet linki geçerli");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invitation token: {Token}", token);
                return ApiResponse<UserInvitation>.ErrorResult("Davet linki doğrulanırken hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> MarkInvitationAsUsedAsync(string token, int usedByUserId)
        {
            try
            {
                var invitation = await _unitOfWork.UserInvitations.Query()
                    .FirstOrDefaultAsync(i => i.Token == token && !i.IsUsed);

                if (invitation == null)
                {
                    return ApiResponse<bool>.ErrorResult("Davet linki bulunamadı veya zaten kullanılmış");
                }

                invitation.IsUsed = true;
                invitation.UsedDate = DateTime.UtcNow;
                invitation.UsedByUserId = usedByUserId;

                _unitOfWork.UserInvitations.Update(invitation);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Marked invitation token {Token} as used by user {UserId}", token, usedByUserId);

                return ApiResponse<bool>.SuccessResult(true, "Davet linki kullanıldı olarak işaretlendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking invitation as used: {Token}", token);
                return ApiResponse<bool>.ErrorResult("Davet linki güncellenirken hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> CleanupExpiredInvitationsAsync()
        {
            try
            {
                var expiredInvitations = await _unitOfWork.UserInvitations.Query()
                    .Where(i => i.ExpiryDate < DateTime.UtcNow && !i.IsUsed)
                    .ToListAsync();

                if (expiredInvitations.Any())
                {
                    foreach (var invitation in expiredInvitations)
                    {
                        _unitOfWork.UserInvitations.Remove(invitation);
                    }

                    await _unitOfWork.CompleteAsync();

                    _logger.LogInformation("Cleaned up {Count} expired invitations", expiredInvitations.Count);
                }

                return ApiResponse<bool>.SuccessResult(true, $"{expiredInvitations.Count} süresi dolmuş davet temizlendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired invitations");
                return ApiResponse<bool>.ErrorResult("Süresi dolmuş davetler temizlenirken hata oluştu");
            }
        }

        private string GenerateSecureToken()
        {
            // Generate a cryptographically secure random token
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenBytes = new byte[32]; // 256 bits
                rng.GetBytes(tokenBytes);

                // Convert to base64 and make URL-safe
                var token = Convert.ToBase64String(tokenBytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");

                return token;
            }
        }
    }
}