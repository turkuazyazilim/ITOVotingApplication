using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	public class CreateCompanyViewModel
	{
		[Required(ErrorMessage = "Sicil numarası zorunludur.")]
		[Display(Name = "Sicil No")]
		public string RegistrationNumber { get; set; }

		[Required(ErrorMessage = "Vergi numarası zorunludur.")]
		[Display(Name = "Vergi No")]
		public string TaxNumber { get; set; }

		[Required(ErrorMessage = "Firma ünvanı zorunludur.")]
		[Display(Name = "Ünvan")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Firma tipi zorunludur.")]
		[Display(Name = "Firma Tipi")]
		public string CompanyType { get; set; }

		[Display(Name = "Ticaret Sicil No")]
		public string TradeRegistrationNumber { get; set; }

		[Display(Name = "Sermaye")]
		public decimal? Capital { get; set; }

		[Display(Name = "Kayıt Adresi")]
		public string RegistrationAddress { get; set; }

		[Display(Name = "Derece")]
		public string Degree { get; set; }

		[Display(Name = "Üyelik Kayıt Tarihi")]
		public DateTime? MemberRegistrationDate { get; set; }

		[Display(Name = "Meslek Grubu")]
		public string ProfessionalGroup { get; set; }

		[Display(Name = "NACE Kodu")]
		public string NaceCode { get; set; }

		[Display(Name = "İş Telefonu")]
		[Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
		public string OfficePhone { get; set; }

		[Display(Name = "Cep Telefonu")]
		[Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
		public string MobilePhone { get; set; }

		[Display(Name = "E-posta")]
		[EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
		public string Email { get; set; }

		[Display(Name = "Web Sitesi")]
		[Url(ErrorMessage = "Geçerli bir URL giriniz.")]
		public string WebSite { get; set; }
	}
}
