using System.ComponentModel.DataAnnotations;

namespace AgroMarket.Models.Entities
{
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        [Required(ErrorMessage = "OrderID is required.")]
        public int OrderID { get; set; }

        // Navigation property for Order
        [Required]
        public virtual Order? Order { get; set; }

        [Required(ErrorMessage = "ProductID is required.")]
        public int ProductID { get; set; }

        // Navigation property for Product
        [Required]
        public virtual Product? Product { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
    }
}
