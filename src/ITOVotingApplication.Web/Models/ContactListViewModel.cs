using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Contact;

namespace ITOVotingApplication.Web.Models
{
	public class ContactListViewModel
	{
		public PagedResult<ContactDto> Contacts { get; set; }
		public string SearchTerm { get; set; }
	}
}
