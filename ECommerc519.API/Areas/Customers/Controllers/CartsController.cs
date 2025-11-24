using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerc519.API.Areas.Customers.Controllers
{
    [Route("api/Area/[controller]")]
    [ApiController]
    [Area("Customers")]
    public class CartsController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartrepository;
        private readonly IRepository<Promotion> _promotionRepostiory;
        private readonly IProductRepository _productRepository;

        //public CartsController(UserManager<ApplicationUser> userManager, IRepository<Cart> cartrepository, IRepository<Promotion> promotionRepostiory, IProductRepository productRepository)
        //{
        //    _userManager = userManager;
        //    _cartrepository = cartrepository;
        //    _promotionRepostiory = promotionRepostiory;
        //    _productRepository = productRepository;
        //}
        //[HttpGet]
        //public async Task<IActionResult> GetAll(string code)
        //{
        //    var user =await _userManager.GetUserAsync(User);

        //    if (user is null)
        //        return NotFound(new
        //        {
        //            msg ="Invalid User"
        //        });
            
        //    var cart =await _cartrepository.GetAsync(e => e.ApplicationUserId == user.Id , includes: [e => e.Product , e => e.ApplicationUser]);

        //    var promotion = await _promotionRepostiory.GetOneAsync(e => e.Code == code && e.IsValid);
        //    if (promotion is not null)
        //    {
        //        var result = cart.FirstOrDefault(e => e!.ProductId == promotion!.ProductId);

        //        if (result is not null)
        //            result.Price -= result.Product.Price * (promotion!.Discount / 100);

        //        await _cartrepository.CommitAsync();
        //    }

        //    return Ok(cart);
        //}

        //public async Task<IActionResult> GetOne(int id  , CancellationToken cancellationToken)
        //{
            

        //    return Ok();
        //}
        //[HttpPost]
        //public async Task<IActionResult> AddCart(int count, int productId, CancellationToken cancellationToken)
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    if (user is null)
        //        return NotFound(new
        //        {
        //            msg = "Invalid User"
        //        });

        //    var productInDB = await _cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == productId);

        //    if (productInDB is not null)
        //    {
        //        productInDB.Count += count;
        //        await _cartrepository.CommitAsync(cancellationToken);

        //        return Ok(new
        //        {
        //            msg = "Update product to cart successfully"
        //        });
        //    }

        //    await _cartrepository.AddAsync(new()
        //    {
        //        ProductId = productId,
        //        Count = count,
        //        ApplicationUser = user,
        //        Price = (await _productRepository.GetOneAsync(e => e.Id == productId)).Price
        //    }, cancellationToken: cancellationToken);

        //    await _cartrepository.CommitAsync(cancellationToken);

        //    return Ok(new
        //    {
        //        msg = "Add product to cart successfully"
        //    });
        //}

    }
}
