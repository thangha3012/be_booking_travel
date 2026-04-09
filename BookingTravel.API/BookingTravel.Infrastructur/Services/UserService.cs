using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Users;
using BookingTravel.Application.Interfaces;
using BookingTravel.Infrastructure.Data;
using BookingTravel.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookingTravel.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly BookingTravelDbContext _context;

        public UserService(BookingTravelDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleUserStatusAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Không cho phép khóa chính Admin
            if (user.Role == Role.Admin)
                throw new Exception("Không được phép khóa tài khoản Quản trị viên gốc.");

            user.IsActive = !user.IsActive;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
