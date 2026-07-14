using ECommerce_API_2.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce_API_2.Areas.Customer
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area(SD.CUSTOMER_AREA)]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> _ordersRepository;
        private readonly IRepository<OrderItems> _ordersItemsRepository;

        public OrdersController ( IRepository<Order> ordersRepository , IRepository<OrderItems> ordersItemsRepository)
        {
            _ordersRepository = ordersRepository;
            _ordersItemsRepository = ordersItemsRepository;
        }

        //show all orders
        [HttpGet]
        public async Task<IActionResult> Get( int? id , int page = 1 )
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null ) return NotFound();

            var userOrders = await _ordersRepository.GetAsync( o => o.ApplicationUserId == userId);
            List<OrderItems> orderItems = new List<OrderItems>();
            foreach ( var item in userOrders)
            {
                orderItems.AddRange(await _ordersItemsRepository.GetAsync( e => e.OrderId == item.Id));
            }

            if ( id is not null )
                userOrders = userOrders.Where(o => o.Id == id).ToList();

            if (orderItems.Count == 0) return NotFound();

            if (page < 1)
                page = 1;
            int pageSize = 10;
            int currentPage = page;
            double totalPages = Math.Ceiling((double)orderItems.Count / pageSize);
            orderItems = orderItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok( new 
            {
                UserOrders = userOrders,
                TotalPages = totalPages,
                CurrentPage = currentPage,
            });
        }
        //order details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne (int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            var userorder = await _ordersRepository.GetOneAsync(o => o.Id == id && o.ApplicationUserId == userId);
            var userorderItems = await _ordersItemsRepository.GetAsync(o => o.OrderId == id , includes: [ e =>e.Product]);


            return Ok(
                new
                {                       
                    userorder,
                    userorderItems
                });
        }
        //cancel 
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            var userorder = await _ordersRepository.GetOneAsync(o => o.Id == id && o.ApplicationUserId == userId);

            if (userorder == null) return NotFound();

            if (userorder.Status == OrderStatus.shipped)
            {
                return BadRequest(new ErrorResponse
                {
                    ErorMsg = "Order has already been shipped and cannot be cancelled."
                });
            }

            var options = new RefundCreateOptions
            {
                Reason = RefundReasons.Unknown,
                Amount = (long)(userorder.TotalPrice * 100), // Convert to cents
                PaymentIntent = userorder.PaymentId
            };

            var service = new RefundService();
            var seesion = service.Create(options);

            userorder.Status = OrderStatus.cancelled;
            userorder.PaymentStatus = PaymentStatus.refunded;
            await _ordersRepository.CommitAsync();

            return NoContent();
        }
        //rate
    }
}
