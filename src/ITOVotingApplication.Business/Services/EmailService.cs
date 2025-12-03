using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ITOVotingApplication.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load email settings from configuration
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            _senderName = _configuration["EmailSettings:SenderName"] ?? "İTOP Oylama Sistemi";
            _username = _configuration["EmailSettings:Username"] ?? "";
            _password = _configuration["EmailSettings:Password"] ?? "";
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
        }

        public async Task<ApiResponse<bool>> SendRegistrationLinkAsync(string email, string registrationLink)
        {
            try
            {
                // Validate email configuration
                if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_password))
                {
                    _logger.LogWarning("Email settings are not configured properly");
                    return ApiResponse<bool>.ErrorResult("E-posta ayarları yapılandırılmamış. Lütfen sistem yöneticisi ile iletişime geçin.");
                }

                var subject = "İTOP - Saha Kullanıcısı Kayıt Daveti";

                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f9f9f9;
        }}
        .header {{
            background-color: #7c3aed;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background-color: white;
            padding: 30px;
            border-radius: 0 0 5px 5px;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #7c3aed;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            color: #666;
            font-size: 12px;
        }}
        .steps {{
            background-color: #f0fdf4;
            padding: 15px;
            border-left: 4px solid #22c55e;
            margin: 20px 0;
        }}
        .warning {{
            background-color: #fef3c7;
            padding: 15px;
            border-left: 4px solid #f59e0b;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎯 İTOP Oylama Sistemi</h1>
            <p>Saha Kullanıcısı Kayıt Daveti</p>
        </div>
        <div class='content'>
            <p>Merhaba,</p>
            <p>Saha kullanıcısı olarak <strong>İTOP Oylama Sistemi</strong>'ne kayıt olmanız için size özel bir davet linki gönderiyoruz.</p>

            <div class='steps'>
                <h3>📋 Kayıt Adımları:</h3>
                <ol>
                    <li>Aşağıdaki butona tıklayarak kayıt sayfasına gidin</li>
                    <li>Kişisel bilgilerinizi eksiksiz doldurun</li>
                    <li>Saha referans bilgileriniz otomatik olarak atanacaktır</li>
                    <li>Kullanıcı adı ve şifrenizi belirleyin</li>
                </ol>
            </div>

            <center>
                <a href='{registrationLink}' class='button' style='color: white;'>
                    Kayıt Ol
                </a>
            </center>

            <p style='color: #666; font-size: 14px;'>
                Veya aşağıdaki linki tarayıcınıza kopyalayabilirsiniz:<br>
                <a href='{registrationLink}'>{registrationLink}</a>
            </p>

            <div class='warning'>
                <p><strong>⚠️ Önemli Bilgiler:</strong></p>
                <ul>
                    <li>Bu davet linki sadece sizin için oluşturulmuştur</li>
                    <li>Link sadece bir kez kullanılabilir</li>
                    <li>Saha referans bilgileriniz önceden atanmıştır</li>
                    <li>Herhangi bir sorun yaşarsanız sistem yöneticisi ile iletişime geçin</li>
                </ul>
            </div>

            <p>Saygılarımızla,<br>
            <strong>İTOP</strong><br>
            Bilgi İşlem Departmanı</p>
        </div>
        <div class='footer'>
            <p>Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayınız.</p>
            <p>&copy; 2026 İTOP - Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration email for {Email}", email);
                return ApiResponse<bool>.ErrorResult($"Kayıt e-postası oluşturulurken hata oluştu: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Validate email configuration
                if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_password))
                {
                    _logger.LogWarning("Email settings are not configured properly");
                    return ApiResponse<bool>.ErrorResult("E-posta ayarları yapılandırılmamış.");
                }

                // Validate recipient email
                if (string.IsNullOrEmpty(toEmail) || !IsValidEmail(toEmail))
                {
                    return ApiResponse<bool>.ErrorResult("Geçersiz e-posta adresi!");
                }

                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtpClient.EnableSsl = _enableSsl;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(_username, _password);

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(_senderEmail, _senderName);
                        mailMessage.To.Add(toEmail);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Priority = MailPriority.Normal;

                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return ApiResponse<bool>.SuccessResult(true, "E-posta başarıyla gönderildi!");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending email to {Email}", toEmail);
                return ApiResponse<bool>.ErrorResult($"E-posta gönderilirken SMTP hatası oluştu: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                return ApiResponse<bool>.ErrorResult($"E-posta gönderilirken hata oluştu: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SendDocumentEmailAsync(string toEmail, string contactName, string companyName, string documentUrl, string expiresIn)
        {
            try
            {
                // Validate email configuration
                if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_password))
                {
                    _logger.LogWarning("Email settings are not configured properly");
                    return ApiResponse<bool>.ErrorResult("E-posta ayarları yapılandırılmamış.");
                }

                var subject = "İTOP - Yetki Belgesi Talep Dilekçesi";

                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f9f9f9;
        }}
        .header {{
            background-color: #2563eb;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background-color: white;
            padding: 30px;
            border-radius: 0 0 5px 5px;
        }}
        .button {{
            display: inline-block;
            padding: 15px 30px;
            background-color: #2563eb;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
            font-weight: bold;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            color: #666;
            font-size: 12px;
        }}
        .info-box {{
            background-color: #dbeafe;
            padding: 15px;
            border-left: 4px solid #2563eb;
            margin: 20px 0;
        }}
        .warning {{
            background-color: #fef3c7;
            padding: 15px;
            border-left: 4px solid #f59e0b;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📄 İTOP</h1>
            <p>Yetki Belgesi Talep Dilekçesi</p>
        </div>
        <div class='content'>
            <p>Sayın <strong>{contactName}</strong>,</p>

            <p><strong>{companyName}</strong> firması için hazırlanan yetki belgesi talep dilekçesi e-posta ile gönderilmiştir.</p>

            <div class='info-box'>
                <h3>📋 Belge Bilgileri:</h3>
                <ul>
                    <li><strong>Belge Türü:</strong> Yetki Belgesi Talep Dilekçesi</li>
                    <li><strong>Firma:</strong> {companyName}</li>
                    <li><strong>Geçerlilik:</strong> {expiresIn}</li>
                </ul>
            </div>

            <center>
                <a href='{documentUrl}' class='button' style='color: white;'>
                    📥 Belgeyi İndir
                </a>
            </center>

            <p style='color: #666; font-size: 14px; text-align: center; margin-top: 10px;'>
                Veya aşağıdaki linki tarayıcınıza kopyalayabilirsiniz:<br>
                <a href='{documentUrl}'>{documentUrl}</a>
            </p>

            <div class='warning'>
                <p><strong>⚠️ Önemli Bilgiler:</strong></p>
                <ul>
                    <li>Bu belge, firmanızın resmi işlemlerinde kullanılmak üzere hazırlanmıştır</li>
                    <li>Belgeyi indirdikten sonra imzalayıp sisteme yüklemeniz gerekmektedir</li>
                    <li>Link {expiresIn} süreyle geçerlidir</li>
                    <li>Herhangi bir sorunuz olması durumunda bizimle iletişime geçebilirsiniz</li>
                </ul>
            </div>

            <div class='info-box'>
                <h4>📝 Sonraki Adımlar:</h4>
                <ol>
                    <li>Belgeyi indirin</li>
                    <li>Gerekli bilgileri kontrol edin</li>
                    <li>Belgeyi imzalayın</li>
                    <li>İmzalı belgeyi sisteme yükleyin</li>
                </ol>
            </div>

            <p>Saygılarımızla,<br>
            <strong>İTOP</strong><br>
            Bilgi İşlem Departmanı</p>
        </div>
        <div class='footer'>
            <p>Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayınız.</p>
            <p>&copy; 2026 İTOP - Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document email for {Email}", toEmail);
                return ApiResponse<bool>.ErrorResult($"Belge e-postası oluşturulurken hata oluştu: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
