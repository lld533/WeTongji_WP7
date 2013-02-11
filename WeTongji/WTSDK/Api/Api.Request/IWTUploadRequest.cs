using System;
using System.Collections.Generic;

namespace WeTongji.Api.Request
{
    public interface IWTUploadRequest<T> : IWTRequest<T> where T : WeTongji.Api.WTResponse
    {
        KeyValuePair<String, WeTongji.Api.Util.FileItem> GetFileParameter();
    }
}
