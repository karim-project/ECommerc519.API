using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe.Climate;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerc519.API.Areas.Customers.Controllers
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area("Customers")]
    [Authorize]
    public class CartsController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartrepository;
        private readonly IRepository<Promotion> _promotionRepostiory;
        private readonly IProductRepository _productRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Models.Order> _orderRepository;

        public CartsController(UserManager<ApplicationUser> userManager, IRepository<Cart> cartrepository, IRepository<Promotion> promotionRepostiory, IProductRepository productRepository,IRepository<OrderItem> orderItemRepository , IRepository<Models.Order> orderRepository)
        {
            _userManager = userManager;
            _cartrepository = cartrepository;
            _promotionRepostiory = promotionRepostiory;
            _productRepository = productRepository;
           _orderItemRepository = orderItemRepository;
           _orderRepository = orderRepository;
        }
        [HttpGet("")]
        public async Task<IActionResult> GetAll(string code)
        {
           var userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userid is  null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userid);

            if (user is null)
                return NotFound(new
                {
                    msg = "Invalid User"
                });

            var cart = await _cartrepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product, e => e.ApplicationUser]);

            var promotion = await _promotionRepostiory.GetOneAsync(e => e.Code == code && e.IsValid);
            if (promotion is not null)
            {
                var result = cart.FirstOrDefault(e => e!.ProductId == promotion!.ProductId);

                if (result is not null)
                    result.Price -= result.Product.Price * (promotion!.Discount / 100);

                await _cartrepository.CommitAsync();
            }

            return Ok(new
            {
                Cart = cart,
                Promotion = promotion,
            });
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> AddCart(int count, int id, CancellationToken cancellationToken)
        {
            var userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userid is null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userid);

            if (user is null)
                return NotFound(new
                {
                    msg = "Invalid User"
                });

            var productInDB = await _cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == id);

            if (productInDB is not null)
            {
                productInDB.Count += count;
                await _cartrepository.CommitAsync(cancellationToken);

                return CreatedAtAction(nameof(GetAll) , new
                
                     {
                    msg = "Update product to cart successfully"
                });
            
               
            }

            await _cartrepository.AddAsync(new()
            {
                ProductId = id,
                Count = count,
                ApplicationUser = user,
                Price = (await _productRepository.GetOneAsync(e => e.Id == id)).Price
            }, cancellationToken: cancellationToken);

            await _cartrepository.CommitAsync(cancellationToken);

            return Ok(new
            {
                msg = "Add product to cart successfully"
            });
        }
        [HttpPut("{id}/Increment")]
        public async Task<IActionResult> IncrementProduct(int id, CancellationToken cancellationToken)
        {
            var userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userid is null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userid);
            if (user is null)
                return NotFound();

            var product = await _cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == id);

            if (product is null) return NotFound();

            product.Count += 1;
            await _cartrepository.CommitAsync(cancellationToken);

            return NoContent();
        }
        [HttpPut("{id}/Decrement")]
        public async Task<IActionResult> DecrementProduct(int id, CancellationToken cancellationToken)
        {
            var userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userid is null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userid);

            if (user is null)
                return NotFound();

            var product = await _cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == id);

            if (product is null) return NotFound();

            if (id <= 1)
                _cartrepository.Delete(product);
            else
                product.Count -= 1;

            await _cartrepository.CommitAsync(cancellationToken);

            return NoContent();
        }
        [HttpPut("{id}/DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userid is null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userid);

            if (user is null)
                return NotFound();

            var product = await _cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.ProductId == id);

            if (product is null) return NotFound();

            _cartrepository.Delete(product);
            await _cartrepository.CommitAsync(cancellationToken);

            return NoContent();
        }
        [HttpGet("Pay")]
        public async Task<IActionResult> Pay(CancellationToken cancellationToken)
        {
            var userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userid is null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userid);

            if (user is null)
                return NotFound();

            var cart = await _cartrepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Product]);
            if (cart is null)
                return NotFound();

            Models.Order order = new()
            {
                TotalPeice = cart.Sum(e => e.Price * e.Count),
                ApplicationUserId = user.Id,
            };

            await _orderRepository.AddAsync(order, cancellationToken:cancellationToken);
            await _orderRepository.CommitAsync(cancellationToken);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customers/checkout/success?orderId={order.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customers/checkout/cancel?orderId={order.Id}",
            };

            foreach (var item in cart)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                        },
                        UnitAmount = (long)item.Price * 100,
                    },
                    Quantity = item.Count,
                });
            }



            var service = new SessionService();
            var session = service.Create(options);

            order.SessionId = session.Id;
          await  _orderRepository.CommitAsync(cancellationToken);
            
            return Ok(new { paymentUrl = session.Url });
        }

    }
}
