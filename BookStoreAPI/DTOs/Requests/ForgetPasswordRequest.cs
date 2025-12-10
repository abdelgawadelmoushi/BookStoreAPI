namespace BookStoreAPI.DTOs.Requests
{
    public class ForgetPasswordRequest
    {
        public int Id { get; set; }
        public string EmailOrUserName { get; set; } = string.Empty;

    }
}
