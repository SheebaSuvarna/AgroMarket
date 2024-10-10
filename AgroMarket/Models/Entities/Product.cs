using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AgroMarket.Models.Entities
{
    public class Product
    {
        [Key]
        public Guid ProductID { get; set; } = Guid.NewGuid();


        [ForeignKey("Retailer")]
        public Guid RetailerID { get; set; }
        public Retailer? Retailer { get; set; } //naviagtion property


        [MaxLength(25)]
        [Required(ErrorMessage = "Product Name is Required")]
        public string? ProductName { get; set; }



        [MaxLength(100)]
        public string? Description { get; set; }


        // Property for handling image uploads (ignored by EF)
        [NotMapped]
        public IFormFile? ImageFile { get; set; }  // IFormFile is ignored by EF

        public string? ImageUrl { get; set; }  // Store image URL or file path


        [Required(ErrorMessage = "Price is Required")]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }


        [Required(ErrorMessage = "Stock Quantity is Required")]
        public int StockQuantity { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        public virtual ICollection<Cart>? Cart { get; set; } // Navigation property for related carts

        public virtual ICollection<ProductCategory>? ProductCategory { get; set; }
        public virtual ICollection<Review>? Review { get; set; }

    }
}
