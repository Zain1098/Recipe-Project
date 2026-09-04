namespace Recipe_Project.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
