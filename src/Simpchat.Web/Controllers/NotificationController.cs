using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Services;
using System.Security.Claims;

namespace Simpchat.Web.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _notificationService.GetAllUserNotificationsAsync(userId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("seen")]
        [Authorize]
        public async Task<IActionResult> SeenAsync(Guid notificationId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _notificationService.SetAsSeenAsync(notificationId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        [HttpPut("seen/batch")]
        [Authorize]
        public async Task<IActionResult> SeenBatchAsync([FromBody] MarkNotificationsRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _notificationService.SetMultipleAsSeenAsync(request.NotificationIds);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }
    }

    public class MarkNotificationsRequest
    {
        public List<Guid> NotificationIds { get; set; } = new();
    }
}
