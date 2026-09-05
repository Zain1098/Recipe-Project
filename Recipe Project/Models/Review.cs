using System.ComponentModel.DataAnnotations;

namespace Recipe_Project.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public int UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required, MaxLength(100)]
        public string AuthorName { get; set; } = "Food Lover";

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required, MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public string? DishPhotoUrl { get; set; }

        public bool HasCookedProof => !string.IsNullOrEmpty(DishPhotoUrl);

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
