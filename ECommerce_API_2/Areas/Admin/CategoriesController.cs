using ECommerce_API_2.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_API_2.Areas.Admin
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    [Authorize] //(Roles = $"{SD.SUPER_ADMIN_ROLE} , {SD.ADMIN_ROLE} , {SD.EMPLOYEE_ROLE} ")
    public class CategoriesController : ControllerBase
    {
        public IRepository<Category> Categories;
        public CategoriesController(IRepository<Category> categories)
        {
            Categories = categories;
        }
        [HttpGet]
        
        public async Task<IActionResult> Get(string? name, int page = 1)
        {

            var categories = await Categories.GetAsync(tracked: false);

            if (name is not null)
                categories = categories.Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();

            if (page < 1)
                page = 1;

            int curentpage = page;
            double totalPages = Math.Ceiling((double)categories.Count / 5.0);
            categories = categories.Skip((curentpage - 1) * 5).Take(5).ToList();



            return Ok(new CategoriesDTO
            {
                category = categories,
                totalPages = totalPages,
                CurrentPage = curentpage
            });
        }
        [HttpGet("{id}")]
        
        public  async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var category = await Categories.GetOneAsync(c => c.Id == id, tracked: false);

            if (category is null || category.Id == 0)
                return BadRequest(new SuccessRespones()
                {
                    Msg = "Invalid Category Data",
                });
            return Ok(category);
        }
        [HttpPost]
       
        public async Task<IActionResult> Create(Category category)
        {
            await Categories.CreateAsync(category);
            await Categories.CommitAsync();
            return Ok(new SuccessRespones()
            {
                Msg = "Category Created Successfully",
            });
        }
        [HttpPut("{id}")]
        
        public async Task<IActionResult> UpDate([FromRoute] int id , Category category)
        {
            var categoryinDb = await Categories.GetOneAsync(c => c.Id == id);

            if (categoryinDb is null) return NotFound();

            Categories.Edit(categoryinDb);
            await Categories.CommitAsync();
            return Ok(new SuccessRespones()
            {
                Msg = "Category updated Successfully",
            });
        }
        [HttpPatch("{id}")]
        
        public async Task<IActionResult> UpDateStatus([FromRoute] int id)
        {
            var categoryinDb = await Categories.GetOneAsync(c => c.Id == id);

            if (categoryinDb is null) return NotFound();
            
            categoryinDb.Status = !categoryinDb.Status;

            await Categories.CommitAsync();
            return Ok(new SuccessRespones()
            {
                Msg = "Category updated Successfully",
            });
        }
        [HttpDelete("{id}")]
        
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var category = await Categories.GetOneAsync(c => c.Id == id);

            if (category is null || category.Id == 0)
                return BadRequest(new SuccessRespones()
                {
                    Msg = "Invalid Category Data",
                });
            Categories.Delete(category);
            await Categories.CommitAsync();

            return Ok(new SuccessRespones()
            {
                Msg = "Category Deleted Successfully",
            });
        }
    }
}
