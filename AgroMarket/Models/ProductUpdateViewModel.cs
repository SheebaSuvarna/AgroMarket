namespace AgroMarket.Models
{
    public class ProductUpdateViewModel
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; }  // Handle category update as needed
    }

}
