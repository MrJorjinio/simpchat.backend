using Simpchat.Application.Models.ApiResult;

using Simpchat.Application.Models.Users;
using Simpchat.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result<Guid>> RegisterAsync(RegisterUserDto registerUserDto);
        Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);
        Task<Result> UpdatePasswordAsync(Guid userId, UpdatePasswordDto updatePasswordDto);
        Task<Result> ResetPasswordAsync(Guid userId, ResetPasswordDto resetPasswordDto);
        Task<Result> ResetPasswordByEmailAsync(ResetPasswordByEmailDto resetPasswordByEmailDto);
    }
}
