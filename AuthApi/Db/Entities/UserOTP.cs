using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Db.Entities
{
    public class UserOTP
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; }
        
        [MaxLength(250)]
        public string OTPHash { get; set; }

        [MaxLength(250)]
        public string Salt { get; set; }

        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; } = false;

    }
}
