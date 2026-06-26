using ECommerce_API_2.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_API_2.Areas.Customer
{
    [Route("[area]/[controller]")]
    [Area(SD.CUSTOMER_AREA)]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> _productRepo;
        public ProductsController(IRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }
        [HttpGet]
        public async Task<IActionResult> index(int? category)
        {
            const double discount = 50;

            var products = (await _productRepo.GetAsync( e => e.Discount == discount, includes: [e => e.Category ], tracked: false)).AsEnumerable();


            if (category is not null)
                products = products.Where(p => p.Category.Id == category).ToList();

            products = products.Skip(0)
                               .Take(5);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details( int Id )
        {
            var product = await _productRepo.GetOneAsync(p => p.Id == Id, tracked: false);

            if ( product is null) return NotFound();

            var sameCategories = ( await _productRepo.GetAsync( e => e.CategoryId == product.CategoryId && e.Id != product.Id))
                .Skip(0)
                .Take(4);

            var minPrices = product.Price - product.Price * 0.10;
            var maxPrices = product.Price + product.Price * 0.10;

            var samePriceProducts = ( await _productRepo.GetAsync(e => e.Price >= minPrices && e.Price <= maxPrices && e.Id != product.Id))
                .Skip(0)
                .Take(4);

            var relatedProducts = (await _productRepo.GetAsync( e=> e.Name.Contains(product.Name) && e.Id != product.Id))
                .Skip(0)
                .Take(4);

            return Ok( new ProductWithRelatedResponse
            {
                Product = product,
                SameCategories = sameCategories.ToList(),
                SamePrice = samePriceProducts.ToList(),
                RelatedProducts = relatedProducts.ToList()
            });
        }

    }
}
