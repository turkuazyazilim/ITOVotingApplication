using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ITOVotingApplication.Business.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(ILogger<WhatsAppService> logger)
        {
            _logger = logger;
        }

        public async Task<ApiResponse<string>> SendRegistrationLinkAsync(string phoneNumber, string registrationLink)
        {
            try
            {
                var message = $"🎯 *İTOP Oylama Sistemi*\n\n" +
                            $"Merhaba! Saha kullanıcısı olarak sisteme kayıt olmanız için size özel bir link gönderiyoruz.\n\n" +
                            $"👤 *Kayıt için:*\n" +
                            $"• Aşağıdaki linke tıklayın\n" +
                            $"• Bilgilerinizi doldurun\n" +
                            $"• Saha referans bilgilerinizi seçin\n\n" +
                            $"🔗 *Kayıt Linki:*\n{registrationLink}\n\n" +
                            $"❓ Herhangi bir sorunuz varsa sistem yöneticisi ile iletişime geçebilirsiniz.\n\n" +
                            $"*İTOP*";

                return await SendTextMessageAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration message for {PhoneNumber}", phoneNumber);
                return ApiResponse<string>.ErrorResult($"Mesaj oluşturulurken hata oluştu: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> SendTextMessageAsync(string phoneNumber, string message)
        {
            try
            {
                // Clean phone number (remove spaces, +, etc.)
                var cleanPhone = CleanPhoneNumber(phoneNumber);

                if (string.IsNullOrEmpty(cleanPhone))
                {
                    return ApiResponse<string>.ErrorResult("Geçersiz telefon numarası formatı!");
                }

                // Return the message for manual sharing
                _logger.LogInformation("WhatsApp message prepared for manual sharing to {PhoneNumber}", cleanPhone);

                return await Task.FromResult(ApiResponse<string>.SuccessResult(
                    message,
                    "Mesaj hazırlandı - manuel olarak gönderilebilir!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing WhatsApp message for {PhoneNumber}", phoneNumber);
                return ApiResponse<string>.ErrorResult($"Mesaj hazırlanırken hata oluştu: {ex.Message}");
            }
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // Remove all non-numeric characters
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Handle Turkish phone numbers
            if (cleaned.StartsWith("90") && cleaned.Length == 12)
            {
                // Already has country code
                return cleaned;
            }
            else if (cleaned.StartsWith("0") && cleaned.Length == 11)
            {
                // Remove leading 0 and add Turkey country code
                return "90" + cleaned.Substring(1);
            }
            else if (cleaned.Length == 10)
            {
                // Add Turkey country code
                return "90" + cleaned;
            }

            // Return as is if format is unclear
            return cleaned;
        }
    }
}