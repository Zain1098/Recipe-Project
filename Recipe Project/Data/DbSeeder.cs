using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Recipe_Project.Models;
using Recipe_Project.Services;

namespace Recipe_Project.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDatabaseAsync(AppDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Lightweight SQLite column migrations for existing databases
            try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Users ADD COLUMN IsVerifiedChef INTEGER NOT NULL DEFAULT 0;"); } catch { }
            try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Users ADD COLUMN ChefTitle TEXT;"); } catch { }
            try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Reviews ADD COLUMN DishPhotoUrl TEXT;"); } catch { }

            if (context.Users.Any())
            {
                // Ensure existing admin/chef user has verified credentials updated
                var existingChef = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Users, u => u.Role == "Admin" || u.Email == "zain@recipeproject.com");
                if (existingChef != null)
                {
                    existingChef.IsVerifiedChef = true;
                    if (string.IsNullOrEmpty(existingChef.ChefTitle))
                    {
                        existingChef.ChefTitle = "Executive Chef & Culinary Director";
                    }
                    await context.SaveChangesAsync();
                }

                // Ensure at least one review has a sample photo for community showcase demo
                var sampleReview = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Reviews, r => string.IsNullOrEmpty(r.DishPhotoUrl));
                if (sampleReview != null)
                {
                    sampleReview.DishPhotoUrl = "/images/foodmenu/menu/1.jpg";
                    await context.SaveChangesAsync();
                }

                return; // Already seeded
            }

            // Seed Admin / Chef User
            var adminUser = new ApplicationUser
            {
                FullName = "Chef Zain",
                Email = "zain@recipeproject.com",
                PasswordHash = PasswordHelper.HashPassword("Password123"),
                Role = "Admin",
                IsVerifiedChef = true,
                ChefTitle = "Executive Chef & Culinary Director",
                AvatarUrl = "/images/author/author1.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var regularUser = new ApplicationUser
            {
                FullName = "Sarah Jenkins",
                Email = "sarah@example.com",
                PasswordHash = PasswordHelper.HashPassword("Password123"),
                Role = "User",
                AvatarUrl = "/images/author/author2.jpg",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(adminUser, regularUser);
            await context.SaveChangesAsync();

            var recipes = new List<Recipe>
            {
                new Recipe
                {
                    Title = "Fluffy Berry Pancakes with Maple Drizzle",
                    Description = "Golden, pillow-soft homemade buttermilk pancakes topped with fresh seasonal berries and warm pure maple syrup.",
                    Category = "Breakfast",
                    Cuisine = "American",
                    PrepTimeMinutes = 10,
                    CookTimeMinutes = 15,
                    Servings = 4,
                    Difficulty = "Easy",
                    ImageUrl = "/images/foodmenu/menu/1.jpg",
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    ViewsCount = 142,
                    IngredientsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "2 cups All-Purpose Flour",
                        "2 tsp Baking Powder",
                        "1/2 tsp Baking Soda",
                        "2 tbsp Granulated Sugar",
                        "1/2 tsp Salt",
                        "1 3/4 cups Buttermilk (or whole milk with lemon)",
                        "2 Large Eggs",
                        "4 tbsp Unsalted Butter (melted)",
                        "1 cup Fresh Blueberries & Strawberries",
                        "Pure Maple Syrup for serving"
                    }),
                    InstructionsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "Whisk together flour, sugar, baking powder, baking soda, and salt in a large mixing bowl.",
                        "In another bowl, whisk together buttermilk, eggs, and melted butter.",
                        "Pour the wet mixture into the dry ingredients and stir gently until just combined. Do not overmix; small lumps are fine.",
                        "Preheat a non-stick skillet over medium heat and lightly butter the surface.",
                        "Pour 1/4 cup of batter per pancake onto the hot skillet. Cook until bubbles form on top (about 2-3 minutes).",
                        "Flip and cook the other side for another 1-2 minutes until golden brown.",
                        "Stack warm pancakes, garnish generously with fresh berries, and drizzle with warm maple syrup."
                    })
                },
                new Recipe
                {
                    Title = "Classic Shakshuka with Poached Eggs",
                    Description = "Rich, aromatic Mediterranean tomato and roasted bell pepper sauce gently simmering with perfectly poached eggs and fresh herbs.",
                    Category = "Breakfast",
                    Cuisine = "Mediterranean",
                    PrepTimeMinutes = 15,
                    CookTimeMinutes = 20,
                    Servings = 3,
                    Difficulty = "Easy",
                    ImageUrl = "/images/foodmenu/home12/1.jpg",
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    ViewsCount = 98,
                    IngredientsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "4 Large Organic Eggs",
                        "2 tbsp Extra Virgin Olive Oil",
                        "1 Medium Yellow Onion (diced)",
                        "1 Red Bell Pepper (chopped)",
                        "3 Cloves Garlic (minced)",
                        "1 tsp Ground Cumin",
                        "1 tsp Smoked Paprika",
                        "1/4 tsp Red Chili Flakes",
                        "1 can (14 oz) Crushed Whole Tomatoes",
                        "1/2 cup Feta Cheese (crumbled)",
                        "Fresh Cilantro & Parsley for garnish",
                        "Warm crusty sourdough bread"
                    }),
                    InstructionsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "Heat olive oil in a large cast-iron skillet over medium heat.",
                        "Add diced onion and red bell pepper. Sauté for 5 minutes until soft and caramelized.",
                        "Stir in minced garlic, cumin, smoked paprika, and chili flakes. Cook for 1 minute until fragrant.",
                        "Pour in crushed tomatoes, season with salt and black pepper, and let simmer on low heat for 10 minutes until sauce thickens.",
                        "Use a spoon to create 4 small wells in the sauce. Gently crack an egg into each well.",
                        "Cover the skillet and cook on low heat for 5-8 minutes until egg whites are set but yolks remain runny.",
                        "Remove from heat, crumble fresh feta cheese on top, sprinkle with cilantro, and serve immediately with crusty sourdough bread."
                    })
                },
                new Recipe
                {
                    Title = "Creamy Tuscan Garlic Butter Pasta",
                    Description = "Velvety parmesan cream sauce tossed with penne pasta, sun-dried tomatoes, fresh baby spinach, and roasted garlic.",
                    Category = "Lunch",
                    Cuisine = "Italian",
                    PrepTimeMinutes = 10,
                    CookTimeMinutes = 20,
                    Servings = 4,
                    Difficulty = "Medium",
                    ImageUrl = "/images/foodmenu/menu12/food1.jpg",
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    ViewsCount = 215,
                    IngredientsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "350g Penne or Fettuccine Pasta",
                        "2 tbsp Unsalted Butter",
                        "4 Cloves Garlic (finely minced)",
                        "1/2 cup Sun-Dried Tomatoes (drained & sliced)",
                        "1 cup Heavy Cream",
                        "1/2 cup Vegetable or Chicken Broth",
                        "1 cup Grated Parmesan Cheese",
                        "2 cups Fresh Baby Spinach",
                        "1/2 tsp Italian Herb Seasoning",
                        "Salt and Cracked Black Pepper to taste"
                    }),
                    InstructionsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "Bring a large pot of salted water to a rolling boil and cook pasta al dente according to package instructions. Reserve 1/2 cup pasta water, then drain.",
                        "In a deep skillet, melt butter over medium heat. Sauté minced garlic and sun-dried tomatoes for 2 minutes.",
                        "Pour in the heavy cream and broth. Bring to a gentle simmer for 3 minutes.",
                        "Lower heat and whisk in parmesan cheese until smooth and creamy.",
                        "Add fresh spinach and Italian seasoning. Stir until the spinach wilts into the sauce.",
                        "Toss cooked pasta into the sauce, adding a splash of reserved pasta water if needed for a glossy consistency.",
                        "Serve hot garnished with extra parmesan and fresh basil leaves."
                    })
                },
                new Recipe
                {
                    Title = "Grilled Lemon Herb Chicken Breast",
                    Description = "Juicy chicken breasts marinated in fresh lemon juice, rosemary, oregano, and garlic, grilled to smoky perfection.",
                    Category = "Lunch",
                    Cuisine = "Continental",
                    PrepTimeMinutes = 20,
                    CookTimeMinutes = 15,
                    Servings = 4,
                    Difficulty = "Easy",
                    ImageUrl = "/images/foodmenu/menu12/food2.jpg",
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    ViewsCount = 180,
                    IngredientsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "4 Boneless Skinless Chicken Breasts",
                        "1/4 cup Extra Virgin Olive Oil",
                        "3 tbsp Fresh Lemon Juice + 1 Lemon Sliced",
                        "4 Cloves Garlic (crushed)",
                        "1 tbsp Fresh Rosemary (chopped)",
                        "1 tsp Dried Oregano",
                        "1 tsp Smoked Paprika",
                        "1 tsp Sea Salt",
                        "1/2 tsp Black Pepper"
                    }),
                    InstructionsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "Place chicken breasts between plastic wrap and gently pound to an even 3/4-inch thickness.",
                        "In a bowl, whisk together olive oil, lemon juice, crushed garlic, rosemary, oregano, paprika, salt, and pepper.",
                        "Coat chicken thoroughly in marinade. Refrigerate for at least 30 minutes (or up to 4 hours).",
                        "Preheat grill or grill pan over medium-high heat and oil the grates lightly.",
                        "Grill chicken breasts for 6-7 minutes on each side until internal temperature reaches 165°F (74°C) with nice grill marks.",
                        "Grill lemon slices for 2 minutes alongside the chicken for extra caramelized citrus flavor.",
                        "Let chicken rest for 5 minutes before slicing. Serve with roasted vegetables or wild rice."
                    })
                },
                new Recipe
                {
                    Title = "Royal Dum Biryani with Spiced Basmati",
                    Description = "Layered fragrant basmati rice infused with saffron, caramelized onions, warm whole spices, and tender marinated chicken cooked on dum.",
                    Category = "Dinner",
                    Cuisine = "Pakistani",
                    PrepTimeMinutes = 30,
                    CookTimeMinutes = 45,
                    Servings = 6,
                    Difficulty = "Hard",
                    ImageUrl = "/images/foodmenu/menu12/food3.jpg",
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    ViewsCount = 389,
                    IngredientsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "800g Chicken (curry cut)",
                        "3 cups Aged Long-Grain Basmati Rice",
                        "1 cup Plain Greek Yogurt",
                        "2 Large Onions (thinly sliced & golden fried 'Birista')",
                        "2 tbsp Ginger-Garlic Paste",
                        "1 tbsp Garam Masala Powder",
                        "1 tbsp Red Chili Powder & 1/2 tsp Turmeric",
                        "Whole Spices (Cardamom, Cloves, Cinnamon, Star Anise)",
                        "A pinch of Saffron soaked in 3 tbsp warm milk",
                        "3 tbsp Pure Desi Ghee",
                        "Fresh Mint & Coriander leaves"
                    }),
                    InstructionsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "Marinate chicken with yogurt, ginger-garlic paste, red chili powder, turmeric, garam masala, half of the fried onions, mint, and salt for 1 hour.",
                        "Wash and soak basmati rice for 30 minutes. In a large pot of boiling water with whole spices and salt, cook rice until 70% done. Drain carefully.",
                        "In a heavy-bottomed handi or pot, heat ghee and sear marinated chicken for 10 minutes until aromatic.",
                        "Layer the parboiled rice over the chicken evenly.",
                        "Top with remaining crispy fried onions, fresh mint, coriander, saffron milk, and a drizzle of desi ghee.",
                        "Seal pot tightly with aluminum foil and lid. Cook on high for 5 minutes, then reduce to lowest heat (dum) for 25 minutes.",
                        "Turn off heat and let rest for 10 minutes before gently fluffing rice with a flat spatula. Serve with mint raita and kachumber salad."
                    })
                },
                new Recipe
                {
                    Title = "Pan-Seared Salmon with Garlic Butter Asparagus",
                    Description = "Crispy skin Atlantic salmon fillets basted with lemon garlic butter, paired with tender grilled asparagus spears.",
                    Category = "Dinner",
                    Cuisine = "Continental",
                    PrepTimeMinutes = 10,
                    CookTimeMinutes = 15,
                    Servings = 2,
                    Difficulty = "Medium",
                    ImageUrl = "/images/foodmenu/menu12/food4.jpg",
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    ViewsCount = 176,
                    IngredientsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "2 Fresh Atlantic Salmon Fillets (skin-on)",
                        "1 bunch Fresh Asparagus (woody ends trimmed)",
                        "3 tbsp Unsalted Butter",
                        "3 Cloves Garlic (minced)",
                        "1 Lemon (halved, for juicing and wedges)",
                        "1 tbsp Olive Oil",
                        "Sea Salt, Coarse Black Pepper, and Dried Dill",
                        "Fresh Parsley for finishing"
                    }),
                    InstructionsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "Pat salmon fillets completely dry with paper towels. Season skin and flesh generously with salt and pepper.",
                        "Heat olive oil in a heavy stainless steel or cast-iron skillet over medium-high heat until shimmering.",
                        "Place salmon skin-side down. Press gently with spatula for 15 seconds to ensure even contact. Cook for 5-6 minutes without moving.",
                        "Flip carefully and cook the flesh side for 3 minutes.",
                        "Add butter, minced garlic, lemon juice, and trimmed asparagus around the fish.",
                        "Tilt the skillet and continuously spoon the foaming garlic butter over the salmon for 2 minutes.",
                        "Transfer salmon and crisp-tender asparagus to plates and pour pan juices on top."
                    })
                },
                new Recipe
                {
                    Title = "Molten Chocolate Lava Cake with Gelato",
                    Description = "Decadent dark chocolate sponge with a warm, flowing molten chocolate center, served with vanilla bean gelato.",
                    Category = "Dessert",
                    Cuisine = "French",
                    PrepTimeMinutes = 15,
                    CookTimeMinutes = 12,
                    Servings = 4,
                    Difficulty = "Medium",
                    ImageUrl = "/images/foodmenu/menu/5.jpg",
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    ViewsCount = 280,
                    IngredientsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "170g High Quality 70% Bittersweet Dark Chocolate",
                        "1/2 cup Unsalted Butter",
                        "2 Whole Eggs + 2 Egg Yolks",
                        "1/3 cup Granulated Sugar",
                        "1 tsp Pure Vanilla Extract",
                        "1/4 cup All-Purpose Flour",
                        "Pinch of Salt",
                        "Powdered sugar for dusting",
                        "Vanilla Bean Gelato or Ice Cream"
                    }),
                    InstructionsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "Preheat oven to 425°F (220°C). Generously butter 4 ramekins and dust with cocoa powder.",
                        "Chop chocolate and butter. Melt together in a heatproof bowl set over a pot of simmering water (double boiler) until silky smooth.",
                        "In another bowl, whisk eggs, egg yolks, sugar, and vanilla until pale and frothy.",
                        "Fold melted chocolate mixture into the egg mixture.",
                        "Gently fold in flour and a pinch of salt until just incorporated.",
                        "Divide batter evenly into ramekins. Bake for exactly 12-14 minutes until edges are firm but center is soft and jiggly.",
                        "Let sit for 1 minute, then carefully run a knife along edges and invert onto dessert plates. Dust with powdered sugar and top with gelato."
                    })
                },
                new Recipe
                {
                    Title = "Iced Mango Mint Mojito Mocktail",
                    Description = "Refreshing tropical cooler infused with ripe mango puree, fresh crushed mint leaves, lime juice, and sparkling soda.",
                    Category = "Drinks",
                    Cuisine = "Tropical",
                    PrepTimeMinutes = 5,
                    CookTimeMinutes = 0,
                    Servings = 2,
                    Difficulty = "Easy",
                    ImageUrl = "/images/foodmenu/menu/4.jpg",
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    ViewsCount = 135,
                    IngredientsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "1 cup Fresh Sweet Mango Puree",
                        "12 Fresh Mint Leaves",
                        "2 Limes (sliced into wedges)",
                        "2 tbsp Agave Nectar or Simple Sugar Syrup",
                        "Crushed Ice",
                        "Chilled Club Soda or Sparkling Water",
                        "Mint sprigs and lime wheels for garnish"
                    }),
                    InstructionsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "In tall serving glasses, add fresh mint leaves and lime wedges.",
                        "Muddle gently with a wooden muddler to release the essential citrus oils and fresh mint aroma (do not shred the leaves).",
                        "Add simple syrup and fresh mango puree to each glass.",
                        "Fill glasses to the top with crushed ice.",
                        "Top off with chilled sparkling soda water and stir gently with a long cocktail spoon.",
                        "Garnish with a sprig of fresh mint and a lime wheel. Serve with a reusable glass straw."
                    })
                }
            };

            context.Recipes.AddRange(recipes);
            await context.SaveChangesAsync();

            // Seed initial reviews
            var reviews = new List<Review>
            {
                new Review
                {
                    RecipeId = recipes[0].Id,
                    UserId = regularUser.Id,
                    AuthorName = "Sarah Jenkins",
                    Rating = 5,
                    Comment = "These pancakes turned out unbelievably fluffy! My kids asked for a second batch immediately. 10/10 recipe!",
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Review
                {
                    RecipeId = recipes[4].Id,
                    UserId = regularUser.Id,
                    AuthorName = "Sarah Jenkins",
                    Rating = 5,
                    Comment = "The aroma of this biryani is out of this world. The chicken was melt-in-the-mouth tender. Perfectly explained steps!",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();
        }
    }
}
