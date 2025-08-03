using System.ComponentModel.DataAnnotations;

namespace e_shop.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }  // Primary key

        public string Name { get; set; }

        public string Email { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
    }

}
