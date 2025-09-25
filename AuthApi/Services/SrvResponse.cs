using AuthApi.Services.Enums;

namespace AuthApi.Services
{
    public class SrvResponse
    {
        public bool IsOk
        {
            get
            {
                return _ResponseCode == ResponseCode.OK;
            }
        }
        public SrvResponse() { }
        public SrvResponse(object data)
        {
            Data = data;
        }
        public SrvResponse(string message)
        {
            _ResponseCode = ResponseCode.InternalServerError;
            Message = message;
        }
        public SrvResponse(ResponseCode httpStatusCode, string message)
        {
            _ResponseCode = httpStatusCode;
            Message = message;
        }
        public SrvResponse(ResponseCode httpStatusCode, string message, string propertyName)
        {
            _ResponseCode = httpStatusCode;
            Message = message;
            PropertyName = propertyName;
        }
        public ResponseCode _ResponseCode { get; set; } = ResponseCode.OK;
        public string Message { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Actions { get; set; } = string.Empty;
        public object Data { get; set; }
        public object AdditionalData { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;

        public object innerObject { get; set; } = null;
    }

    public static class SrvResponseExt
    {
        public static SrvResponse Success(this SrvResponse response)
        {
            return response ?? new SrvResponse();
        }
        public static SrvResponse Success(this SrvResponse response, object data)
        {
            response = response ?? new SrvResponse();
            response.Data = data;
            return response;
        }
        public static SrvResponse Success(this SrvResponse response, object data, object AdditionalData)
        {
            response = response ?? new SrvResponse();
            response.Data = data;
            response.AdditionalData = AdditionalData;
            return response;
        }
        public static SrvResponse Error(this SrvResponse response, ResponseCode Code, string message)
        {
            response = response ?? new SrvResponse();
            response._ResponseCode = Code;
            response.Message = message;
            return response;
        }
        public static SrvResponse Error(this SrvResponse response, string message, object innerObject = null)
        {
            response = response ?? new SrvResponse();
            response._ResponseCode = ResponseCode.InternalServerError;
            response.Message = message;
            response.innerObject = innerObject;
            return response;
        }

        public static SrvResponse BadRequst(this SrvResponse response, string message)
        {
            response = response ?? new SrvResponse();
            response._ResponseCode = ResponseCode.BadRequest;
            response.Message = message;
            return response;
        }
        public static SrvResponse _Return(this SrvResponse response, ResponseCode httpStatusCode, string message, string propertyName = "")
        {
            response = new SrvResponse(httpStatusCode, message, propertyName);
            return response;
        }
        public static bool IsOk(this SrvResponse response)
        {
            return response._ResponseCode == ResponseCode.OK;
        }

        //-----------EX Methods--------------------------------------

        public static SrvResponse GetSrvResponse(this Exception ex)
        {
            var Rx_Message = ex.Message;
            if (ex.InnerException != null) Rx_Message += " | " + ex.InnerException.Message;
            return new SrvResponse().Error(Rx_Message);
        }


        //----------------------------------------

        /// <summary>
        /// Get Data If Response is Success
        /// The expected data should be a single item, not a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static T? GetItem<T>(this SrvResponse response) where T : new()
        {
            try
            {
                if (response.IsOk)
                {
                    return (T)response.Data;
                }
                return default;
            }
            catch (Exception ex)
            {
                return default;
            }

        }
        public static T? GetItem<T>(this SrvResponse response, out string errorMesage) where T : new()
        {
            try
            {
                errorMesage = "";
                if (response.IsOk)
                {
                    return (T)response.Data;
                }
                errorMesage = response.Message;
                return default;
            }
            catch (Exception ex)
            {
                errorMesage = ex.Message;
                return default;
            }

        }
        /// <summary>
        /// Get Data If Response is Success
        /// The expected data should be a list, not a single item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static List<T> GetItems<T>(this SrvResponse response) where T : new()
        {
            try
            {

                if (response.IsOk)
                {
                    return (List<T>)response.Data;
                }
                return default;
            }
            catch (Exception ex)
            {
                return default;
            }
        }
        public static List<T> GetItems<T>(this SrvResponse response, out string errorMesage) where T : new()
        {
            try
            {
                errorMesage = "";
                if (response.IsOk)
                {
                    return (List<T>)response.Data;
                }
                errorMesage = response.Message;
                return default;
            }
            catch (Exception ex)
            {
                errorMesage = ex.Message;
                return default;
            }
        }




        public static int GetItemsCount(this SrvResponse response)
        {
            try
            {
                if (response.IsOk)
                {
                    return (int)response.AdditionalData;
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
