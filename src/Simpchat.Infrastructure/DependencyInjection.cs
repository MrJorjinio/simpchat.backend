using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Simpchat.Application.Interfaces.Auth;
using Simpchat.Application.Interfaces.Email;
using Simpchat.Application.Interfaces.File;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Infrastructure.Email;
using Simpchat.Infrastructure.FileStorage;
using Simpchat.Infrastructure.Persistence;
using Simpchat.Infrastructure.Persistence.Interfaces;
using Simpchat.Infrastructure.Persistence.Repositories;
using Simpchat.Infrastructure.Security;
using Simpchat.Shared.Config;
using System.Threading.Tasks;

namespace Simpchat.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddSecurity()
                .AddPersistence(config)
                .AddFileStorage(config)
                .AddEmail();

            return services;
        }

        private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
        {
            var connectionStrings = config.GetSection("ConnectionStrings").Get<ConnectionStrings>();

            if (connectionStrings == null)
                throw new Exception("ConnectionStrings section is missing from configuration.");

            services.AddDbContext<SimpchatDbContext>(options =>
            {
                options.UseNpgsql(connectionStrings.Default);
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGlobalRoleRepository, GlobalRoleRepository>();
            services.AddScoped<IGlobalPermissionRepository, GlobalPermissionRepository>();
            services.AddScoped<IChannelRepository, ChannelRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IChatPermissionRepository, ChatPermissionRepository>();
            services.AddScoped<IChatUserPermissionRepository, ChatUserPermissionRepository>();
            services.AddScoped<IUserOtpRepository, UserOtpRepository>();
            services.AddScoped<IEmailOtpRepository, EmailOtpRepository>();
            services.AddScoped<IChatBanRepository, ChatBanRepository>();
            services.AddScoped<IReactionRepository, ReactionRepository>();
            services.AddScoped<IMessageReactionRepository, MessageReactionRepository>();
            services.AddScoped<IChannelSubscriberRepository, ChannelSubscriberRepository>();

            services.AddScoped<IDataSeeder, DataSeeder>();

            return services;
        }

        private static IServiceCollection AddSecurity(this IServiceCollection services)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            return services;
        }

        private static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration config)
        {
            var minioSettings = config.GetSection("MinioSettings").Get<MinioSettings>();

            if (minioSettings == null)
                throw new Exception("MinioSettings section is missing from configuration.");

            // Register MinioSettings as singleton
            services.AddSingleton(minioSettings);

            // Register FileStorageService
            services.AddScoped<IFileStorageService, FileStorageService>();

            // Register MinioClient
            services.AddSingleton<IMinioClient>(sp =>
            {
                var client = new MinioClient()
                    .WithEndpoint(minioSettings.Endpoint)
                    .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey);

                if (minioSettings.UseSsl)
                {
                    client = client.WithSSL();
                }

                return client.Build();
            });

            return services;
        }

        private static IServiceCollection AddEmail(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOtpService, OtpService>();

            return services;
        }
    }
}
