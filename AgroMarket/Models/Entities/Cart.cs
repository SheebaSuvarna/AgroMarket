using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroMarket.Models.Entities
{
    public class Cart
    {
        [Key, Column(Order = 0)]
        public Guid CustomerID { get; set; }

        public virtual Customer? Customer { get; set; }

        [Key, Column(Order = 1)]
        public Guid ProductID { get; set; }

        public virtual Product? Product { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
