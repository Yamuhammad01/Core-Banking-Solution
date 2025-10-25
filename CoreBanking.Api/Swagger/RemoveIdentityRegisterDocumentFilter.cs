using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoreBanking.Api.Swagger
{

    public class RemoveIdentityRegisterDocumentFilter : IDocumentFilter
    {
        // pathToRemove MUST match the final swagger path case-sensitively

        private static readonly string[] PathsToRemove =
        {
                 "api/auth/register",
                  "api/auth/login",
                  "api/auth/forgotpassword",
                  "api/auth/resetpassword",
                  "api/auth/confirmEmail",
                  "api/auth/resendconfirmationEmail"
        };

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var path in PathsToRemove)
            {
                var keyToRemove = swaggerDoc.Paths.Keys.FirstOrDefault(k =>
                    k.Contains(path, StringComparison.OrdinalIgnoreCase));
                if (keyToRemove != null)
                    swaggerDoc.Paths.Remove(keyToRemove);
            }
        }
    }
}
