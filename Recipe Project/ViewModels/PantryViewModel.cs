using Recipe_Project.Models;

namespace Recipe_Project.ViewModels
{
    public class PantryViewModel
    {
        public List<string> SelectedIngredients { get; set; } = new List<string>();
        public List<string> PopularIngredients { get; set; } = new List<string>
        {
            "Chicken", "Eggs", "Milk", "Flour", "Rice", "Tomatoes", "Garlic", 
            "Onions", "Potatoes", "Cheese", "Butter", "Sugar", "Lemon", "Pasta", 
            "Cream", "Yogurt", "Olive Oil", "Bread", "Beef", "Berries"
        };
        public List<PantryRecipeMatch> Matches { get; set; } = new List<PantryRecipeMatch>();
    }

    public class PantryRecipeMatch
    {
        public Recipe Recipe { get; set; } = null!;
        public int MatchedCount { get; set; }
        public int TotalIngredients { get; set; }
        public int MatchPercentage => TotalIngredients > 0 ? (int)Math.Round((double)MatchedCount / TotalIngredients * 100) : 0;
        public List<string> HaveIngredients { get; set; } = new List<string>();
        public List<string> MissingIngredients { get; set; } = new List<string>();
    }
}
