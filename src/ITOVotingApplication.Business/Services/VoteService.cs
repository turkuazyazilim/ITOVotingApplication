using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Vote;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Business.Services
{
	public class VoteService : IVoteService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public VoteService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public async Task<ApiResponse<int>> GetVoteCountAsync(int? ballotBoxId = null)
		{
			try
			{
				var query = _unitOfWork.VoteTransactions.Query();

				if (ballotBoxId.HasValue)
				{
					query = query.Where(v => v.BallotBoxId == ballotBoxId.Value);
				}

				var count = await query.CountAsync();

				return ApiResponse<int>.SuccessResult(count);
			}
			catch (Exception ex)
			{
				return ApiResponse<int>.ErrorResult($"Oy sayısı getirme hatası: {ex.Message}");
			}
		}
		public async Task<ApiResponse<VoteDto>> CastVoteAsync(CastVoteDto voteDto, int userId)
		{
			try
			{
				// Kişinin oy kullanmaya uygun olup olmadığını kontrol et
				var contact = await _unitOfWork.Contacts.GetByIdAsync(voteDto.ContactId);
				if (contact == null)
				{
					return ApiResponse<VoteDto>.ErrorResult("Yetkili kişi bulunamadı.");
				}

				if (!contact.EligibleToVote)
				{
					return ApiResponse<VoteDto>.ErrorResult("Bu kişi oy kullanmaya yetkili değil.");
				}

				// Daha önce oy kullanıp kullanmadığını kontrol et
				var existingVote = await _unitOfWork.VoteTransactions
					.SingleOrDefaultAsync(v => v.ContactId == voteDto.ContactId &&
											  v.BallotBoxId == voteDto.BallotBoxId);

				if (existingVote != null)
				{
					return ApiResponse<VoteDto>.ErrorResult("Bu kişi bu sandıkta zaten oy kullanmış.");
				}

				// Sandık kontrolü
				var ballotBox = await _unitOfWork.BallotBoxes.GetByIdAsync(voteDto.BallotBoxId);
				if (ballotBox == null)
				{
					return ApiResponse<VoteDto>.ErrorResult("Sandık bulunamadı.");
				}

				// Yeni oy kaydı oluştur
				var voteTransaction = new VoteTransaction
				{
					CompanyId = contact.CompanyId,
					ContactId = voteDto.ContactId,
					BallotBoxId = voteDto.BallotBoxId,
					Description = voteDto.Description,
					CreatedUserId = userId,
					CreatedDate = DateTime.Now
				};

				await _unitOfWork.VoteTransactions.AddAsync(voteTransaction);
				await _unitOfWork.CompleteAsync();

				// Kaydedilen oyu detaylı olarak getir
				var savedVote = await _unitOfWork.VoteTransactions.Query()
					.Include(v => v.Company)
					.Include(v => v.Contact)
					.Include(v => v.BallotBox)
					.Include(v => v.CreatedUser)
					.FirstOrDefaultAsync(v => v.Id == voteTransaction.Id);

				var result = _mapper.Map<VoteDto>(savedVote);
				return ApiResponse<VoteDto>.SuccessResult(result, "Oy başarıyla kaydedildi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<VoteDto>.ErrorResult($"Oy kullanırken hata oluştu: {ex.Message}");
			}
		}

		public async Task<ApiResponse<bool>> CheckIfVotedAsync(int contactId, int ballotBoxId)
		{
			var hasVoted = await _unitOfWork.VoteTransactions
				.SingleOrDefaultAsync(v => v.ContactId == contactId && v.BallotBoxId == ballotBoxId) != null;

			return ApiResponse<bool>.SuccessResult(hasVoted);
		}

		public async Task<ApiResponse<List<VoteDto>>> GetVotesByBallotBoxAsync(int ballotBoxId)
		{
			var votes = await _unitOfWork.VoteTransactions.Query()
				.Include(v => v.Company)
				.Include(v => v.Contact)
				.Include(v => v.BallotBox)
				.Include(v => v.CreatedUser)
				.Where(v => v.BallotBoxId == ballotBoxId)
				.ToListAsync();

			var result = _mapper.Map<List<VoteDto>>(votes);
			return ApiResponse<List<VoteDto>>.SuccessResult(result);
		}

		public async Task<ApiResponse<List<VoteDto>>> GetVotesByCompanyAsync(int companyId)
		{
			var votes = await _unitOfWork.VoteTransactions.Query()
				.Include(v => v.Company)
				.Include(v => v.Contact)
				.Include(v => v.BallotBox)
				.Include(v => v.CreatedUser)
				.Where(v => v.CompanyId == companyId)
				.ToListAsync();

			var result = _mapper.Map<List<VoteDto>>(votes);
			return ApiResponse<List<VoteDto>>.SuccessResult(result);
		}

		public async Task<ApiResponse<VoteResultDto>> GetVoteResultsAsync(int ballotBoxId)
		{
			var ballotBox = await _unitOfWork.BallotBoxes.GetByIdAsync(ballotBoxId);
			if (ballotBox == null)
			{
				return ApiResponse<VoteResultDto>.ErrorResult("Sandık bulunamadı.");
			}

			var totalVotes = await _unitOfWork.VoteTransactions.Query()
				.CountAsync(v => v.BallotBoxId == ballotBoxId);

			var eligibleVoters = await _unitOfWork.Contacts.Query()
				.CountAsync(c => c.EligibleToVote);

			var lastVote = await _unitOfWork.VoteTransactions.Query()
				.Where(v => v.BallotBoxId == ballotBoxId)
				.OrderByDescending(v => v.CreatedDate)
				.FirstOrDefaultAsync();

			var result = new VoteResultDto
			{
				BallotBoxId = ballotBoxId,
				BallotBoxDescription = ballotBox.BallotBoxDescription,
				TotalVotes = totalVotes,
				EligibleVoters = eligibleVoters,
				TurnoutPercentage = eligibleVoters > 0 ? (decimal)totalVotes / eligibleVoters * 100 : 0,
				LastVoteDate = lastVote?.CreatedDate ?? DateTime.MinValue
			};

			return ApiResponse<VoteResultDto>.SuccessResult(result);
		}

		public async Task<ApiResponse<List<VoteResultDto>>> GetAllVoteResultsAsync()
		{
			var ballotBoxes = await _unitOfWork.BallotBoxes.GetAllAsync();
			var results = new List<VoteResultDto>();

			foreach (var ballotBox in ballotBoxes)
			{
				var resultResponse = await GetVoteResultsAsync(ballotBox.Id);
				if (resultResponse.Success)
				{
					results.Add(resultResponse.Data);
				}
			}

			return ApiResponse<List<VoteResultDto>>.SuccessResult(results);
		}
	}
}