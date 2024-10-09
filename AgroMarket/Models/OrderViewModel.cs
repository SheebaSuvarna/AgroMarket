using System.ComponentModel.DataAnnotations;
using AgroMarket.Models.Entities;

namespace AgroMarket.Models
{
    public class OrderViewModel
    {
        public Guid CustomerID { get; set; } = Guid.Parse("8491C02D-7309-4888-AC89-0AA2EFE0AA99");
        public List<Cart> CartItems { get; set; } = new List<Cart>();

        [Required(ErrorMessage = "Shipping Address is required.")]
        [MaxLength(200, ErrorMessage = "Shipping Address can't exceed 200 characters.")]
        public string ShippingAddress { get; set; }

        public decimal TotalAmount { get; set; }
    }

}
