namespace Ecommerce.Models
{
    public class ShowAll
    {
        public enum Roles
        {
            Admin = 1,
            SuperAdmin = 2,
            Dealer = 3
        }

        public enum Status
        {
            Pending = 0,
            Approves = 1,
            Reject = 2,
            Block = 3,
                Admin=5,
        }

        public enum DiscountType
        {
            Amount = 0,
            Percentage = 1,
        }
    }
}
