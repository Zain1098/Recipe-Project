using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Project.Data;
using Recipe_Project.Models;
using Recipe_Project.ViewModels;

namespace Recipe_Project.Controllers
{
    public class RecipesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RecipesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Recipes
        public async Task<IActionResult> Index(string? search, string? category, string? difficulty, string? sort)
        {
            var query = _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Reviews)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(r => r.Title.ToLower().Contains(term) 
                                      || r.Description.ToLower().Contains(term)
                                      || r.Cuisine.ToLower().Contains(term)
                                      || r.IngredientsJson.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                query = query.Where(r => r.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(difficulty) && difficulty != "All")
            {
                query = query.Where(r => r.Difficulty.ToLower() == difficulty.ToLower());
            }

            // Sorting
            query = sort switch
            {
                "popular" => query.OrderByDescending(r => r.ViewsCount),
                "rating" => query.OrderByDescending(r => r.Reviews.Any() ? r.Reviews.Average(rev => rev.Rating) : 0),
                "oldest" => query.OrderBy(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt) // "newest" default
            };

            var recipes = await query.ToListAsync();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category ?? "All";
            ViewBag.CurrentDifficulty = difficulty ?? "All";
            ViewBag.CurrentSort = sort ?? "newest";

            ViewBag.AllCategories = await _context.Recipes
                .Select(r => r.Category)
                .Distinct()
                .ToListAsync();

            return View(recipes);
        }

        // GET: /Recipes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Reviews)
                .ThenInclude(rev => rev.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Increment views
            recipe.ViewsCount++;
            await _context.SaveChangesAsync();

            // Check if current user saved this recipe
            var isSaved = false;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = GetCurrentUserId();
                isSaved = await _context.Favorites.AnyAsync(f => f.RecipeId == id && f.UserId == userId);
            }
            ViewBag.IsSaved = isSaved;

            // Related recipes in same category
            ViewBag.RelatedRecipes = await _context.Recipes
                .Where(r => r.Category == recipe.Category && r.Id != recipe.Id)
                .Take(3)
                .ToListAsync();

            return View(recipe);
        }

        // GET: /Recipes/Create
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new RecipeCreateViewModel();
            return View(model);
        }

        // POST: /Recipes/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecipeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();
            var imageUrl = "/images/foodmenu/menu/1.jpg";

            // Handle image upload if user uploaded a file
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                imageUrl = "/uploads/" + uniqueFileName;
            }
            else
            {
                // Assign a thematic default image based on category
                imageUrl = model.Category.ToLower() switch
                {
                    "breakfast" => "/images/foodmenu/menu/1.jpg",
                    "lunch" => "/images/foodmenu/menu12/food1.jpg",
                    "dinner" => "/images/foodmenu/menu12/food3.jpg",
                    "dessert" => "/images/foodmenu/menu/5.jpg",
                    "drinks" => "/images/foodmenu/menu/4.jpg",
                    _ => "/images/foodmenu/menu12/food2.jpg"
                };
            }

            // Parse raw line-by-line ingredients and instructions
            var ingredientsList = model.IngredientsRaw
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var instructionsList = model.InstructionsRaw
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var recipe = new Recipe
            {
                Title = model.Title,
                Description = model.Description,
                Category = model.Category,
                Cuisine = string.IsNullOrWhiteSpace(model.Cuisine) ? "Continental" : model.Cuisine,
                PrepTimeMinutes = model.PrepTimeMinutes,
                CookTimeMinutes = model.CookTimeMinutes,
                Servings = model.Servings,
                Difficulty = model.Difficulty,
                ImageUrl = imageUrl,
                Ingredients = ingredientsList,
                Instructions = instructionsList,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🎉 Recipe created successfully!";
            return RedirectToAction(nameof(Details), new { id = recipe.Id });
        }

        // GET: /Recipes/Edit/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");
            if (recipe.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

            var model = new RecipeCreateViewModel
            {
                Title = recipe.Title,
                Description = recipe.Description,
                Category = recipe.Category,
                Cuisine = recipe.Cuisine,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes,
                Servings = recipe.Servings,
                Difficulty = recipe.Difficulty,
                ExistingImageUrl = recipe.ImageUrl,
                IngredientsRaw = string.Join(Environment.NewLine, recipe.Ingredients),
                InstructionsRaw = string.Join(Environment.NewLine, recipe.Instructions)
            };

            ViewBag.RecipeId = id;
            return View(model);
        }

        // POST: /Recipes/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecipeCreateViewModel model)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");
            if (recipe.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RecipeId = id;
                return View(model);
            }

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                recipe.ImageUrl = "/uploads/" + uniqueFileName;
            }

            recipe.Title = model.Title;
            recipe.Description = model.Description;
            recipe.Category = model.Category;
            recipe.Cuisine = model.Cuisine;
            recipe.PrepTimeMinutes = model.PrepTimeMinutes;
            recipe.CookTimeMinutes = model.CookTimeMinutes;
            recipe.Servings = model.Servings;
            recipe.Difficulty = model.Difficulty;

            recipe.Ingredients = model.IngredientsRaw
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            recipe.Instructions = model.InstructionsRaw
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Recipe updated successfully!";
            return RedirectToAction(nameof(Details), new { id = recipe.Id });
        }

        // POST: /Recipes/Delete/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");
            if (recipe.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Recipe deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Recipes/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int recipeId, string? authorName, int rating, string comment, IFormFile? dishPhoto)
        {
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                return NotFound();
            }

            int? userId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                userId = GetCurrentUserId();
                if (string.IsNullOrWhiteSpace(authorName))
                {
                    authorName = User.Identity.Name ?? "Registered User";
                }
            }

            if (string.IsNullOrWhiteSpace(authorName))
            {
                authorName = "Food Lover";
            }

            if (rating < 1 || rating > 5)
            {
                rating = 5;
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = recipeId });
            }

            string? dishPhotoUrl = null;
            if (dishPhoto != null && dishPhoto.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(dishPhoto.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(ext) && dishPhoto.Length <= 5 * 1024 * 1024)
                {
                    var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "reviews");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await dishPhoto.CopyToAsync(fileStream);
                    }

                    dishPhotoUrl = $"/uploads/reviews/{uniqueFileName}";
                }
            }

            var review = new Review
            {
                RecipeId = recipeId,
                UserId = userId ?? 1,
                AuthorName = authorName,
                Rating = rating,
                Comment = comment.Trim(),
                DishPhotoUrl = dishPhotoUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = dishPhotoUrl != null 
                ? "Awesome! Your photo & review have been published to the community showcase!" 
                : "Thank you! Your review has been published.";
            return RedirectToAction(nameof(Details), new { id = recipeId });
        }

        // POST: /Recipes/ToggleFavorite
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int recipeId)
        {
            var userId = GetCurrentUserId();
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.RecipeId == recipeId && f.UserId == userId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isSaved = false, message = "Removed from Saved Recipes" });
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    RecipeId = recipeId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return Json(new { success = true, isSaved = true, message = "Saved to My Recipes!" });
            }
        }

        // GET: /Recipes/MySaved
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MySaved()
        {
            var userId = GetCurrentUserId();
            var savedRecipes = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Recipe)
                .ThenInclude(r => r!.Reviews)
                .Select(f => f.Recipe!)
                .ToListAsync();

            ViewBag.PageHeading = "My Saved Recipes";
            return View("Index", savedRecipes);
        }

        // GET: /Recipes/MyRecipes
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyRecipes()
        {
            var userId = GetCurrentUserId();
            var myRecipes = await _context.Recipes
                .Where(r => r.UserId == userId)
                .Include(r => r.Reviews)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.PageHeading = "My Created Recipes";
            return View("Index", myRecipes);
        }

        // GET: /Recipes/Pantry
        [HttpGet]
        public async Task<IActionResult> Pantry(string? ingredients)
        {
            var model = new PantryViewModel();
            var allRecipes = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Reviews)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(ingredients))
            {
                var inputList = ingredients.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim().ToLower())
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .Distinct()
                    .ToList();

                model.SelectedIngredients = inputList;

                var matches = new List<PantryRecipeMatch>();

                foreach (var r in allRecipes)
                {
                    var recipeIngs = r.Ingredients;
                    if (!recipeIngs.Any()) continue;

                    var have = new List<string>();
                    var missing = new List<string>();

                    foreach (var ring in recipeIngs)
                    {
                        var lowerRing = ring.ToLower();
                        // Check if any of the user's input ingredients is in this ingredient
                        var matched = inputList.Any(userInput => 
                            lowerRing.Contains(userInput) || userInput.Contains(lowerRing));

                        if (matched)
                        {
                            have.Add(ring);
                        }
                        else
                        {
                            missing.Add(ring);
                        }
                    }

                    if (have.Any())
                    {
                        matches.Add(new PantryRecipeMatch
                        {
                            Recipe = r,
                            MatchedCount = have.Count,
                            TotalIngredients = recipeIngs.Count,
                            HaveIngredients = have,
                            MissingIngredients = missing
                        });
                    }
                }

                model.Matches = matches
                    .OrderByDescending(m => m.MatchPercentage)
                    .ThenByDescending(m => m.MatchedCount)
                    .ToList();
            }

            return View(model);
        }

        // GET: /Recipes/ShoppingList
        [HttpGet]
        public IActionResult ShoppingList()
        {
            return View();
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 1;
        }
    }
}
