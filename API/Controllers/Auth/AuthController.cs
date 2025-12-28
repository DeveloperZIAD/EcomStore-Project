using Microsoft.AspNetCore.Mvc;
using Business_layer;
using Business_layer.Dtos.Auth;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Admin login only
        /// </summary>
        [HttpPost("login")]
        public ActionResult<AuthResponseDto> Login(LoginRequestDto dto)
        {
            try
            {
                var user = UserService.Login(dto);

                // تأكد إنه admin
                if (user.Role != "admin")
                    return Unauthorized( "Access denied. Admin only." );

                var token = AuthService.GenerateToken(user.Id, user.Email, user.Role);
                return Ok(token);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}