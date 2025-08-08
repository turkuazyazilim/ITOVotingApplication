using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
		[Display(Name = "Kullanıcı Adı")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Şifre zorunludur.")]
		[DataType(DataType.Password)]
		[Display(Name = "Şifre")]
		public string Password { get; set; }
	}
}