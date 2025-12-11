using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.DTOs.Requests
{
    public class ResetPasswordRequest
    {

        [Required, DataType(DataType.Password)]
        public String Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public String ConfirmPassword { get; set; } = string.Empty;
        public string UserId { get; set; }
    }
}
