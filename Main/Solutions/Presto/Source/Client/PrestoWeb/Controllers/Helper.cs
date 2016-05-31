using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace PrestoWeb.Controllers
{
    internal static class Helper
    {
        internal static HttpResponseException CreateHttpResponseException(Exception ex, string reasonPhrase)
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ConcatenateExceptionMessages(ex)),
                ReasonPhrase = reasonPhrase
            };

            return new HttpResponseException(response);
        }

        private static string ConcatenateExceptionMessages(Exception ex)
        {
            if (ex is AggregateException)
            {
                return ConcatenateAggregateExceptionMessages(ex as AggregateException);
            }

            var stringBuilder = new StringBuilder();

            var exception = ex;
            do
            {
                stringBuilder.AppendLine(exception.Message);
                exception = exception.InnerException;
            }
            while (exception != null);

            return stringBuilder.ToString();
        }

        private static string ConcatenateAggregateExceptionMessages(AggregateException ex)
        {
            var stringBuilder = new StringBuilder();

            foreach (var inner in ex.InnerExceptions)
            {
                stringBuilder.AppendLine(inner.Message);
            }

            return stringBuilder.ToString();
        }
    }
}