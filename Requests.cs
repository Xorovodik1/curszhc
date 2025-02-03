namespace SieveApp.Models
{
    public class Request
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty; 
        public string Method { get; set; } = string.Empty; 
        public string? Response { get; set; } 
        public int UserId { get; set; } 
        public User User { get; set; } = null!;
    }
}


