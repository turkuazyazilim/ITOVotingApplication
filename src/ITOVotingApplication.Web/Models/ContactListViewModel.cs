using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Contact;

namespace ITOVotingApplication.Web.Models
{
	public class ContactListViewModel
	{
		public PagedResult<ContactDto> Contacts { get; set; }
		public string SearchTerm { get; set; }

		public ContactListViewModel()
		{
			Contacts = new PagedResult<ContactDto>
			{
				Items = new List<ContactDto>(),
				TotalCount = 0,
				PageNumber = 1,
				PageSize = 10
			};
		}
	}
}