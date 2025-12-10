namespace BookStoreAPI.Models
{
    public class ApplicationUserOTP
    {
        public int Id { get; set; }
        public string OTP { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public DateTime Validto { get; set; } = DateTime.UtcNow.AddMinutes(30);
        public bool isValid { get; set; } = true;
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
