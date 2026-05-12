namespace FitStack.ViewModels.Dashboard
{
    public class SocialPostViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsLiked { get; set; }
    }
}
