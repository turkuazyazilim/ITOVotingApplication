using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Vote;

namespace ITOVotingApplication.Business.Interfaces
{
	public interface IVoteService
	{
		Task<ApiResponse<VoteDto>> CastVoteAsync(CastVoteDto voteDto, int userId);
		Task<ApiResponse<bool>> CheckIfVotedAsync(int contactId, int ballotBoxId);
		Task<ApiResponse<List<VoteDto>>> GetVotesByBallotBoxAsync(int ballotBoxId);
		Task<ApiResponse<List<VoteDto>>> GetVotesByCompanyAsync(int companyId);
		Task<ApiResponse<VoteResultDto>> GetVoteResultsAsync(int ballotBoxId);
		Task<ApiResponse<List<VoteResultDto>>> GetAllVoteResultsAsync();
		Task<ApiResponse<int>> GetVoteCountAsync(int? ballotBoxId = null);
	}
}