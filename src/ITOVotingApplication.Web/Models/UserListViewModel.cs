using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.User;

namespace ITOVotingApplication.Web.Models
{
	public class UserListViewModel
	{
		public PagedResult<UserDto> Users { get; set; }
		public string SearchTerm { get; set; }
	}
}
