using System.ComponentModel.DataAnnotations;

namespace AgroMarket.Models.Entities
{
    public class Category
    {

        [Key]
        public Guid CategoryID { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        [Required(ErrorMessage = "Category Name is Required")]
        public string? CategoryName { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        public virtual ICollection<ProductCategory>? ProductCategory { get; set; } // Navigation property
    }
}
