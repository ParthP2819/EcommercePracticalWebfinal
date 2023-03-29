using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Dealer
    {
        public string UserName { get; set; }
        public string SecurityStamp { get; set; }
        [Key]
        public string Email { get; set; }
        public int Phone { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string? Country { get; set; }
        public string PostalCode { get; set; }
        public string Password { get; set; }
        public ShowAll.Status status { get; set; }
        public string? Reason { get; set; }
    }
}
 