using System;

namespace SieveApp.Models
{
    public class UserRequest
    {
        public int Id { get; set; } 
        public int UserId { get; set; } 
        public string RequestData { get; set; } = string.Empty; 
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; 

        public User User { get; set; } = null!;
    }
}
















