// File: UserService.cs
using Business_layer.Dtos;
using Business_layer.Dtos.Auth; // <-- أضف ده
using Data_layer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business_layer
{
    public static class UserService
    {
        /// <summary>
        /// Registers a new regular user (customer role)
        /// </summary>
        /// 

        public static int RegisterUser(Business_layer.Dtos.Auth.RegisterRequestDto dto)
        {
            ValidateRegisterDto(dto);

            if (user_dal.EmailExists(dto.Email))
                throw new InvalidOperationException("Email already exists.");

            if (!string.IsNullOrWhiteSpace(dto.Username) && user_dal.UsernameExists(dto.Username))
                throw new InvalidOperationException("Username already exists.");

            string passwordHash = HashPassword(dto.Password);

            int newUserId = user_dal.CreateUser(
                username: dto.Username?.Trim(),
                email: dto.Email.Trim(),
                passwordHash: passwordHash,
                role: "customer"
            );

            if (newUserId <= 0)
                throw new Exception("Failed to register user.");

            AuditLogService.LogAction("User Registered", $"User ID: {newUserId}, Email: {dto.Email}");

            return newUserId;
        }

        public static UserResponseDto Login(Dtos.Auth.LoginRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Email and password are required.");

            var user = user_dal.GetUserByEmail(dto.Email.Trim());

            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            bool passwordValid = VerifyPassword(dto.Password, user.password_hash);

            if (!passwordValid)
                throw new UnauthorizedAccessException("Invalid email or password.");

            AuditLogService.LogAction("User Login", $"User ID: {user.id}, Email: {user.email}");

            return MapToResponseDto(user);
        }

        /// <summary>
        /// Retrieves all users (admin view)
        /// </summary>
        public static List<UserResponseDto> GetAllUsers()
        {
            var dbUsers = user_dal.GetAllUsers();

            return dbUsers.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Retrieves a user by ID (with optional ownership check)
        /// </summary>
        public static UserResponseDto GetUserById(int userId, int requestingUserId = 0)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID.");

            var user = user_dal.GetUserById(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // Optional: restrict non-admin from viewing others
            // if (requestingUserId > 0 && requestingUserId != userId && user.role != "admin") ...

            return MapToResponseDto(user);
        }

        /// <summary>
        /// Retrieves a user by email
        /// </summary>
        public static UserResponseDto GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            var user = user_dal.GetUserByEmail(email.Trim());

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return MapToResponseDto(user);
        }

        /// <summary>
        /// Activates a guest user by setting username, password, and changing role to customer
        /// </summary>
        public static bool ActivateGuestUser(int guestUserId, ActivateGuestRequestDto dto)
        {
            if (guestUserId <= 0) throw new ArgumentException("Invalid guest user ID.");
            ValidateActivateDto(dto);

            var guest = user_dal.GetUserById(guestUserId);

            if (guest == null)
                throw new KeyNotFoundException("Guest user not found.");

            if (guest.role != "guest")
                throw new InvalidOperationException("Only guest users can be activated.");

            if (user_dal.UsernameExists(dto.Username))
                throw new InvalidOperationException("Username already exists.");

            string passwordHash = HashPassword(dto.Password);

            bool success = user_dal.ActivateGuestUser(guestUserId, dto.Username.Trim(), passwordHash);

            if (success)
            {
                AuditLogService.LogAction("Guest User Activated", $"User ID: {guestUserId}, New Username: {dto.Username}");
            }

            return success;
        }

        /// <summary>
        /// Updates user details (admin only)
        /// </summary>
        public static bool UpdateUser(int userId, UpdateUserRequestDto dto)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID.");
            ValidateUpdateDto(dto);

            var existing = user_dal.GetUserById(userId);

            if (existing == null)
                throw new KeyNotFoundException("User not found.");

            if (existing.email != dto.Email && user_dal.EmailExists(dto.Email))
                throw new InvalidOperationException("Email already exists.");

            if (!string.IsNullOrWhiteSpace(dto.Username) &&
                existing.username != dto.Username &&
                user_dal.UsernameExists(dto.Username))
                throw new InvalidOperationException("Username already exists.");

            existing.username = string.IsNullOrWhiteSpace(dto.Username) ? existing.username : dto.Username.Trim();
            existing.email = dto.Email.Trim();
            existing.role = dto.Role;

            bool success = user_dal.UpdateUser(existing);

            if (success)
            {
                AuditLogService.LogAction("User Updated", $"User ID: {userId}, New Role: {dto.Role}");
            }

            return success;
        }

        /// <summary>
        /// Deletes a user (admin only - cascades to related data)
        /// </summary>
        public static bool DeleteUser(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID.");

            var user = user_dal.GetUserById(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (user.role == "admin")
                throw new InvalidOperationException("Cannot delete admin users.");

            bool success = user_dal.DeleteUser(userId);

            if (success)
            {
                AuditLogService.LogAction("User Deleted", $"User ID: {userId}, Email: {user.email}");
            }

            return success;
        }

        // ----------------- Private Helper Methods -----------------

        private static void ValidateRegisterDto(Business_layer.Dtos.Auth.RegisterRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required.");
            if (dto.Password.Length < 6) throw new ArgumentException("Password must be at least 6 characters.");
        }

        private static void ValidateActivateDto(ActivateGuestRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Username)) throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required.");
            if (dto.Password.Length < 6) throw new ArgumentException("Password must be at least 6 characters.");
        }

        private static void ValidateUpdateDto(UpdateUserRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email is required.");
        }

        private static string HashPassword(string password)
        {
            // TODO: Replace with real hashing (e.g., BCrypt.Net)
            return password; // Placeholder
        }

        private static bool VerifyPassword(string password, string hash)
        {
            // TODO: Replace with real verification
            return password == hash; // Placeholder
        }

        private static UserResponseDto MapToResponseDto(clsuser db)
        {
            return new UserResponseDto
            {
                Id = db.id,
                Username = db.username,
                Email = db.email,
                Role = db.role,
                CreatedAt = db.created_at
            };
        }
    }
}