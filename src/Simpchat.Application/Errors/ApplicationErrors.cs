using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Errors
{
    public static class ApplicationErrors
    {
        public static class User
        {
            public static readonly Error WrongPasswordOrEmail = new Error(
                "User.WrongPasswordOrEmail",
                "Given [PASSWORD] or [EMAIL] is wrong"
                );

            public static readonly Error WrongPasswordOrUsername = new Error(
                "User.WrongPasswordOrUsername",
                "Given [PASSWORD] or [USERNAME] is wrong"
                );

            public static readonly Error WrongPassword = new Error(
                "User.WrongPassword",
                "Given [PASSWORD] is wrong"
                );
               
            public static readonly Error IdNotFound = new Error(
                "User.IdNotFound",
                "User with given [ID] not found"
                );

            public static readonly Error UsernameNotFound = new Error(
                "User.UsernameNotFound",
                "User with given [USERNAME] not found"
                );

            public static readonly Error EmailNotFound = new Error(
                "User.EmailNotFound",
                "User with given [EMAIL] not found"
                );

            public static readonly Error UsernameAlreadyExists = new Error(
                "User.UsernameAlreadyExists",
                "User with given [USERNAME] already exists"
                );

            public static readonly Error EmailAlreadyExists = new Error(
                "User.UsernameAlreadyExists",
                "User with given [EMAIL] already exists"
                );

            public static readonly Error NotParticipatedInChat = new Error(
                "User.NotParticipatedInChat",
                "User not participated in [CHAT] to perform [ACTION]"
                );

            public static readonly Error CanNotDeleteAdmin = new Error(
                "User.CanNotDeleteAdmin",
                "User can't delete user with [ROLE] - Admin"
                );
        }

        public static class Otp
        {
            public static readonly Error Wrong = new Error(
                "Otp.Wrong",
                "Given [OTP] is wrong"
                );

            public static readonly Error Expired = new Error(
                "Otp.Expired",
                "Given [OTP] expired"
                );
        }

        public static class Chat
        {
            public static readonly Error IdNotFound = new Error(
                "Chat.IdNotFound",
                "Chat with given [ID] not found"
                );

            public static readonly Error NotValidChatType = new Error(
                "Chat.NotValidChatType",
                "Chat with given [TYPE] not valid"
                );
        }

        public static class ChatPermission
        {
            public static readonly Error Denied = new Error(
                "Chat.Permission.Denied",
                "User don't have [PERMISSION] to perform [ACTION]"
                );

            public static readonly Error NameNotFound = new Error(
                "Chat.Permission.NameNotFound",
                "Permission with given [NAME] not found"
                );
        }

        public static class ChatBan
        {
            public static readonly Error UserBanned = new Error(
                "Chat.Ban.UserBanned",
                "You are banned from this chat"
                );

            public static readonly Error AlreadyBanned = new Error(
                "Chat.Ban.AlreadyBanned",
                "User is already banned from this chat"
                );

            public static readonly Error CannotBanSelf = new Error(
                "Chat.Ban.CannotBanSelf",
                "You cannot ban yourself"
                );

            public static readonly Error CannotBanOwner = new Error(
                "Chat.Ban.CannotBanOwner",
                "You cannot ban the owner of this chat"
                );

            public static readonly Error NotFound = new Error(
                "Chat.Ban.NotFound",
                "Ban record not found"
                );
        }

        public static class Reaction
        {
            public static readonly Error IdNotFound = new Error(
                "Reaction.IdNotFound",
                "Reaction with given [ID] not found"
                );
        }

        public static class Message
        {
            public static readonly Error IdNotFound = new Error(
                "Message.IdNotFound",
                "Message with given [ID] not found"
                );
        }

        public static class UserReaction
        {
            public static readonly Error IdNotFound = new Error(
                "UserReaction.IdNotFound",
                "UserReaction with given [ID] not found"
                );

            public static readonly Error NotFoundWithUserIdAndReactionId = new Error(
                "UserReaction.NotFoundWithUserIdAndReactionId",
                "UserReaction with [USER_ID] and [REACTION_ID] not found"
                );
        }

        public static class Notification
        {
            public static readonly Error IdNotFound = new Error(
               "Notification.IdNotFound",
               "Notification with given [ID] not found"
               );
        }

        public static class Validation
        {
            public static readonly Error Failed = new Error(
                "Validation.Failed",
                "Validation has been failed"
                );
        }

        public static class GlobalRole
        {
            public static readonly Error NameNotFound = new Error(
                "GlobalRole.NameNotFound",
                "GlobalRole with given [NAME] not found"
                );
        }

        public static class File
        {
            public static readonly Error TooLarge = new Error(
                "File.TooLarge",
                "File size exceeds maximum allowed size of 50MB"
                );

            public static readonly Error InvalidType = new Error(
                "File.InvalidType",
                "File type is not allowed. Allowed types: images, PDF, Office documents"
                );
        }
    }
}
