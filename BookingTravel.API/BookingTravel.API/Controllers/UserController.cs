using System;
using System.Threading.Tasks;
using BookingTravel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingTravel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Quản lý danh sách người dùng (Quyền Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return SuccessResult(users, "Lấy danh sách người dùng thành công.");
        }

        // Khóa hoặc mở khóa tài khoản người dùng (Quyền Admin)
        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try 
            {
                var success = await _userService.ToggleUserStatusAsync(id);
                if (!success) return ErrorResult("Không tìm thấy người dùng.");
                return SuccessResult(null, "Cập nhật trạng thái người dùng thành công.");
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message, 400);
            }
        }
    }
}
