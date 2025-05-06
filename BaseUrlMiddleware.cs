using ZoumaFinance.DTO;

namespace ZoumaFinance
{
    public class BaseUrlMiddleware
    {
        private readonly RequestDelegate _next;

        public BaseUrlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, BaseUrlProvider baseUrlProvider)
        {
            var request = context.Request;
            baseUrlProvider.BaseUrl = $"{request.Scheme}://{request.Host.Value}";

            await _next(context);
        }
    }
}
