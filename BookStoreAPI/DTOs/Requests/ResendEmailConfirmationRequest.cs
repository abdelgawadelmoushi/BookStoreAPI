namespace BookStoreAPI.DTOs.Requests
{
    public class ResendEmailConfirmationRequest
    {
        public int Id { get; set; }
        public string EmailOrUserName { get; set; } = string.Empty;

    }
}
