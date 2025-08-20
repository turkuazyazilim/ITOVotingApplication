using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	public class CreateUserViewModel
	{
		[Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
		[Display(Name = "Kullanıcı Adı")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Şifre zorunludur.")]
		[DataType(DataType.Password)]
		[Display(Name = "Şifre")]
		public string Password { get; set; }

		[Required(ErrorMessage = "E-posta zorunludur.")]
		[EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
		[Display(Name = "E-posta")]
		public string Email { get; set; }

		[Required(ErrorMessage = "E-posta zorunludur.")]
		[Phone(ErrorMessage = "Geçerli bir telefon giriniz.")]
		[Display(Name = "Telefon")]
		public string PhoneNumber { get; set; }

		[Required(ErrorMessage = "Ad zorunludur.")]
		[Display(Name = "Ad")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Soyad zorunludur.")]
		[Display(Name = "Soyad")]
		public string LastName { get; set; }

		[Display(Name = "Roller")]
		public List<int> SelectedRoleIds { get; set; }
	}
}
