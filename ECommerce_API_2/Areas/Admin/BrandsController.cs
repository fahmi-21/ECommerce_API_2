using ECommerce_API_2.DTOs.Requests;
using ECommerce_API_2.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_API_2.Areas.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IRepository<Brand> _brandRepo;
        public BrandsController(IRepository<Brand> brandRepo)
        {
            _brandRepo = brandRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBrands( string? name, int page = 1)
        {
            var brands = await _brandRepo.GetAsync( tracked: false);

            if (name is not null)
                brands = brands.Where(b => b.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();

            if (page < 1)
                page = 1;

            int curentpage = page;
            double totalPages = Math.Ceiling((double)brands.Count / 5.0);
            brands = brands.Skip((curentpage - 1) * 5).Take(5).ToList();

            return Ok( new BrandResponse()
            {
                Brand = brands,
                TotalPages = totalPages,
                CurrentPage = curentpage
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBrandById([FromRoute] int id)
        {
            var brand = await _brandRepo.GetOneAsync( e => e.Id == id );

            if (brand == null)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErorMsg = "Invalid Category Data",
                });
            }
            return Ok(brand);
        }
        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromForm] BrandCreateRequest brandRequest)
        {
            var brand = brandRequest.Adapt<Brand>();
            
            if (brandRequest.Logo is not null && brandRequest.Logo.Length > 0)
            {
                string ImgName = Guid.NewGuid().ToString() + Path.GetExtension(brandRequest.Logo.FileName);
                string imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Imgs\\BrandsLogos", ImgName);
                using (var stream = new FileStream(imgPath, FileMode.Create))
                {
                    await brandRequest.Logo.CopyToAsync(stream);
                }
                brand.Logo= ImgName;
            }
            await _brandRepo.CreateAsync(brand);
            await _brandRepo.CommitAsync();

            return Ok(new SuccessRespones()
            {
                Msg = "Brand Created Successfully"
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateBrand( [FromForm]int Id, [FromForm] BrandUpdateRequest brandUpdateRequest)
        {
            Brand? brandInDb = await _brandRepo.GetOneAsync(e => e.Id == Id , tracked: false);
            if (brandInDb == null) NotFound();
           
            if (brandUpdateRequest.Logo is not null && brandUpdateRequest.Logo.Length > 0)
            {
                string newImgName = Guid.NewGuid().ToString() + Path.GetExtension(brandUpdateRequest.Logo.FileName);
                string imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Imgs\\BrandsLogos", newImgName);
                using (var stream = new FileStream(imgPath, FileMode.Create))
                {
                    brandUpdateRequest.Logo.CopyToAsync(stream);
                }
               
                var oldImgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Imgs\\BrandsLogos", brandInDb.Logo);
                if (System.IO.File.Exists(oldImgPath))
                {
                    System.IO.File.Delete(oldImgPath);
                }

                brandInDb.Logo = newImgName;
            }
            else
            {
                brandInDb.Logo = brandInDb.Logo;
            }

            brandInDb.Status = brandUpdateRequest.status;
            brandInDb.Name = brandUpdateRequest.Name;

            _brandRepo.Edit(brandInDb);
            await _brandRepo.CommitAsync();

            return Ok(new SuccessRespones()
            {
                Msg = "Brand Updated Successfully"
            });

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand([FromRoute] int id)
        {
            var brand = await _brandRepo.GetOneAsync(e => e.Id == id, tracked: false);
            if (brand == null)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErorMsg = "Invalid Brand Data",
                });
            }

            var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Imgs\\BrandsLogos", brand.Logo);
            if (System.IO.File.Exists(imgPath))
            {
                System.IO.File.Delete(imgPath);
            }

            _brandRepo.Delete(brand);
            await _brandRepo.CommitAsync();

            return Ok(new SuccessRespones()
            {
                Msg = "Brand Deleted Successfully"
            });
        }


    }
}
