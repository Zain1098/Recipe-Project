using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Recipe_Project.ViewModels
{
    public class RecipeCreateViewModel
    {
        [Required(ErrorMessage = "Recipe title is required")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        [Display(Name = "Recipe Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category")]
        public string Category { get; set; } = "Dinner";

        [Display(Name = "Cuisine (e.g. Italian, Pakistani, Mexican)")]
        public string Cuisine { get; set; } = "Continental";

        [Required, Range(1, 1440, ErrorMessage = "Prep time must be between 1 and 1440 minutes")]
        [Display(Name = "Prep Time (minutes)")]
        public int PrepTimeMinutes { get; set; } = 15;

        [Required, Range(0, 1440, ErrorMessage = "Cook time must be between 0 and 1440 minutes")]
        [Display(Name = "Cook Time (minutes)")]
        public int CookTimeMinutes { get; set; } = 30;

        [Required, Range(1, 100, ErrorMessage = "Servings must be at least 1")]
        public int Servings { get; set; } = 4;

        public string Difficulty { get; set; } = "Easy";

        [Display(Name = "Upload Recipe Photo (Optional)")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Required(ErrorMessage = "Please enter at least one ingredient (one per line)")]
        [Display(Name = "Ingredients (One per line)")]
        public string IngredientsRaw { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter cooking steps/instructions (one per line)")]
        [Display(Name = "Cooking Instructions (One step per line)")]
        public string InstructionsRaw { get; set; } = string.Empty;
    }
}
