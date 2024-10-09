using AgroMarket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace AgroMarket.Controllers
{
    public class RetailerController : Controller
    {

        private readonly ApplicationDbContext dbContext;
        public RetailerController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }



        public IActionResult AddRetailer()
        {
            return View();
        }

        public IActionResult AddRetailer(AddUniqueConstraintOperation viewModel)
        {
            return View();
        }
    }
}
