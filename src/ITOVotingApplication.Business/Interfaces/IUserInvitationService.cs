using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.Entities;

namespace ITOVotingApplication.Business.Interfaces
{
    public interface IUserInvitationService
    {
        Task<ApiResponse<string>> CreateInvitationAsync(string? email, string? phoneNumber, int createdByUserId);
        Task<ApiResponse<UserInvitation>> ValidateInvitationTokenAsync(string token);
        Task<ApiResponse<bool>> MarkInvitationAsUsedAsync(string token, int usedByUserId);
        Task<ApiResponse<bool>> CleanupExpiredInvitationsAsync();
    }
}