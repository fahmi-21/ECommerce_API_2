using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_API_2.Areas.Customer
{
    [Route("[Area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area(SD.CUSTOMER_AREA)]
    public class CheckoutsController : ControllerBase
    {
        private IRepository<Order> _ordersRepository;

        public IActionResult Success ()
        {

            return Ok();
        }
        public IActionResult Cancel()
        {
            return Ok();
        }
    }
}
