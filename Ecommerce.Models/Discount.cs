using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Ecommerce.Models.ShowAll;

namespace Ecommerce.Models
{
    public class Discount
    {
        public int Discountid { get; set; }
        public int Id { get; set; }
        public double Amount { get; set; }
        public DiscountType DiscountType { get; set; }
        [Display(Name = "From")]
        [Required]
        public DateTime FromDate { get; set; }
        [Display(Name = "To")]
        [Required]
        public DateTime ToDate { get; set; } 
    }

  
    
}
