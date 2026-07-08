using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace ECommerce_API_2.Areas.Customer
{
    [Route("[Area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area(SD.CUSTOMER_AREA)]
    public class CheckoutsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Logger<CheckoutsController> _logger;
        private IRepository<Order> _ordersRepo;
        private readonly IRepository<Cart> _cartRepo;
        private readonly IRepository<OrderItems> _orderItemsRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutsController ( IRepository<Order> ordersordersRepository , AppDbContext context , Logger<CheckoutsController> logger
            , IRepository<Cart> cartRepo , IRepository<OrderItems> orderItemsRepo , UserManager<ApplicationUser> userManager)
        { 

            _context = context;
            _logger = logger;
            _ordersRepo= ordersordersRepository;
            _cartRepo = cartRepo;
            _orderItemsRepo = orderItemsRepo;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Success ( int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return NotFound();

            var transaction = _context.Database.BeginTransaction();

            try
            {
                var order = _ordersRepo.GetOneAsync( e => e.Id == id && e.ApplicationUserId == userId );
                

                transaction.Commit();
            }
            catch (Exception ex)
            {

                transaction.Rollback();
                _logger.LogError($"Error{ex.Message}");
            }

            return Ok();
        }
        public IActionResult Cancel()
        {
            return Ok();
        }
    }
}
