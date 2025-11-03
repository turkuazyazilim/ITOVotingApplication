using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.CompanyDocument;
using ITOVotingApplication.Core.Enums;

namespace ITOVotingApplication.Business.Interfaces
{
    public interface ICompanyDocumentTransactionService
    {
        Task<ApiResponse<CompanyDocumentTransactionDto>> CreateAsync(CreateDocumentTransactionDto dto, int userId);
        Task<ApiResponse<CompanyDocumentTransactionDto>> UpdateDeliveryStatusAsync(UpdateDeliveryStatusDto dto);
        Task<ApiResponse<CompanyDocumentTransactionDto>> AssignToContactAsync(int transactionId, int contactId);
        Task<ApiResponse<bool>> DeleteAsync(int transactionId);
        Task<ApiResponse<CompanyDocumentTransactionDto>> GetByIdAsync(int id);
        Task<ApiResponse<List<CompanyDocumentTransactionDto>>> GetByCompanyIdAsync(int companyId);
        Task<ApiResponse<CompanyDocumentTransactionDto>> GetLatestByCompanyAndTypeAsync(int companyId, CompanyDocumentType documentType);
    }
}
