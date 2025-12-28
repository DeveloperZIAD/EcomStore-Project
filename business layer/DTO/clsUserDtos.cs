// File: UserDtos.cs
namespace Business_layer.Dtos
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }           // customer, admin, guest
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterRequestDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ActivateGuestRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UpdateUserRequestDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}