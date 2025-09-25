namespace AuthApi.Services.Enums
{
    public enum ResponseCode
    {
        OK = 200,
        error = 300,
        NotFound = 404,
        BadRequest = 400,
        Unauthorized = 401,
        Conflict = 409,
        InternalServerError = 500,
        ServiceUnavailable = 503,
        //LoginFaild = 1201,
        //emailNotConfirmed = 1202,
        //UserIsInActive = 1203,
    }
}
