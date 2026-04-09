using System.Collections.Generic;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Users;

namespace BookingTravel.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> ToggleUserStatusAsync(int id);
    }
}
