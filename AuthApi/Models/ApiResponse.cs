using AuthApi.Services.Enums;

namespace AuthApi.Models
{
    public class ApiResponse
    {
        public ResponseCode _ResponseCode { get; set; } = ResponseCode.OK;
        public string Message { get; set; } = string.Empty;
        public object Data { get; set; }
        public object AdditionalData { get; set; }
    }
}
