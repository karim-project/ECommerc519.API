using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerc519.API.Areas.Admin.Controllers
{
    [Route("api/[Area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoriesController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);
            return Ok(categories.AsEnumerable());
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOne(int id , CancellationToken cancellationToken)
        {
            var category= await _categoryRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken, tracked: false);
            
            if (category is null) 
                return NotFound();

            return Ok(category);
        }
        [HttpPost]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> Create(Category category, CancellationToken cancellationToken)
        {

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _categoryRepository.CommitAsync(cancellationToken);

            return CreatedAtAction(nameof(GetOne), new { id = category.Id }, new 
            {
                success_notifaction = "Create Category Successfully"
            });

        }

        [HttpPut("{id}")]
        //[Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> Edit(int id ,Category category, CancellationToken cancellationToken)
        {
          var categryInDB = await _categoryRepository.GetOneAsync(e =>e.Id == id , cancellationToken: cancellationToken);

            if (categryInDB == null)
                return NotFound();

            categryInDB.Name = category.Name;
            categryInDB.Description = category.Description;
            categryInDB .Status = category.Status;

            await _categoryRepository.CommitAsync(cancellationToken);

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (category == null)
                return NotFound();

            _categoryRepository.Delete(category);
            await _categoryRepository.CommitAsync(cancellationToken);
            return NoContent();
        }

    }
}
