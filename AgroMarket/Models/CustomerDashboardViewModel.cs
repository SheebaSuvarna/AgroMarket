using AgroMarket.Models.Entities;

namespace AgroMarket.Models
{
	public class CustomerDashboardViewModel
	{
		public List<Order> Orders { get; set; }
		public List<Product> Products { get; set; }
        public List<string> Categories { get; set; }


    }
}
