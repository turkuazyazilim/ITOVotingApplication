using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Company;

namespace ITOVotingApplication.Web.Models
{
	public class CompanyListViewModel
	{
		public PagedResult<CompanyDto> Companies { get; set; }
		public string SearchTerm { get; set; }
	}
}
