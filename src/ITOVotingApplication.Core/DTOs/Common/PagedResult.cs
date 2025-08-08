namespace ITOVotingApplication.Core.DTOs.Common
{
	public class PagedResult<T>
	{
		public List<T> Items { get; set; }
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
		public bool HasPreviousPage => PageNumber > 1;
		public bool HasNextPage => PageNumber < TotalPages;

		public PagedResult()
		{
			Items = new List<T>();
		}
	}

	public class PagedRequest
	{
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string SearchTerm { get; set; }
		public string SortBy { get; set; }
		public bool IsDescending { get; set; }
	}
}