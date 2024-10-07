using System.ComponentModel.DataAnnotations;

namespace AgroMarket.ViewModels
{
    public class RegisterRetailor
    {
        [MaxLength(25)]
        [Required(ErrorMessage = "First Name is Required")]
        public string FirstName { get; set; }

        [MaxLength(25)]
        public string LastName { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$",
         ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, and one number.")]
        [Display(Name = "Password")]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; }
        /* [Required(ErrorMessage = "Password is Required")]
         [DataType(DataType.Password)]
         [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
         [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$",
             ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, and one number.")]
         public string Password { get; set; }
        */
        /*[Required(ErrorMessage = "Confirm Password is Required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        */
        [MaxLength(10)]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; }

        [MaxLength(25)]
        [Required(ErrorMessage = "Farm Name is Required")]
        public string FarmName { get; set; }

        [MaxLength(100)]
        public string FarmAddress { get; set; }

        [MaxLength(7)]
        [Required(ErrorMessage = "Pin Code is Required")]
        [RegularExpression("^[0-9]{6,7}$", ErrorMessage = "Pin Code must be a 6-7 digit number.")]
        public string PinCode { get; set; }
    }
}
