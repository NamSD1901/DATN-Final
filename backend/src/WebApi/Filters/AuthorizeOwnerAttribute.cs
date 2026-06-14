using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyPetClinic.Application.Interfaces.Repositories;

namespace MyPetClinic.WebApi.Filters
{
    public class AuthorizeOwnerAttribute : TypeFilterAttribute
    {
        public AuthorizeOwnerAttribute() : base(typeof(AuthorizeOwnerFilter))
        {
        }

        private class AuthorizeOwnerFilter : IAsyncActionFilter
        {
            private readonly IPetRepository _petRepository;

            public AuthorizeOwnerFilter(IPetRepository petRepository)
            {
                _petRepository = petRepository;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                // 1. Get current logged in user ID from Claims
                var userIdStr = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Không tìm thấy thông tin xác thực người dùng." });
                    return;
                }

                // 2. Locate the resource ID from route parameters (typically 'id')
                if (context.RouteData.Values.TryGetValue("id", out var idVal) && idVal != null)
                {
                    if (long.TryParse(idVal.ToString(), out long petId))
                    {
                        var pet = await _petRepository.GetPetByIdAsync(petId);
                        if (pet == null)
                        {
                            context.Result = new NotFoundObjectResult(new { message = "Không tìm thấy thú cưng." });
                            return;
                        }

                        // 3. Prevent IDOR: Check if current user owns the pet
                        if (pet.OwnerId != userId)
                        {
                            context.Result = new ObjectResult(new { message = "Bạn không có quyền truy cập hồ sơ thú cưng này." })
                            {
                                StatusCode = 403
                            };
                            return;
                        }
                    }
                }

                await next();
            }
        }
    }
}
