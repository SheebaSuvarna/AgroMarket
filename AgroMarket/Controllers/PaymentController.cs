using AgroMarket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket.Controllers
{
	public class PaymentController : Controller
	{
		private readonly ApplicationDbContext _context;

		public PaymentController(ApplicationDbContext context)
		{
			_context = context;
		}
		public IActionResult Payment(Guid orderId)
		{
			var order = _context.Orders
								.Include(o => o.OrderItem)
								.ThenInclude(oi => oi.Product)
								.FirstOrDefault(o => o.OrderID == orderId);

			if (order == null)
			{
				return NotFound();
			}

			// Render payment page with order summary
			return View(order);
		}

	}
}
