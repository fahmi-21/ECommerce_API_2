using ECommerce_API_2.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

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

        [HttpGet("{id}/success")]
        public async Task<IActionResult> Success ( int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return NotFound();

            var transaction = _context.Database.BeginTransaction();

            try
            {
                //1-)Update Order Prop
                var order = await _ordersRepo.GetOneAsync( e => e.Id == id && e.ApplicationUserId == userId );
                if (order == null) return NotFound();

                var service = new SessionService();
                var session = service.Get(order.SessionId);

                order.PaymentId = session.PaymentIntentId;
                order.Status = OrderStatus.inproccesssing;
                order.PaymentStatus = PaymentStatus.Completed;

                await _ordersRepo.CommitAsync();


                //2-)Move Cart => orderItems 
                var carts = await _cartRepo.GetAsync(e => e.ApplicationUserId == userId, includes: [e => e.Product]);
                foreach (var cart in carts)
                {
                    await _orderItemsRepo.CreateAsync(new OrderItems
                    {
                        OrderId = order.Id,
                        ProductId = cart.ProductId,
                        CurrentPrice = cart.ListPrice,
                        Count = cart.Count
                    });
                    var product = cart.Product;
                    product.Quantity -= cart.Count;
                    _context.Products.Update(product);
                    _cartRepo.Delete(cart);
                }

                await _ordersRepo.CommitAsync();

                //3-)decrease product quantity
                foreach (var item in carts)
                {
                   item.Product.Quantity -= item.Count;
                }

                //4-)remove cart 
                foreach (var item in carts)
                {
                    _cartRepo.Delete(item);
                }
                await _cartRepo.CommitAsync();

                transaction.Commit();

                return Ok(new SuccessRespones
                {
                    Msg = "Payment processed successfully"
                });

            }
            catch (Exception ex)
            {

                transaction.Rollback();
                _logger.LogError($"Error{ex.Message}");
                return BadRequest(new ErrorResponse
                {
                    ErorMsg = "An error occurred while processing your payment"
                });
            }
        }
        [HttpGet("{id}/cancel")]
        public async Task<IActionResult> Cancel( int id )
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return NotFound();

            var order = await _ordersRepo.GetOneAsync(e => e.Id == id && e.ApplicationUserId == userId);
            if (order == null) return NotFound();

            
            order.Status = OrderStatus.inproccesssing;
            order.PaymentStatus = PaymentStatus.Completed;

            await _ordersRepo.CommitAsync();

            return Ok( new ErrorResponse
            {
                ErorMsg = "Payment has been cancelled"
            });
        }
    }
}
