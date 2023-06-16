using System.ComponentModel.DataAnnotations;

namespace AspForum.Models.Account
{
    public class ChangePasswordFormViewModel
    {
        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;

        [Required]
        public string NewPasswordRepeat { get; set; } = null!;
    }
}
