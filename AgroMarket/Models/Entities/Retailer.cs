using System.ComponentModel.DataAnnotations;

namespace AgroMarket.Models.Entities
{
    public class Retailer
    {

        [Key]
        public Guid RetailerID { get; set; } = Guid.NewGuid();

        [MaxLength(25)]
        [Required(ErrorMessage = "First Name is Required")]
        public string? FirstName { get; set; }
        [MaxLength(25)]
        public string? LastName { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is Required")]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [MaxLength(10)]
        public string? PhoneNumber { get; set; }

        [MaxLength(25)]
        [Required(ErrorMessage = "FarmName is Required")]
        public string? FarmName { get; set; }


        [MaxLength(100)]
        public string? FarmAddress { get; set; }

        [MaxLength(7)]
        public string? PinCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

       
        public virtual ICollection<Product>? Product { get; set; }

    }
}
