using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Simpchat.Application.Features;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Messages;
using Simpchat.Application.Models.Reactions;
using Simpchat.Application.Models.Users;
using Simpchat.Application.Validators;

namespace Simpchat.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services
                .AddServices()
                .AddValidation();

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IChannelService, ChannelService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IReactionService, ReactionService>();
            services.AddScoped<IMessageReactionService, MessageReactionService>();
            services.AddScoped<IChatBanService, ChatBanService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddSingleton<IPresenceService, PresenceService>(); // SINGLETON for in-memory state

            return services;
        }

        private static IServiceCollection AddValidation(this IServiceCollection services)
        {
            services.AddScoped<IValidator<RegisterUserDto>, RegisterUserValidator>();
            services.AddScoped<IValidator<LoginUserDto>, LoginUserValidator>();
            services.AddScoped<IValidator<PostChatDto>, PostChatValidator>();
            services.AddScoped<IValidator<PostMessageDto>, PostMessageValidator>();
            services.AddScoped<IValidator<UpdateChatDto>,UpdateChatValidator >();
            services.AddScoped<IValidator<UpdateUserDto>, UpdateUserInfoValidator>();
            services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordValidator>();
            services.AddScoped<IValidator<UpdatePasswordDto>, UpdatePasswordValidator>();
            services.AddScoped<IValidator<PostReactionDto>, PostReactionValidator>();
            services.AddScoped<IValidator<UpdateReactionDto>, UpdateReactionValidator>();
            services.AddScoped<IValidator<UpdateMessageDto>, UpdateMessageValidator>();

            return services;
        }
    }
}
