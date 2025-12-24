using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Files;
using Simpchat.Application.Models.Messages;
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IMessageService
    {
        Task<Result<Guid>> SendMessageAsync(PostMessageDto postMessageDto, UploadFileRequest? uploadFileRequest);
        Task<Result> UpdateAsync(Guid messageId, UpdateMessageDto updateMessageDto, UploadFileRequest? uploadFileRequest, Guid userId);
        Task<Result> DeleteAsync(Guid messageId, Guid userId);
        Task<Result> PinMessageAsync(Guid messageId, Guid userId);
        Task<Result> UnpinMessageAsync(Guid messageId, Guid userId);
        Task<Result<List<PinnedMessageDto>>> GetPinnedMessagesAsync(Guid chatId, Guid userId);
        Task<Result<List<Guid>>> MarkMessagesAsSeenAsync(Guid chatId, Guid userId);
    }
}
