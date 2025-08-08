namespace ITOVotingApplication.Web.Models
{
	/// <summary>
	/// Other view models used in the application
	/// </summary>
	public class AdminDashboardViewModel
	{
		public string UserName { get; set; }
		public string FullName { get; set; }
		public int TotalCompanies { get; set; }
		public int TotalContacts { get; set; }
		public int TotalUsers { get; set; }
		public int TotalVotes { get; set; }
	}
}
