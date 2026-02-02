using System;
using Task4.Enums;

namespace Task4.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public DateTime RegistrationTime { get; set; }
        public bool IsSelected { get; set; }
    }
}