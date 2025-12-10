namespace BookStoreAPI.DTOs.Requests
{
    public class ValidateOTPRequest
    {
        public int Id { get; set; }
        public string OTP { get; set; } = string.Empty;
        public string UserId { get; set; }
    }
}
