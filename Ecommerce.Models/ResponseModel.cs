namespace Ecommerce.Models
{
    public class ResponseModel
    {
        public string Message { get; set; }
        public string Status { get; set; }
        public dynamic Data { get; set; }
    }
}
