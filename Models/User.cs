using System;
using System.ComponentModel.DataAnnotations;
using Task4.Enums;

namespace Task4.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsEmailVerified { get; set; } = false;
    }
}