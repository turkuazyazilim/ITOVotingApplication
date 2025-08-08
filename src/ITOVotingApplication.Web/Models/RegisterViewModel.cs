﻿using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	/// <summary>
	/// Register view model for Account/Register page
	/// </summary>
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
		[Display(Name = "Kullanıcı Adı")]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır.")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "E-posta adresi zorunludur.")]
		[EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
		[Display(Name = "E-posta")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Ad zorunludur.")]
		[Display(Name = "Ad")]
		[StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Soyad zorunludur.")]
		[Display(Name = "Soyad")]
		[StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
		public string LastName { get; set; }

		[Required(ErrorMessage = "Şifre zorunludur.")]
		[StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Şifre")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Şifre Tekrar")]
		[Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
		public string ConfirmPassword { get; set; }
	}
}
