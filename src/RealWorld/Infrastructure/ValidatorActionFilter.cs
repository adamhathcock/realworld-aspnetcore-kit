using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace RealWorld.Infrastructure
{
    public class ValidatorActionFilter : IActionFilter
    {
        private readonly ILogger<ValidatorActionFilter> logger;

        public ValidatorActionFilter(ILogger<ValidatorActionFilter> logger)
        {
            this.logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ModelState.IsValid) return;

            var result = new ContentResult();
            var errors = new Dictionary<string, string[]>();

            foreach (var valuePair in filterContext.ModelState)
            {
                errors.Add(valuePair.Key, valuePair.Value.Errors.Select(x => x.ErrorMessage).ToArray());
            }

            var content = JsonConvert.SerializeObject(new { errors});
            result.Content = content;
            result.ContentType = "application/json";

            filterContext.HttpContext.Response.StatusCode = 422; //unprocessable entity;
            filterContext.Result = result;

            logger.Log(LogLevel.Warning, content);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}