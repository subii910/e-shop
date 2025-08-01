namespace e_shop.Models.ViewModel
{
    public class CheckoutViewModel
    {
        // Customer info (you can extend this)
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        // Display cart items
        public List<CartItem> CartItems { get; set; } = new();  // Prevents null by default

        public decimal TotalAmount =>
      (decimal)((CartItems == null || !CartItems.Any())
          ? 0
          : CartItems.Sum(i => i.product.Srice * i.quantity));


    }

}
