using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
//using System.Web.Mvc;
using System.Web.Security;

namespace Pnbp.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage="Nama pengguna harus diisi")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage="Kata sandi harus diisi")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        bool? rememberMe;
        [Display(Name = "Tetap login?")]
        public bool RememberMe
        {
            get { return rememberMe ?? false; }
            set { rememberMe = value; }
        }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Nama pengguna harus diisi")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Kata sandi harus diisi")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Konfirmasi kata sandi harus diisi")]
        [Compare("Password", ErrorMessage="Konfirmasi password harus sama dengan password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Alamat email harus diisi")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "alamat email harus diisi")]
        [Display(Name = "alamat email")]
        public string Email { get; set; }
    }
}