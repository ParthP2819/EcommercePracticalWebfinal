using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
       public string? City { get; set; }
        public string? State { get; set; }
        public string? StreetAddress { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; }
    }
    
}
