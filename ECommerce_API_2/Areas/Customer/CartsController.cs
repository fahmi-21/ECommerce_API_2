using ECommerce_API_2.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace ECommerce_API_2.Areas.Customer
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Promotion> _promotionRepo;
        private readonly IRepository<Cart> _cartRepo; 
        private readonly IRepository<Order> _orderRepo;
        private readonly ILogger<CartsController> _logger;
        private readonly IRepository<OrderItems> _orderItemsRepo;
        public CartsController(UserManager<ApplicationUser> userManager, IRepository<Product> productRepo, IRepository<Promotion> promotionRepo,
            IRepository<Cart> cartRepo , ILogger<CartsController> logger , IRepository<Order> OrderRepo , IRepository<OrderItems> orderItemsRepo)
        {
            _userManager = userManager;
            _productRepo = productRepo;
            _promotionRepo = promotionRepo;
            _cartRepo = cartRepo;
            _logger = logger;
            _orderRepo = OrderRepo;
            _orderItemsRepo = orderItemsRepo;
        }
        [HttpGet("AddToCart")]
        public async Task<IActionResult> AddToCart(int productId, int count)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _productRepo.GetOneAsync(p => p.Id == productId);

            if (user is null || product is null) return NotFound();

            var cartinDb = await _cartRepo.GetOneAsync(e => e.ApplicationUserId == user.Id && e.Product.Id == productId);
            if (cartinDb is null)
            {
                await _cartRepo.CreateAsync(new Cart
                {
                    ApplicationUserId = user.Id,
                    ProductId = productId,
                    Count = count,
                    ListPrice = (double)product.Price
                });
            }
            else
                cartinDb.Count += count;

            await _cartRepo.CommitAsync();
            return Ok( new SuccessRespones 
            { 
                Msg = "Product added to cart successfully" 
            } );
        }
        [HttpGet]
        public async Task<IActionResult> Get(string? code = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var userCarts = await _cartRepo
              .GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);

            if (code is not null)
            {
                var promotions = await _promotionRepo.GetAsync(e => e.Code == code);

                var moviesId = promotions.Select(e => e.ProductId);
                bool movieIsFoundded = true;

                foreach (var item in userCarts)
                {
                    if (moviesId.Contains(item.ProductId))
                    {
                        var discount = promotions.FirstOrDefault().Discount;

                        var discountClac = item.ListPrice - (item.ListPrice * (discount / 100));

                        item.ListPrice = discountClac;
                        await _promotionRepo.CommitAsync();
                         

                        return Ok(new SuccessRespones
                        {
                            Msg = $" Your Code {code} Has Been Applied Successfully"
                        });

                        break;
                    }
                    else
                    {
                        movieIsFoundded = false;
                    }
                }
                if (!movieIsFoundded)
                    return BadRequest(new ErrorResponse
                    {
                        ErorMsg= $" Your Code {code} Can Not Applied"
                    });
            }

            return Ok(userCarts);
        }
        [HttpPatch("{id}/Increment")]
        public async Task<IActionResult> Increment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var cart = await _cartRepo.GetOneAsync(
                e => e.Id == id && e.ApplicationUserId == user.Id,
                includes: new Expression<Func<Cart, object>>[]
                {
                    e => e.Product
                }
            );
            if (cart == null) return NotFound();

            string notificationMsg = string.Empty;

            if (cart.Count != cart.Product.Quantity)
            {
                cart.Count += 1;
                await _cartRepo.CommitAsync();
            }
            else
                notificationMsg = "No more stock available";
            

            return Ok( new SuccessRespones 
            { 
                Msg = notificationMsg
            } );
        }
        [HttpPatch("{id}/Decrement")]
        public async Task<IActionResult> Decrement(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var cart = await _cartRepo.GetOneAsync(
                e => e.Id == id && e.ApplicationUserId == user.Id,
                includes: new Expression<Func<Cart, object>>[]
                {
                    e => e.Product
                }
            );

            if (cart == null) return NotFound();
            if (cart.Count > 1)
            {
                cart.Count -= 1;
                await _cartRepo.CommitAsync();

            }

            return NoContent( );
        }
        [HttpPatch("{id}/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var cart = await _cartRepo.GetOneAsync(
                e => e.Id == id && e.ApplicationUserId == user.Id,
                includes: new Expression<Func<Cart, object>>[]
                {
                    e => e.Product  
                }
            );
            if (cart == null) return NotFound();

            _cartRepo.Delete(cart);
            await _cartRepo.CommitAsync();
            return NoContent();
        }
        [HttpGet("Pay")]
        public async Task<IActionResult> Pay()
        {


            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            Order order = new Order
            {
                ApplicationUserId = user.Id
            };

            await _orderRepo.CreateAsync(order);
            await _orderRepo.CommitAsync();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/checkout/success/{order.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/checkout/cancel"
            };

            var carts = await _cartRepo.GetAsync(e => e.ApplicationUserId == user.Id, includes: [ e => e.
            Product]);

            order.TotalPrice = carts.Sum(e => e.ListPrice);

            foreach (var item in carts)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                        },
                        UnitAmount = (long)item.ListPrice * 100
                    },
                    Quantity = item.Count
                });
            }

            var service = new SessionService();
            var session = service.Create(options);
            order.SessionId = session.Id;
            await _orderRepo.CommitAsync();
            return Ok(new SuccessRespones
            {
                Msg = "Redirecting to checkout...",
                OptinalData = [session.Url]
            });
        }
    }
}
