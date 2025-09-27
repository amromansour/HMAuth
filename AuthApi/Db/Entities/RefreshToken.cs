using System.ComponentModel.DataAnnotations;

namespace AuthApi.Db.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpireIn { get; set; }
    }
}
