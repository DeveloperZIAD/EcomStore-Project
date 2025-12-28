using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business_layer;
using Business_layer.Dtos;
using Business_layer.Dtos.Auth;
namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")] // كل الـ endpoints هنا للـ Admin فقط
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Admin adds a new user (customer or another admin)
        /// </summary>
        [HttpPost]
        public ActionResult<int> AddUser([FromBody] UpdateUserRequestDto dto) // نستخدم نفس DTO للبساطة
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // في الواقع، هنا الـ Admin بيحدد الباسوورد يدويًا
                // للبساطة، هنستخدم password افتراضي أو نعدل الـ DTO لاحقًا
                var registerDto = new Business_layer.Dtos.Auth.RegisterRequestDto
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    Password = "TempPassword123!" // الـ Admin يغيره لاحقًا أو يرسل رابط reset
                };

                int userId = UserService.RegisterUser(registerDto);

                // تحديث الـ Role لو الـ Admin عايز يعمل admin جديد
                if (dto.Role == "admin")
                {
                    UserService.UpdateUser(userId, new UpdateUserRequestDto
                    {
                        Username = dto.Username,
                        Email = dto.Email,
                        Role = "admin"
                    });
                }

                return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { id = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all users (admin only)
        /// </summary>
        [HttpGet]
        public ActionResult<List<UserResponseDto>> GetAllUsers()
        {
            var users = UserService.GetAllUsers();
            return Ok(users);
        }

        /// <summary>
        /// Gets a user by ID (admin only)
        /// </summary>
        [HttpGet("{id:int}")]
        public ActionResult<UserResponseDto> GetUserById(int id)
        {
            try
            {
                var user = UserService.GetUserById(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates user details or role (admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        public IActionResult UpdateUser(int id, UpdateUserRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                bool success = UserService.UpdateUser(id, dto);
                if (!success)
                    return NotFound(new { message = "User not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a user (admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                bool success = UserService.DeleteUser(id);
                if (!success)
                    return NotFound(new { message = "User not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}