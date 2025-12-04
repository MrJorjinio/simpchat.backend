using Simpchat.Application.Errors;
using Simpchat.Application.Extentions;
using Simpchat.Application.Interfaces.File;
using Simpchat.Application.Interfaces.Repositories;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Chats;
using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Users;
using Simpchat.Domain.Enums;
using Simpchat.Shared.Models;

namespace Simpchat.Application.Features
{
    internal class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConversationRepository _conversationRepo;
        private readonly IFileStorageService _fileStorageService;
        private const string BucketName = "users-avatars";

        public UserService(IUserRepository userRepo, IConversationRepository conversationRepo, IFileStorageService fileStorageService)
        {
            _userRepo = userRepo;
            _conversationRepo = conversationRepo;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<GetByIdUserDto>> GetByIdAsync(Guid userId, Guid currentUserId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure<GetByIdUserDto>(ApplicationErrors.User.IdNotFound);
            }

            var currentUser = await _userRepo.GetByIdAsync(currentUserId);

            if (currentUser is null)
            {
                return Result.Failure<GetByIdUserDto>(ApplicationErrors.User.IdNotFound);
            }

            var conversationBetweenId = await _conversationRepo.GetConversationBetweenAsync(userId, currentUserId);

            var model = new GetByIdUserDto
            {
                IsOnline = user.LastSeen.GetOnlineStatus(),
                Description = user.Description,
                AvatarUrl = user.AvatarUrl,
                ChatId = conversationBetweenId,
                LastSeen = user.LastSeen,
                UserId = userId,
                Username = user.Username
            };

            return model;
        }

        public async Task<Result<List<SearchChatResponseDto>>> SearchAsync(string term, Guid userId)
        {
            var users = await _userRepo.SearchAsync(term);

            var modeledUsers = new List<SearchChatResponseDto>();

            foreach (var user in users)
            {
                var conversationBetweenId = await _conversationRepo.GetConversationBetweenAsync(userId, user.Id);

                var model = new SearchChatResponseDto
                {
                    EntityId = user.Id,
                    DisplayName = user.Username,
                    AvatarUrl = user.AvatarUrl,
                    ChatType = ChatTypes.Conversation,
                    ChatId = conversationBetweenId
                };

                modeledUsers.Add(model);
            }

            return modeledUsers;
        }

        public async Task<Result> SetLastSeenAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            user.LastSeen = DateTimeOffset.UtcNow;

            await _userRepo.UpdateAsync(user);

            return Result.Success();
        }

        public async Task<Result> UpdateAsync(Guid userId, UpdateUserDto updateUserDto, UploadFileRequest? avatar)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            var existingUsers = await _userRepo.SearchAsync(updateUserDto.Username);
            if (existingUsers != null && existingUsers.Any(u => u.Id != userId))
            {
                return Result.Failure(ApplicationErrors.User.UsernameNotFound);
            }

            if (avatar is not null)
            {
                if (avatar.FileName != null && avatar.Content != null && avatar.ContentType != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                    user.AvatarUrl = await _fileStorageService.UploadFileAsync(BucketName, uniqueFileName, avatar.Content, avatar.ContentType);
                }
            }

            user.Username = updateUserDto.Username;
            user.Description = updateUserDto.Description;
            user.HwoCanAddType = updateUserDto.AddChatMinLvl;

            await _userRepo.UpdateAsync(user);

            return Result.Success();
        }

        public async Task<Result<List<GetAllUserDto>>> GetAllAsync()
        {
            var users = await _userRepo.GetAllAsync();

            var modeledUsers = users
                .Where(u => u.Role.Name == GlobalRoleTypes.User.ToString())
                .Select(u => new GetAllUserDto
                {
                    Username = u.Username,
                    Description = u.Description,
                    Id = u.Id,
                    AvatarUrl = u.AvatarUrl,
                    Email = u.Email,
                    IsOnline = u.LastSeen.GetOnlineStatus(),
                    LastSeen = u.LastSeen,
                }).ToList();

            return modeledUsers;
        }

        public async Task<Result> DeleteAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
            {
                return Result.Failure(ApplicationErrors.User.IdNotFound);
            }

            if (user.Role.Name == GlobalRoleTypes.Admin.ToString())
            {
                return Result.Failure(ApplicationErrors.User.CanNotDeleteAdmin);
            }

            await _userRepo.DeleteAsync(user);

            return Result.Success();
        }
    }
}
