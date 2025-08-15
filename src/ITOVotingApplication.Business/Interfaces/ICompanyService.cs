using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Company;

namespace ITOVotingApplication.Business.Interfaces
{
	public interface ICompanyService
	{
		Task<ApiResponse<CompanyDto>> GetByIdAsync(int id);
		Task<ApiResponse<PagedResult<CompanyDto>>> GetAllAsync(PagedRequest request);
		Task<ApiResponse<CompanyDto>> CreateAsync(CreateCompanyDto dto);
		Task<ApiResponse<CompanyDto>> UpdateAsync(UpdateCompanyDto dto);
		Task<ApiResponse<bool>> DeleteAsync(int id);
		Task<ApiResponse<List<CompanyDto>>> GetActiveCompaniesAsync();
		Task<ApiResponse<CompanyDto>> GetByRegistrationNumberAsync(string registrationNumber);
		Task<ApiResponse<int>> GetCountAsync(bool onlyActive = true);
	}
}