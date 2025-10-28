namespace Together.Domain.Entities;

public class PostImage
{
    public Guid Id { get; private set; }
    public Guid PostId { get; private set; }
    public string? ImageUrl { get; private set; }
    public int Order { get; private set; }

    // Navigation property
    public Post Post { get; private set; } = null!;

    private PostImage() { }

    public PostImage(Guid postId, string imageUrl, int order)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));

        Id = Guid.NewGuid();
        PostId = postId;
        ImageUrl = imageUrl;
        Order = order;
    }
}
