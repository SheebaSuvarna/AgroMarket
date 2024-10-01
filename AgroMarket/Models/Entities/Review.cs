using System.ComponentModel.DataAnnotations;

namespace AgroMarket.Models.Entities
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        [Required(ErrorMessage = "ProductID is required.")]
        public int ProductID { get; set; }

        // Navigation property for Product
        [Required]
        public virtual Product? Product { get; set; }

        [Required(ErrorMessage = "CustomerID is required.")]
        public int CustomerID { get; set; }

        // Navigation property for Customer
        [Required]
        public virtual Customer? Customer { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Comment can't exceed 500 characters.")]
        public string? Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
