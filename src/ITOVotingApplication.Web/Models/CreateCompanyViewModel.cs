using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	// CreateCompanyViewModel.cs - Updated model

	public class CreateCompanyViewModel
	{
		[Required(ErrorMessage = "Sicil numarası zorunludur.")]
		[Display(Name = "Sicil Numarası")]
		public string RegistrationNumber { get; set; }

		[Required(ErrorMessage = "Firma unvanı zorunludur.")]
		[Display(Name = "Firma Unvanı")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Şirket tipi zorunludur.")]
		[Display(Name = "Şirket Tipi")]
		public string CompanyType { get; set; }

		[Display(Name = "Ticaret Sicil No")]
		[Required(ErrorMessage = "Ticaret Sicil No zorunludur.")]
		public string TradeRegistrationNumber { get; set; }

		[Display(Name = "Sermaye")]
		[DataType(DataType.Currency)]
		public decimal Capital { get; set; }

		[Display(Name = "Kayıt Adresi")]
		[Required(ErrorMessage = "Adres zorunludur.")]
		public string RegistrationAddress { get; set; }

		[Required(ErrorMessage = "Derece zorunludur.")]
		[Display(Name = "Derece")]
		[Range(1, 5, ErrorMessage = "Derece 1-5 arasında olmalıdır.")]
		public string Degree { get; set; }

		[Display(Name = "Üyelik Kayıt Tarihi")]
		[DataType(DataType.Date)]
		public DateTime MemberRegistrationDate { get; set; } = DateTime.Now;

		[Display(Name = "Meslek Grubu")]
		[Required(ErrorMessage = "Meslek grubu zorunludur.")]
		public string ProfessionalGroup { get; set; }

		[Display(Name = "NACE Kodu")]
		[Required(ErrorMessage = "NACE kodu zorunludur.")]
		public string NaceCode { get; set; }

		[Display(Name = "İş Telefonu")]
		[Phone]
		public string? OfficePhone { get; set; }

		[Display(Name = "Cep Telefonu")]
		[Phone]
		public string? MobilePhone { get; set; }

		[Display(Name = "E-posta")]
		[EmailAddress]
		public string? Email { get; set; }

		[Display(Name = "Web Sitesi")]
		public string? WebSite { get; set; } // Optional - no Required attribute

		[Display(Name = "Durum")]
		public bool IsActive { get; set; } = true; // Default active

		[Display(Name = "2022 Yetki Belgesi Var mı?")]
		public bool Has2022AuthorizationCertificate { get; set; } = true;
	}
}
