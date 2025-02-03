namespace SieveApp.Models
{
    public class User
    {
        public int Id { get; set; } 
        public string Username { get; set; } = string.Empty; 
        public string PasswordHash { get; set; } = string.Empty; 
        public string? Token { get; set; } = null; // Токен может быть null

        
        public ICollection<UserRequest> Requests { get; set; } = new List<UserRequest>();

        public ICollection<History> Histories { get; set; } = new List<History>();
    }
}












