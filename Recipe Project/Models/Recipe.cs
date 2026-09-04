using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Recipe_Project.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Category { get; set; } = "Dinner"; // Breakfast, Lunch, Dinner, Dessert, Drinks, Fast Food

        [MaxLength(50)]
        public string Cuisine { get; set; } = "Continental"; // Italian, Pakistani, American, Asian, etc.

        [Range(1, 1440)]
        public int PrepTimeMinutes { get; set; } = 15;

        [Range(1, 1440)]
        public int CookTimeMinutes { get; set; } = 30;

        [Range(1, 100)]
        public int Servings { get; set; } = 4;

        [MaxLength(20)]
        public string Difficulty { get; set; } = "Easy"; // Easy, Medium, Hard

        public string ImageUrl { get; set; } = "/images/foodmenu/menu/1.jpg";

        // JSON stored ingredients list
        public string IngredientsJson { get; set; } = "[]";

        // JSON stored instructions list
        public string InstructionsJson { get; set; } = "[]";

        public int UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ViewsCount { get; set; } = 0;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

        [NotMapped]
        public List<string> Ingredients
        {
            get => string.IsNullOrEmpty(IngredientsJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(IngredientsJson) ?? new List<string>();
            set => IngredientsJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> Instructions
        {
            get => string.IsNullOrEmpty(InstructionsJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(InstructionsJson) ?? new List<string>();
            set => InstructionsJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public double AverageRating => Reviews.Any() ? Math.Round(Reviews.Average(r => r.Rating), 1) : 5.0;

        [NotMapped]
        public int TotalReviews => Reviews.Count;
    }
}
