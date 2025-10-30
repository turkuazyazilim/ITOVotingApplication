using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	public class CreateContactViewModel
	{
		[Required(ErrorMessage = "Firma seçimi zorunludur.")]
		[Display(Name = "Firma")]
		public int CompanyId { get; set; }

		[Required(ErrorMessage = "Ad zorunludur.")]
		[Display(Name = "Ad")]
		[StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Soyad zorunludur.")]
		[Display(Name = "Soyad")]
		[StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
		public string LastName { get; set; }

		[Display(Name = "TC Kimlik No")]
		[StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
		[RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır.")]
		public string IdentityNum { get; set; }

		[Required(ErrorMessage = "Yetki tipi zorunludur.")]
		[Display(Name = "Yetki Tipi")]
		public int AuthorizationType { get; set; }

		[Display(Name = "Cep Telefonu")]
		[StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
		[Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
		public string MobilePhone { get; set; }

		[Display(Name = "E-posta")]
		[StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
		[EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
		public string Email { get; set; }

		[Display(Name = "Oy Kullanma Yetkisi")]
		public bool EligibleToVote { get; set; } = true;
	}
}