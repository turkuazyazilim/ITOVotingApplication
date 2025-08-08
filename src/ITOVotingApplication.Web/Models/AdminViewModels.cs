using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Company;
using ITOVotingApplication.Core.DTOs.Contact;
using ITOVotingApplication.Core.DTOs.User;
using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	public class AdminDashboardViewModel
	{
		public string UserName { get; set; }
		public string FullName { get; set; }
		public int TotalCompanies { get; set; }
		public int TotalUsers { get; set; }
		public int TotalContacts { get; set; }
		public int TotalVotes { get; set; }
	}

	public class CompanyListViewModel
	{
		public PagedResult<CompanyDto> Companies { get; set; }
		public string SearchTerm { get; set; }
	}

	public class CreateCompanyViewModel
	{
		[Required(ErrorMessage = "Sicil numarası zorunludur.")]
		[Display(Name = "Sicil Numarası")]
		public string RegistrationNumber { get; set; }

		[Required(ErrorMessage = "Vergi numarası zorunludur.")]
		[Display(Name = "Vergi Numarası")]
		public string TaxNumber { get; set; }

		[Required(ErrorMessage = "Firma unvanı zorunludur.")]
		[Display(Name = "Firma Unvanı")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Şirket tipi zorunludur.")]
		[Display(Name = "Şirket Tipi")]
		public string CompanyType { get; set; }

		[Display(Name = "Ticaret Sicil No")]
		public string TradeRegistrationNumber { get; set; }

		[Display(Name = "Sermaye")]
		[DataType(DataType.Currency)]
		public decimal Capital { get; set; }

		[Display(Name = "Kayıt Adresi")]
		public string RegistrationAddress { get; set; }

		[Display(Name = "Derece")]
		public string Degree { get; set; }

		[Display(Name = "Üyelik Kayıt Tarihi")]
		[DataType(DataType.Date)]
		public DateTime MemberRegistrationDate { get; set; } = DateTime.Now;

		[Display(Name = "Meslek Grubu")]
		public string ProfessionalGroup { get; set; }

		[Display(Name = "NACE Kodu")]
		public string NaceCode { get; set; }

		[Display(Name = "İş Telefonu")]
		[Phone]
		public string OfficePhone { get; set; }

		[Display(Name = "Cep Telefonu")]
		[Phone]
		public string MobilePhone { get; set; }

		[Display(Name = "E-posta")]
		[EmailAddress]
		public string Email { get; set; }

		[Display(Name = "Web Sitesi")]
		[Url]
		public string WebSite { get; set; }
	}

	public class UserListViewModel
	{
		public PagedResult<UserDto> Users { get; set; }
		public string SearchTerm { get; set; }
	}

	public class CreateUserViewModel
	{
		[Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
		[Display(Name = "Kullanıcı Adı")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Şifre zorunludur.")]
		[StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Şifre")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Şifre Tekrar")]
		[Compare("Password", ErrorMessage = "Şifre ve şifre tekrarı uyuşmuyor.")]
		public string ConfirmPassword { get; set; }

		[Required(ErrorMessage = "E-posta zorunludur.")]
		[EmailAddress]
		[Display(Name = "E-posta")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Ad zorunludur.")]
		[Display(Name = "Ad")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Soyad zorunludur.")]
		[Display(Name = "Soyad")]
		public string LastName { get; set; }

		[Display(Name = "Roller")]
		public List<int> SelectedRoleIds { get; set; } = new List<int>();
	}

	public class ContactListViewModel
	{
		public PagedResult<ContactDto> Contacts { get; set; }
		public string SearchTerm { get; set; }
	}
}