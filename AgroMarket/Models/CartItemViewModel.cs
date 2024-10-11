namespace AgroMarket.Models
{
    public class CartItemViewModel
    {
        public Guid ProductID { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity {get;set;}

    }
}
