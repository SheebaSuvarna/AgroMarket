using System.ComponentModel.DataAnnotations;

namespace AgroMarket.ViewModels
{
    public class RegisterCustomerModel
    {
        [MaxLength(25)]
        //[Required(ErrorMessage = "First Name is Required")]
        //[Required]
        public string FirstName { get; set; }

        [MaxLength(25)]
        public string LastName { get; set; }

        [EmailAddress]
        //  [Required(ErrorMessage = "Email is Required")]
        // [Required]
        public string Email { get; set; }

        // [Required]
        //[Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$",
          ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, and one number.")]
        [Display(Name = "Password")]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]
        public string Password { get; set; }

        //  [Required(ErrorMessage = "Confirm Password is required.")]
        // [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; }

        //[Required]
        /*[MaxLength(10, ErrorMessage = "Phone number cannot be longer than 10 digits.")]
        [Phone]*/

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(7, ErrorMessage = "Pin Code cannot exceed 7 characters.")]
        public string PinCode { get; set; }
    }
}
