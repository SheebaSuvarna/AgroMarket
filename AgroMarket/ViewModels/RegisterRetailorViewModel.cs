using System.ComponentModel.DataAnnotations;

namespace AgroMarket.ViewModels
{
    public class RegisterRetailorViewModel
    {
        [MaxLength(25)]
        //[Required(ErrorMessage = "First Name is Required")]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(25)]
        [Required]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
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


        [MaxLength(10)]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; }

        [MaxLength(25)]
        [Required]
        //[Required(ErrorMessage = "Farm Name is Required")]
        public string FarmName { get; set; }

        [MaxLength(100)]
        public string FarmAddress { get; set; }

        [MaxLength(7)]
        [Required]

        [RegularExpression("^[0-9]{6,7}$", ErrorMessage = "Pin Code must be a 6-7 digit number.")]
        public string PinCode { get; set; }
    }
}
