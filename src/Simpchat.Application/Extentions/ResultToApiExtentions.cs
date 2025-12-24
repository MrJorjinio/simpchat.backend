using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Simpchat.Application.Models.ApiResult;
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Extentions
{
    public static class ResultToApiResultExtensions
    {
        public static Models.ApiResult.ApiResult<object?> ToApiResult(this Result result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            return new Models.ApiResult.ApiResult<object?>
            {
                Success = result.IsSuccess,
                StatusCode = MapToStatusCode(result.Error, result.ValidationErrors),
                Data = result.IsSuccess ? null : null,
                Error = result.IsSuccess ? null : new ApiError(result.Error.Code, result.Error.Message),
                ValidationErrors = result.ValidationErrors != null
                    ? new Dictionary<string, string[]>(result.ValidationErrors)
                    : null
            };
        }

        public static Models.ApiResult.ApiResult<TValue?> ToApiResult<TValue>(this Result<TValue> result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            return new Models.ApiResult.ApiResult<TValue?>
            {
                Success = result.IsSuccess,
                StatusCode = MapToStatusCode(result.Error, result.ValidationErrors),
                Data = result.IsSuccess ? result.Value : default,
                Error = result.IsSuccess ? null : new ApiError(result.Error.Code, result.Error.Message),
                ValidationErrors = result.ValidationErrors != null
                    ? new Dictionary<string, string[]>(result.ValidationErrors)
                    : null
            };
        }

        private static int MapToStatusCode(Error? error, IReadOnlyDictionary<string, string[]>? validationErrors)
        {
            // Validation errors 
            if (validationErrors != null && validationErrors.Any())
                return (int)HttpStatusCode.BadRequest;

            // Null error object
            if (error == null) return (int)HttpStatusCode.InternalServerError;

            return error.Code switch
            {
                // Authentication
                "User.WrongPasswordOrEmail" or
                "User.WrongPasswordOrUsername" or
                "User.WrongPassword" or
                "Otp.Wrong" => (int)HttpStatusCode.Unauthorized,

                // Resources not found
                "User.IdNotFound" or
                "User.UsernameNotFound" or
                "User.EmailNotFound" or
                "Chat.IdNotFound" or
                "Reaction.IdNotFound" or
                "Message.IdNotFound" or
                "UserReaction.IdNotFound" or
                "UserReaction.NotFoundWithUserIdAndReactionId" or
                "Notification.IdNotFound" or
                "GlobalRole.NameNotFound" or
                "Chat.Permission.NameNotFound" or
                "Chat.Ban.NotFound" or
                "User.Ban.NotFound" or
                "Message.Pinning.NotPinned" => (int)HttpStatusCode.NotFound,

                // Conflicts
                "User.UsernameAlreadyExists" or
                "User.EmailAlreadyExists" or
                "User.AlreadyMember" or
                "Chat.Ban.AlreadyBanned" or
                "User.Ban.AlreadyBanned" or
                "Message.Pinning.AlreadyPinned" => (int)HttpStatusCode.Conflict,

                // Forbidden actions
                "User.NotParticipatedInChat" or
                "User.CanNotDeleteAdmin" or
                "Chat.Permission.Denied" or
                "Chat.Ban.UserBanned" or
                "Chat.Ban.CannotBanOwner" or
                "User.Ban.UserBanned" or
                "User.Ban.CannotMessageBannedUser" => (int)HttpStatusCode.Forbidden,

                "Chat.NotValidChatType" or
                "Chat.Ban.CannotBanSelf" or
                "User.Ban.CannotBanSelf" or
                "Message.Pinning.PinLimitReached" or
                "File.TooLarge" or
                "File.InvalidType" => (int)HttpStatusCode.BadRequest,

                "Otp.Expired" => (int)HttpStatusCode.Gone,

                "Validation.Failed" => 422,

                "Error.NullValue" => (int)HttpStatusCode.NotFound,
                "" or null => (int)HttpStatusCode.OK,
                _ => (int)HttpStatusCode.BadRequest
            };
        }
    }
}
