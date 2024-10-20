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

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,20}$",
   ErrorMessage = "Password must be 8-20 characters long, with at least one uppercase letter, one lowercase letter, and one number.")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        [MaxLength(10)]
        public string? PhoneNumber { get; set; }


        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(7, ErrorMessage = "Pin Code cannot exceed 7 characters.")]
        public string PinCode { get; set; }
    }
}
