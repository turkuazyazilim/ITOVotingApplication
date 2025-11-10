using ITOVotingApplication.Core.DTOs.Common;

namespace ITOVotingApplication.Business.Interfaces
{
    public interface ITuratelSmsService
    {
        Task<ApiResponse<TuratelSmsResponse>> SendVerificationCodeAsync(string phoneNumber, string verificationCode);
    }

    // Response models for Turatel SMS API
    public class TuratelSmsResponse
    {
        public string ErrorCode { get; set; }
        public string PacketId { get; set; }
        public string MessageId { get; set; }
        public bool IsSuccess => ErrorCode == "0";
    }
}
