using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SPM.Http.SelfUpdateHost
{
    public class CUrlFileTransferFormatter : InputFormatter
    {
        public CUrlFileTransferFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/octet-stream"));
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(IFormFile);
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            string fileName = Path.GetFileName(context.HttpContext.Request.Path.Value);
            var file = new FormFile(context.HttpContext.Request.Body, 0, context.HttpContext.Request.ContentLength.GetValueOrDefault(), context.ModelName, fileName);

            return InputFormatterResult.SuccessAsync(file);
        }
    }
}
