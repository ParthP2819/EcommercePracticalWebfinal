using System.ComponentModel.DataAnnotations;
using static Ecommerce.Models.ShowAll;

namespace Ecommerce.Models
{
    public class Dealer
    {
        public string UserName { get; set; }
        public string SecurityStamp { get; set; }
        [Key]
        public string Email { get; set; }
        public string Phone { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string? Country { get; set; }
        public string PostalCode { get; set; }
        public string Password { get; set; }
        public Status status { get; set; }
        public string? Reason { get; set; }
    }
}
 