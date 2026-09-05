using System.ComponentModel.DataAnnotations;

namespace Recipe_Project.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User"; // "User" or "Admin"

        public string? AvatarUrl { get; set; } = "/images/author/user.png";

        public bool IsVerifiedChef { get; set; } = false;

        [MaxLength(100)]
        public string? ChefTitle { get; set; } // e.g. "Executive Chef", "Master Culinary Specialist"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
