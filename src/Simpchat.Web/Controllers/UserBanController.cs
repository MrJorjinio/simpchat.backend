using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.Services;
using System.Security.Claims;

namespace Simpchat.Web.Controllers
{
    [Route("api/users/blocks")]
    [ApiController]
    [Authorize]
    public class UserBanController : ControllerBase
    {
        private readonly IUserBanService _userBanService;
        private readonly IRealTimeNotificationService _realTimeNotificationService;

        public UserBanController(
            IUserBanService userBanService,
            IRealTimeNotificationService realTimeNotificationService)
        {
            _userBanService = userBanService;
            _realTimeNotificationService = realTimeNotificationService;
        }

        /// <summary>
        /// Block a user. The blocked user will not be able to send messages or start conversations with you.
        /// </summary>
        [HttpPost("{userId}")]
        public async Task<IActionResult> BlockUserAsync(Guid userId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _userBanService.BlockUserAsync(userId, requesterId);

            if (response.IsSuccess)
            {
                await _realTimeNotificationService.NotifyUserBlockedAsync(requesterId, userId);
            }

            var apiResponse = response.ToApiResult();
            return apiResponse.ToActionResult();
        }

        /// <summary>
        /// Unblock a user.
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> UnblockUserAsync(Guid userId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _userBanService.UnblockUserAsync(userId, requesterId);

            if (response.IsSuccess)
            {
                await _realTimeNotificationService.NotifyUserUnblockedAsync(requesterId, userId);
            }

            var apiResponse = response.ToApiResult();
            return apiResponse.ToActionResult();
        }

        /// <summary>
        /// Get all users that you have blocked.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBlockedUsersAsync()
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _userBanService.GetBlockedUsersAsync(requesterId);
            var apiResponse = response.ToApiResult();

            return apiResponse.ToActionResult();
        }

        /// <summary>
        /// Check if you have blocked a specific user.
        /// </summary>
        [HttpGet("{userId}/status")]
        public async Task<IActionResult> GetBlockStatusAsync(Guid userId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var isBlocked = await _userBanService.IsUserBlockedAsync(requesterId, userId);

            return Ok(new { isBlocked });
        }

        /// <summary>
        /// Check mutual block status between you and another user.
        /// Returns both whether you blocked them and whether they blocked you.
        /// </summary>
        [HttpGet("{userId}/mutual-status")]
        public async Task<IActionResult> GetMutualBlockStatusAsync(Guid userId)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var iBlockedThem = await _userBanService.IsUserBlockedAsync(requesterId, userId);
            var theyBlockedMe = await _userBanService.IsUserBlockedAsync(userId, requesterId);

            return Ok(new { iBlockedThem, theyBlockedMe });
        }
    }
}
