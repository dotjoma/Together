using Together.Domain.Enums;

namespace Together.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string? Username { get; private set; }
    public string? Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public string? Bio { get; private set; }
    public ProfileVisibility Visibility { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? PartnerId { get; private set; }

    // Navigation properties
    public CoupleConnection? CoupleConnection { get; private set; }
    public ICollection<Post> Posts { get; private set; }
    public ICollection<FollowRelationship> Following { get; private set; }
    public ICollection<FollowRelationship> Followers { get; private set; }
    public ICollection<MoodEntry> MoodEntries { get; private set; }

    private User() 
    {
        Posts = new List<Post>();
        Following = new List<FollowRelationship>();
        Followers = new List<FollowRelationship>();
        MoodEntries = new List<MoodEntry>();
    }

    public User(string username, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Visibility = ProfileVisibility.Public;
        CreatedAt = DateTime.UtcNow;
        
        Posts = new List<Post>();
        Following = new List<FollowRelationship>();
        Followers = new List<FollowRelationship>();
        MoodEntries = new List<MoodEntry>();
    }

    public void UpdateProfile(string? bio, string? profilePictureUrl, ProfileVisibility visibility)
    {
        Bio = bio;
        ProfilePictureUrl = profilePictureUrl;
        Visibility = visibility;
    }

    public void SetPartner(Guid partnerId)
    {
        PartnerId = partnerId;
    }

    public void RemovePartner()
    {
        PartnerId = null;
    }
}
