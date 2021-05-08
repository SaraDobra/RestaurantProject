using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateCMS.Models {
    public class LoginModel {
        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Display(Name = "Password")]
        [Required]
        [MinLength(8, ErrorMessage = "Password must be 8 characters or longer!")]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
}
