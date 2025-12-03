using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace ECommerc519.API.Areas.Customers.Controllers
{
    [Route("[area]/[controller]")]
    [ApiController]
    //[Authorize]
    [Area("Customers")]
    public class CheckoutController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IOrderItemRepository _orderItemRepository;

        public CheckoutController(IRepository<Order> orderRepository  , IEmailSender emailSender ,IRepository<Cart> cartRepository, IOrderItemRepository orderItemRepository)
        {
           _orderRepository = orderRepository;
           _emailSender = emailSender;
           _cartRepository = cartRepository;
            _orderItemRepository = orderItemRepository;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Succussed(int orderId)
        {
            var order =await _orderRepository.GetOneAsync(e => e.Id == orderId, includes: [e => e.ApplicationUser]);

            if (order is null) return NotFound();

            // send email

           await _emailSender.SendEmailAsync(order.ApplicationUser.Email!, "Place Order Successfully" ,$"<h1>Thanks ,Place Order Successfully - {order.TotalPeice}<h1>");

            //Update Order

            order.OrderStauts = OrderStauts.InProcessing;
            var service = new SessionService();
            var transaction = service.Get(order.SessionId);

            order.TransactionId = transaction.SetupIntentId;

            //Trancfer cart => Orders Item
            var cart = await _cartRepository.GetAsync(e => e.ApplicationUserId == order.ApplicationUserId, includes: [e => e.Product]);

            List<OrderItem> items = cart.Select(e => new OrderItem
            {
                ProductId =e!.ProductId,
                Count = e.Count,
                Price = e.Price,
                OrderId = orderId
            }).ToList();

           await _orderItemRepository.AddRangeAsync(items);

            //Decrese Product Quntity in stock 

            foreach(var item in cart)
                item!.Product.Quantity -=item.Count;

            //Delete cart
            foreach(var item in cart)
                _cartRepository.Delete(item!);

          await  _cartRepository.CommitAsync();
            //Return

            return Ok(new
            {
                  msg = "Place Order Successfully"
            });
        }
        //public IActionResult Canceled()
        //{
        //    return Ok();
        //}
    }
}
