using ITOVotingApplication.Core.DTOs.Common;

namespace ITOVotingApplication.Business.Interfaces
{
    public interface IWhatsAppService
    {
        Task<ApiResponse<string>> SendRegistrationLinkAsync(string phoneNumber, string registrationLink);
        Task<ApiResponse<string>> SendTextMessageAsync(string phoneNumber, string message);
    }
}