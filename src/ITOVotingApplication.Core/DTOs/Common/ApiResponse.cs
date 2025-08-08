namespace ITOVotingApplication.Core.DTOs.Common
{
	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public T Data { get; set; }
		public List<string> Errors { get; set; }

		public ApiResponse()
		{
			Errors = new List<string>();
		}

		public static ApiResponse<T> SuccessResult(T data, string message = null)
		{
			return new ApiResponse<T>
			{
				Success = true,
				Data = data,
				Message = message ?? "İşlem başarıyla tamamlandı."
			};
		}

		public static ApiResponse<T> ErrorResult(string message, List<string> errors = null)
		{
			return new ApiResponse<T>
			{
				Success = false,
				Message = message,
				Errors = errors ?? new List<string>()
			};
		}
	}
}