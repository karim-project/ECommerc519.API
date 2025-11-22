using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerc519.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        ApplicationDBContext _context; //= new();
        private readonly IProductRepository _producrRepository;// = new();
        private readonly IRepository<Category> _categoryRepository;//= new();
        private readonly IRepository<Brand> _brandRepository;//= new();
        private readonly IRepository<ProductSubImg> _ProductSubImgRepository;//= new();
        private readonly IProductColorRepository _ProductColorRepository;// = new();
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDBContext context, IProductRepository producrRepository, IRepository<Category> categoryRepository, IRepository<Brand> brandRepository, IRepository<ProductSubImg> productSubImgRepository, IProductColorRepository productColorRepository , ILogger<ProductsController> logger)
        {
            _context = context;
            _producrRepository = producrRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _ProductSubImgRepository = productSubImgRepository;
            _ProductColorRepository = productColorRepository;
            _logger = logger;
        }
        [HttpPost("Get")]
        public async Task<IActionResult> GetAll(FillProductReaquest fillProductReaquest, CancellationToken cancellationToken, [FromQuery] int page = 1)
        {
            const decimal discount = 50;
            var products = await _producrRepository.GetAsync(includes: [e => e.category, e => e.brand], tracked: false, cancellationToken: cancellationToken);


            #region Filtter Product
            // Add Filtter
            FillProductResponse fillProductResponse = new();

            if (fillProductReaquest.name is not null)
            {
                products = products.Where(e => e.Name.Contains(fillProductReaquest.name));
                fillProductResponse.Name = fillProductReaquest.name;
            }

            if (fillProductReaquest.MainPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * e.Discont / 100 > fillProductReaquest.MainPrice);
                fillProductResponse.MainPrice = fillProductReaquest.MainPrice;
            }

            if (fillProductReaquest.MainPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * e.Discont / 100 < fillProductReaquest.MaxPrice);
                fillProductResponse.MaxPrice = fillProductReaquest.MaxPrice;
            }

            if (fillProductReaquest.categoryId is not null)
            {
                products = products.Where(e => e.CategoryId == fillProductReaquest.categoryId);
                fillProductResponse.CategoryId = fillProductReaquest.categoryId;
            }

            if (fillProductReaquest.brandid is not null)
            {
                products = products.Where(e => e.BrandId == fillProductReaquest.brandid);
                fillProductResponse.Brandid = fillProductReaquest.brandid;
            }

            if (fillProductReaquest.LessQuantity)
            {
                products = products.OrderBy(e => e.Quantity);
                fillProductResponse.LessQuantity = fillProductReaquest.LessQuantity;
            }


            //categories

            #endregion

            #region pagination
            //pagination
            paginationResponse paginationResponse = new();

            paginationResponse.TotalPages = Math.Ceiling(products.Count() / 8.0);
            paginationResponse.CurrentPage = page;
            products = products.Skip((page - 1) * 8).Take(8);
            #endregion

            return Ok(new
            {
                Products = products.AsEnumerable(),
                FilterProductResponse = fillProductResponse,
                PaginationResponse = paginationResponse

            });

                
        }
        [HttpGet("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var product = await _producrRepository.GetOneAsync(e => e.Id == id, includes: [e => e.ProductColors, e => e.ProductSubImgs], tracked: false, cancellationToken: cancellationToken);

            if (product == null)
                return NotFound();
            // return RedirectToAction("NotFoundPage", "Home");


            return Ok(product);


        }

        [HttpPost("")]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> Create(CreateProductRequest createProductRequest, CancellationToken cancellationToken)
        {
            var transation = _context.Database.BeginTransaction();

            Product product = createProductRequest.Adapt<Product>();
            try
            {
                // save img in wwwroot
                if (createProductRequest.img is not null && createProductRequest.img.Length > 0)
                {
                    var fillName = Guid.NewGuid().ToString() + Path.GetExtension(createProductRequest.img.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", fillName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                       await createProductRequest.img.CopyToAsync(stream);
                    }
                    // save img in DB
                    product.MainImg = fillName;
                }
                //save product  in DB with new img and name
                var ProductCreated = await _producrRepository.AddAsync(product, cancellationToken: cancellationToken);
                await _producrRepository.CommitAsync(cancellationToken);

                // subImgs
                if (createProductRequest.subImgs is not null && createProductRequest.subImgs.Count > 0)
                {
                    foreach (var item in createProductRequest.subImgs)
                    {
                        var fillName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", fillName);

                        using (var stream = System.IO.File.Create(filePath))
                        {
                           await item.CopyToAsync(stream);
                        }
                        await _ProductSubImgRepository.AddAsync(new()
                        {
                            Img = fillName,
                            ProductId = ProductCreated.Id,
                        }, cancellationToken: cancellationToken);
                    }
                    await _ProductSubImgRepository.CommitAsync(cancellationToken);
                }
                // Response.Cookies.Append("success-notification", "Add Product Succeessfully");

                if (createProductRequest.Colors is not null && createProductRequest.Colors.Any())
                {
                    foreach (var item in createProductRequest.Colors)
                    {
                        await _ProductColorRepository.AddAsync(new()
                        {
                            Color = item,
                            ProductId = ProductCreated.Id,
                        }, cancellationToken: cancellationToken);
                    }
                    await _ProductColorRepository.CommitAsync(cancellationToken);
                }

                transation.Commit();
                return CreatedAtAction(nameof(GetOne), new { id = product.Id }, new
                {
                    success_notifaction = "Add Product Successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                transation.Rollback();

                return BadRequest(new ErrorModelResponse
                {
                    Code = "Error While Saving the product",
                    Description = ex.Message,
                });
                
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> Edit(int id, UpdateProductRequest updateProductRequest, CancellationToken cancellationToken)
        {
            var productInDb = await _producrRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (productInDb == null)
                return NotFound();

            // --------- تحديث الصورة ----------
            if (updateProductRequest.img is not null && updateProductRequest.img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateProductRequest.img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await updateProductRequest.img.CopyToAsync(stream);
                }

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", productInDb.MainImg);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                productInDb.MainImg = fileName;
            }

            productInDb.Name = updateProductRequest.Name;
            productInDb.Status = updateProductRequest.Status;
            productInDb.Description = updateProductRequest.Description;
            productInDb.Price = updateProductRequest.Price;
            productInDb.Quantity = updateProductRequest.Quantity;
            productInDb.Discont = updateProductRequest.Discont;
            productInDb.CategoryId = updateProductRequest.CategoryId;
            productInDb.BrandId = updateProductRequest.BrandId;


            // --------- تحديث بيانات المنتج ----------
            _producrRepository.Update(productInDb);
            await _producrRepository.CommitAsync(cancellationToken);

            if (updateProductRequest.subImgs is not null && updateProductRequest.subImgs.Count > 0)
            {
                var oldproductsubimg = await _ProductSubImgRepository.GetAsync(e => e.ProductId == id);

                foreach (var item in oldproductsubimg)
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", item.Img);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                    _ProductSubImgRepository.Delete(item);
                }

                foreach (var item in updateProductRequest.subImgs)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }
                    await _ProductSubImgRepository.AddAsync(new()
                    {
                        Img = fileName,
                        ProductId = id,
                    });

                }
                await _ProductSubImgRepository.CommitAsync(cancellationToken);
            }

            // --------- تحديث الألوان ----------
            if (updateProductRequest.Colors is not null && updateProductRequest.Colors.Any())
            {

                var oldProductColors = await _ProductColorRepository.GetAsync(e => e.ProductId == id);

                _ProductColorRepository.RemoveRange(oldProductColors);

                foreach (var item in updateProductRequest.Colors)
                {
                    await _ProductColorRepository.AddAsync(new()
                    {
                        Color = item,
                        ProductId = id,
                    }, cancellationToken);
                }

                await _ProductColorRepository.CommitAsync(cancellationToken);
            }

            return NoContent();


        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {

            var product = await _producrRepository.GetOneAsync(e => e.Id == id, includes: [e => e.ProductSubImgs, e => e.ProductColors], cancellationToken: cancellationToken);
            if (product == null)
                return NotFound();
            // remove old photo
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", product.MainImg);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            foreach (var item in product.ProductSubImgs)
            {
                var SubImgOldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", item.Img);
                if (System.IO.File.Exists(SubImgOldPath))
                {
                    System.IO.File.Delete(SubImgOldPath);
                }
            }

            _producrRepository.Delete(product);
            await _producrRepository.CommitAsync(cancellationToken);

            return NoContent();
        }
        [HttpDelete("{productId}/{Img}")]
        [Authorize(Roles = $"{SD.Super_Admin_Role} ,{SD.Admin_Role}")]
        public async Task<IActionResult> DeleteSubImg(int productId, string Img, CancellationToken cancellationToken)
        {
            var productSubImgInDb = await _ProductSubImgRepository.GetOneAsync(e => e.ProductId == productId && e.Img == Img);

            if (productSubImgInDb is null)
                return NotFound();

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_img", productSubImgInDb.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _ProductSubImgRepository.Delete(productSubImgInDb);
            await _ProductSubImgRepository.CommitAsync(cancellationToken);

            return NoContent();
        }
    }
}
