namespace StylekAPI.Models;

public class Review
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<ReviewLike> Likes { get; set; } = new List<ReviewLike>();
}
