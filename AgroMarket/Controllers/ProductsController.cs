using Microsoft.AspNetCore.Mvc;

namespace AgroMarket.Controllers
{
    public class ProductsController : Controller
    {
        [HttpGet]
        public IActionResult Addproduct()
        {
            return View();
        }
    }
}
