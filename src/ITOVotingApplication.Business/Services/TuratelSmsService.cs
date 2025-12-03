using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ITOVotingApplication.Business.Services
{
    public class TuratelSmsService : ITuratelSmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TuratelSmsService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _originator;
        private readonly string _username;
        private readonly string _password;
        private readonly int _userCode;
        private readonly int _accountId;

        public TuratelSmsService(
            IConfiguration configuration,
            ILogger<TuratelSmsService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();

            // Load Turatel SMS settings from configuration
            _apiUrl = _configuration["TuratelSmsSettings:ApiUrl"] ?? "https://api.turatel.com/send-sms";
            _originator = _configuration["TuratelSmsSettings:Originator"] ?? "TESTOTPTRTL";
            _username = _configuration["TuratelSmsSettings:Username"] ?? "";
            _password = _configuration["TuratelSmsSettings:Password"] ?? "";
            _userCode = int.Parse(_configuration["TuratelSmsSettings:UserCode"] ?? "0");
            _accountId = int.Parse(_configuration["TuratelSmsSettings:AccountId"] ?? "0");
        }

        public async Task<ApiResponse<TuratelSmsResponse>> SendVerificationCodeAsync(string phoneNumber, string verificationCode)
        {
            try
            {
                // Validate configuration
                if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
                {
                    _logger.LogWarning("Turatel SMS settings are not configured properly");
                    return ApiResponse<TuratelSmsResponse>.ErrorResult("SMS ayarları yapılandırılmamış.");
                }

                // Clean phone number (ensure it starts with 90)
                var cleanedPhone = CleanPhoneNumber(phoneNumber);
                if (string.IsNullOrEmpty(cleanedPhone))
                {
                    return ApiResponse<TuratelSmsResponse>.ErrorResult("Geçersiz telefon numarası formatı!");
                }

                // Prepare message text
                var messageText = $"Kayıt onay şifreniz : {verificationCode} ITOP Oy Sistemi";

                // Prepare request payload
                var requestPayload = new
                {
                    originator = _originator,
                    sendDate = "",
                    validityPeriod = 0,
                    messageText = messageText,
                    receiverList = new[] { cleanedPhone },
                    personalMessages = new string[] { },
                    isCheckBlackList = false,
                    isEncryptedParameter = false,
                    username = _username,
                    password = _password,
                    userCode = _userCode,
                    accountId = _accountId
                };

                var jsonContent = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending SMS verification code to {PhoneNumber}", cleanedPhone);

                // Send request to Turatel SMS API
                var response = await _httpClient.PostAsync(_apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Turatel SMS API Response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Turatel SMS API returned error status: {StatusCode}", response.StatusCode);
                    return ApiResponse<TuratelSmsResponse>.ErrorResult($"SMS servisi hatası: HTTP {response.StatusCode}");
                }

                // Parse response
                var smsResponse = ParseTuratelResponse(responseContent);

                if (smsResponse.IsSuccess)
                {
                    _logger.LogInformation("SMS sent successfully. PacketId: {PacketId}, MessageId: {MessageId}",
                        smsResponse.PacketId, smsResponse.MessageId);
                    return ApiResponse<TuratelSmsResponse>.SuccessResult(smsResponse, "SMS başarıyla gönderildi!");
                }
                else
                {
                    _logger.LogWarning("SMS sending failed. ErrorCode: {ErrorCode}", smsResponse.ErrorCode);
                    return ApiResponse<TuratelSmsResponse>.ErrorResult($"SMS gönderilemedi. Hata kodu: {smsResponse.ErrorCode}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error while sending SMS to {PhoneNumber}", phoneNumber);
                return ApiResponse<TuratelSmsResponse>.ErrorResult($"SMS gönderilirken bağlantı hatası oluştu: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                return ApiResponse<TuratelSmsResponse>.ErrorResult($"SMS gönderilirken hata oluştu: {ex.Message}");
            }
        }
		public async Task<ApiResponse<TuratelSmsResponse>> SendSmsAsync(string phoneNumber, string message)
		{
			try
			{
				// Validate configuration
				if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
				{
					_logger.LogWarning("Turatel SMS settings are not configured properly");
					return ApiResponse<TuratelSmsResponse>.ErrorResult("SMS ayarları yapılandırılmamış.");
				}

				// Clean phone number (ensure it starts with 90)
				var cleanedPhone = CleanPhoneNumber(phoneNumber);
				if (string.IsNullOrEmpty(cleanedPhone))
				{
					return ApiResponse<TuratelSmsResponse>.ErrorResult("Geçersiz telefon numarası formatı!");
				}

				// Prepare message text
				var messageText = message;

				// Prepare request payload
				var requestPayload = new
				{
					originator = _originator,
					sendDate = "",
					validityPeriod = 0,
					messageText = messageText,
					receiverList = new[] { cleanedPhone },
					personalMessages = new string[] { },
					isCheckBlackList = false,
					isEncryptedParameter = false,
					username = _username,
					password = _password,
					userCode = _userCode,
					accountId = _accountId
				};

				var jsonContent = JsonSerializer.Serialize(requestPayload);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				_logger.LogInformation("Sending SMS verification code to {PhoneNumber}", cleanedPhone);

				// Send request to Turatel SMS API
				var response = await _httpClient.PostAsync(_apiUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				_logger.LogInformation("Turatel SMS API Response: {Response}", responseContent);

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Turatel SMS API returned error status: {StatusCode}", response.StatusCode);
					return ApiResponse<TuratelSmsResponse>.ErrorResult($"SMS servisi hatası: HTTP {response.StatusCode}");
				}

				// Parse response
				var smsResponse = ParseTuratelResponse(responseContent);

				if (smsResponse.IsSuccess)
				{
					_logger.LogInformation("SMS sent successfully. PacketId: {PacketId}, MessageId: {MessageId}",
						smsResponse.PacketId, smsResponse.MessageId);
					return ApiResponse<TuratelSmsResponse>.SuccessResult(smsResponse, "SMS başarıyla gönderildi!");
				}
				else
				{
					_logger.LogWarning("SMS sending failed. ErrorCode: {ErrorCode}", smsResponse.ErrorCode);
					return ApiResponse<TuratelSmsResponse>.ErrorResult($"SMS gönderilemedi. Hata kodu: {smsResponse.ErrorCode}");
				}
			}
			catch (HttpRequestException httpEx)
			{
				_logger.LogError(httpEx, "HTTP error while sending SMS to {PhoneNumber}", phoneNumber);
				return ApiResponse<TuratelSmsResponse>.ErrorResult($"SMS gönderilirken bağlantı hatası oluştu: {httpEx.Message}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
				return ApiResponse<TuratelSmsResponse>.ErrorResult($"SMS gönderilirken hata oluştu: {ex.Message}");
			}
		}
		private TuratelSmsResponse ParseTuratelResponse(string responseContent)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("sendSmsResult", out var sendSmsResult))
                    {
                        var errorCode = sendSmsResult.TryGetProperty("ErrorCode", out var ec) ? ec.GetString() : "-1";
                        var packetId = sendSmsResult.TryGetProperty("PacketId", out var pi) ? pi.GetString() : "";
                        var messageId = "";

                        if (sendSmsResult.TryGetProperty("MessageIdList", out var messageIdList))
                        {
                            if (messageIdList.TryGetProperty("MessageId", out var mi))
                            {
                                messageId = mi.GetString() ?? "";
                            }
                        }

                        return new TuratelSmsResponse
                        {
                            ErrorCode = errorCode ?? "-1",
                            PacketId = packetId ?? "",
                            MessageId = messageId
                        };
                    }
                }

                return new TuratelSmsResponse { ErrorCode = "-1" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Turatel SMS response");
                return new TuratelSmsResponse { ErrorCode = "-1" };
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
