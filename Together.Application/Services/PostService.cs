using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private const int MaxImages = 4;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IStorageService storageService)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _storageService = storageService;
    }

    public async Task<PostDto> CreatePostAsync(Guid authorId, CreatePostDto dto)
    {
        // Validate content
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Content), new[] { "Content cannot be empty" } }
            });
        }

        if (dto.Content.Length > 500)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Content), new[] { "Content cannot exceed 500 characters" } }
            });
        }

        // Validate images
        if (dto.ImagePaths != null && dto.ImagePaths.Count > MaxImages)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.ImagePaths), new[] { $"Cannot attach more than {MaxImages} images" } }
            });
        }

        // Validate image sizes
        if (dto.ImagePaths != null)
        {
            foreach (var imagePath in dto.ImagePaths)
            {
                if (File.Exists(imagePath))
                {
                    var fileInfo = new FileInfo(imagePath);
                    if (fileInfo.Length > MaxImageSizeBytes)
                    {
                        throw new ValidationException(new Dictionary<string, string[]>
                        {
                            { nameof(dto.ImagePaths), new[] { $"Each image must be less than 5MB" } }
                        });
                    }
                }
            }
        }

        // Get author
        var author = await _userRepository.GetByIdAsync(authorId);
        if (author == null)
        {
            throw new NotFoundException(nameof(User), authorId);
        }

        // Create post
        var post = new Post(authorId, dto.Content);

        // Upload images if provided
        if (dto.ImagePaths != null && dto.ImagePaths.Any())
        {
            var imageUrls = new List<string>();
            for (int i = 0; i < dto.ImagePaths.Count; i++)
            {
                var imagePath = dto.ImagePaths[i];
                if (File.Exists(imagePath))
                {
                    var imageUrl = await _storageService.UploadImageAsync(imagePath, $"posts/{post.Id}");
                    imageUrls.Add(imageUrl);
                    
                    // Add PostImage entity
                    var postImage = new PostImage(post.Id, imageUrl, i);
                    post.Images.Add(postImage);
                }
            }
        }

        await _postRepository.AddAsync(post);

        return MapToDto(post, author);
    }

    public async Task<PostDto> UpdatePostAsync(Guid userId, UpdatePostDto dto)
    {
        var post = await _postRepository.GetByIdAsync(dto.PostId);
        if (post == null)
        {
            throw new NotFoundException(nameof(Post), dto.PostId);
        }

        // Check if user is the author
        if (post.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You can only edit your own posts");
        }

        // Validate content
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Content), new[] { "Content cannot be empty" } }
            });
        }

        if (dto.Content.Length > 500)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Content), new[] { "Content cannot exceed 500 characters" } }
            });
        }

        // Edit will throw if outside 15-minute window
        try
        {
            post.Edit(dto.Content);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "EditWindow", new[] { ex.Message } }
            });
        }

        await _postRepository.UpdateAsync(post);

        return MapToDto(post, post.Author);
    }

    public async Task DeletePostAsync(Guid userId, Guid postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        // Check if user is the author
        if (post.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own posts");
        }

        // Delete images from storage
        if (post.Images != null && post.Images.Any())
        {
            foreach (var image in post.Images)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    await _storageService.DeleteImageAsync(image.ImageUrl);
                }
            }
        }

        await _postRepository.DeleteAsync(postId);
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            return null;
        }

        return MapToDto(post, post.Author);
    }

    public async Task<IEnumerable<PostDto>> GetUserPostsAsync(Guid userId, int skip = 0, int take = 20)
    {
        var posts = await _postRepository.GetUserPostsAsync(userId, skip, take);
        return posts.Select(p => MapToDto(p, p.Author));
    }

    public async Task<IEnumerable<PostDto>> GetFeedPostsAsync(Guid userId, int skip = 0, int take = 20)
    {
        var posts = await _postRepository.GetFeedPostsAsync(userId, skip, take);
        return posts.Select(p => MapToDto(p, p.Author));
    }

    private PostDto MapToDto(Post post, User author)
    {
        var authorDto = new UserDto(
            author.Id,
            author.Username ?? string.Empty,
            author.Email?.Value ?? string.Empty,
            author.ProfilePictureUrl,
            author.Bio
        );

        var imageUrls = post.Images?
            .OrderBy(i => i.Order)
            .Select(i => i.ImageUrl ?? string.Empty)
            .ToList() ?? new List<string>();

        return new PostDto(
            post.Id,
            authorDto,
            post.Content ?? string.Empty,
            post.CreatedAt,
            post.EditedAt,
            post.LikeCount,
            post.CommentCount,
            imageUrls
        );
    }
}
