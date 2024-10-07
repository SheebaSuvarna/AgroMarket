using System.ComponentModel.DataAnnotations;

namespace AgroMarket.Models.Entities
{
    public class Order
    {
        [Key]
        public Guid OrderID { get; set; } = Guid.NewGuid();
        [Required(ErrorMessage = "Customer ID is required.")]
        public Guid CustomerID { get; set; }

        // Navigation property
        [Required]
        public virtual Customer? Customer { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Total Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total Amount must be greater than 0.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Shipping Address is required.")]
        [MaxLength(200, ErrorMessage = "Shipping Address can't exceed 200 characters.")]
        public string? ShippingAddress { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [MaxLength(20, ErrorMessage = "Status can't exceed 20 characters.")]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Order must have at least one item.")]
        public ICollection<OrderItem> OrderItem { get; set; } = new List<OrderItem>();
    }
}
