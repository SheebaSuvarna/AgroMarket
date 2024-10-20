using AgroMarket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket.Controllers
{
	[Route("Payment")]
	public class PaymentController : Controller
	{
		private readonly ApplicationDbContext _context;

		public PaymentController(ApplicationDbContext context)
		{
			_context = context;
		}
		[HttpPost("/{orderId}")]
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
