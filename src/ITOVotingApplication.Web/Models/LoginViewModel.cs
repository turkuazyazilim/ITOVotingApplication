using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Contact;
using ITOVotingApplication.Core.DTOs.User;
using ITOVotingApplication.Core.DTOs.Vote;
using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	/// <summary>
	/// Login view model for Account/Login page
	/// </summary>
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
		[Display(Name = "Kullanıcı Adı")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Şifre zorunludur.")]
		[DataType(DataType.Password)]
		[Display(Name = "Şifre")]
		public string Password { get; set; }

		[Display(Name = "Beni Hatırla")]
		public bool RememberMe { get; set; }
	}
}