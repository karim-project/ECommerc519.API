using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerc519.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        //ApplicationDBContext _context = new();
        private readonly IRepository<Brand> _brandRepostiory; //= new();

        public BrandsController(IRepository<Brand> brandRepostiory)
        {
            _brandRepostiory = brandRepostiory;
        }
        [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var brands = await _brandRepostiory.GetAsync(tracked: false, cancellationToken: cancellationToken);
            return Ok(brands.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.Status

            }).AsEnumerable());
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var brand = await _brandRepostiory.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken,tracked:false);

            if (brand == null)
                return NotFound();

            return Ok(brand);
        }
        [HttpPost("")]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> Create(CreateBrandRequest createBrandRequest, CancellationToken cancellationToken)
        {
            Brand brand = createBrandRequest.Adapt<Brand>();

            if (createBrandRequest.Img is not null && createBrandRequest.Img.Length > 0)
            {
                // Save Img in wwwroot
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createBrandRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_img", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await createBrandRequest.Img.CopyToAsync(stream);
                }

                // Save Img in db
                brand.Img = fileName;
            }

            // Save brand in db
            await _brandRepostiory.AddAsync(brand, cancellationToken);
            await _brandRepostiory.CommitAsync(cancellationToken);

            return CreatedAtAction(nameof(GetOne), new { id = brand.Id }, new
            {
                success_notifaction = "Add Brand Successfully"
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> Edit(int id ,UpdateBrandRequest updateBrandRequest, CancellationToken cancellationToken)
        {

           
            var brandInDb = await _brandRepostiory.GetOneAsync(e => e.Id ==id , tracked: false, cancellationToken: cancellationToken);

            if (brandInDb == null)
                return NotFound();
            if (updateBrandRequest.NewImg is not null)
            {
                if (updateBrandRequest.NewImg.Length > 0)
                {
                    // save img in wwwroot
                    var fillName = Guid.NewGuid().ToString() + Path.GetExtension(updateBrandRequest.NewImg.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgs", fillName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        updateBrandRequest.NewImg.CopyTo(stream);
                    }
                    // remove old photo
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgs", brandInDb.Img);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                    // save img in DB
                    brandInDb.Img = fillName;
                }
            }
          
            brandInDb.Name = updateBrandRequest.Name;
            brandInDb.Status = updateBrandRequest.Status;
            brandInDb.Description = updateBrandRequest.Description;



            await _brandRepostiory.CommitAsync(cancellationToken);


            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {

            var brand = await _brandRepostiory.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (brand == null)
                return NotFound();
            // remove old photo
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_img", brand.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _brandRepostiory.Delete(brand);
            await _brandRepostiory.CommitAsync(cancellationToken);

            return NoContent();
        }
    }
}
