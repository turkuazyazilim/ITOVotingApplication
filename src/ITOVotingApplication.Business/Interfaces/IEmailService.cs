using ITOVotingApplication.Core.DTOs.Common;

namespace ITOVotingApplication.Business.Interfaces
{
    public interface IEmailService
    {
        Task<ApiResponse<bool>> SendRegistrationLinkAsync(string email, string registrationLink);
        Task<ApiResponse<bool>> SendEmailAsync(string toEmail, string subject, string body);
    }
}
