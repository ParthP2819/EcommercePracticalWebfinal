using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ecommerce.Models.ShowAll;

namespace Ecommerce.Models
{
    public class Discount
    {
        public int Discountid { get; set; }
        public int Id { get; set; }
        public double Amount { get; set; }
        public DiscountType DiscountType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

  
    
}
