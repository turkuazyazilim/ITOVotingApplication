using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
    public class InvitationRegisterViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6, en fazla 100 karakter olmalıdır")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string LastName { get; set; }

        [Display(Name = "Saha Referans Kategorisi")]
        public int? FieldReferenceCategoryId { get; set; }

        [Display(Name = "Saha Referans Alt Kategorisi")]
        public int? FieldReferenceSubCategoryId { get; set; }
    }
}