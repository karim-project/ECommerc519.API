using ECommerc519.API.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ECommerc519.API.Areas.Customers.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customers")]
    [Authorize]
    public class HomesController : ControllerBase
    {
        private readonly IProductRepository _producrRepository;// = new();

        public HomesController(IProductRepository producrRepository)
        {
            _producrRepository = producrRepository;
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

            if (fillProductReaquest.isHot)
            {
                fillProductResponse.IsHot = fillProductReaquest.isHot;
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
        public async Task<IActionResult> Item(int id, CancellationToken cancellationToken)
        {
            //var products = await _context.Products.Include(e => e.category).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            var products =await _producrRepository.GetOneAsync(e => e.Id == id, includes: [e => e.category]);

            if (products is null)
                return NotFound();

            var relattedproducts = (await _producrRepository.GetAsync(e => e.Name.Contains(products.Name) && e.Id != products.Id, includes: [e=>e.CategoryId])).Skip(0).Take(4);

            return Ok(new 
            {
                Product = products,
                Relatedproducts = relattedproducts
            });
        }
    }
}
