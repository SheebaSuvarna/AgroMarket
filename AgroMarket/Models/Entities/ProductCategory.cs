using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AgroMarket.Models.Entities
{
    public class ProductCategory
    {
        [Key]
        [Column(Order = 0)]
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Key]
        [Column(Order = 1 )]
        public Guid CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    }
}
