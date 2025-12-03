namespace ECommerc519.API.Models
{
    public enum TransactionType
    {
        Visa,
        Cash
    }
    public enum OrderStauts
    {
        Pending,
        InProcessing,
        Shipped,
        Completed,
        Canceled
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public decimal TotalPeice { get; set; }

        public TransactionType TransactionType { get; set; } = TransactionType.Visa;
        public OrderStauts OrderStauts { get; set; } = OrderStauts.Pending;

        public string SessionId { get; set; }
        public string? CarrierId { get; set; }
        public string? TransactionId { get; set; }
        public string? CarrierName { get; set; }
        public DateTime CarrierDate { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser  { get; set; }

    }
}
