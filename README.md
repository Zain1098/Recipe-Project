# 🍳 Deliciousa - Full-Stack Recipe Management & Sharing Platform

A modern, responsive, full-stack culinary web application built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **SQLite**. Designed for home cooks, foodies, and chefs to discover, create, bookmark, and review restaurant-quality recipes.

![Deliciousa Preview](Recipe%20Project/wwwroot/images/foodmenu/menu12/food3.jpg)

---

## 🌟 Key Features

- **🔐 User Authentication & Authorization:**
  - Secure registration, login, and cookie-based authentication.
  - Password hashing with PBKDF2/SHA256.
  - Role-based authorization (`Admin` vs `User`).
  - Pre-configured demo login buttons for instant recruiter reviews.

- **🍲 Rich Recipe Catalog (CRUD):**
  - **Create:** Publish new recipes with image file upload, category selection, prep/cook times, servings, difficulty ratings, and dynamic ingredient/step lists.
  - **Read:** Dedicated recipe detail pages with interactive ingredient checklists, step-by-step numbered cooking instructions, and author details.
  - **Update & Delete:** Recipe creators and administrators can edit or delete their recipes.

- **🔍 Search & Filtering:**
  - Instant search across recipe titles, descriptions, cuisines, and ingredients.
  - Category filters: *Breakfast, Lunch, Dinner, Dessert, Drinks, Fast Food*.
  - Sort by *Newest, Most Popular, or Top Rated*.

- **⭐ Community Reviews & Ratings:**
  - 5-star rating system with user feedback comments.
  - Live average rating calculations and review counts.

- **❤️ Bookmarks & Saved Recipes:**
  - AJAX-powered 1-click favorite bookmarking system.
  - Dedicated "My Saved Recipes" view.

- **🐳 Docker & Cloud Ready:**
  - Multi-stage production `Dockerfile` included.
  - 1-click free deployment configuration for **Render.com** or any container cloud host.

---

## 🛠️ Tech Stack & Architecture

- **Backend:** C# / ASP.NET Core 8.0 (MVC Pattern)
- **Database / ORM:** SQLite + Entity Framework Core 8.0
- **Authentication:** Cookie Authentication (`Microsoft.AspNetCore.Authentication.Cookies`)
- **Frontend:** Razor Views (`.cshtml`), Bootstrap 5, FontAwesome, Owl Carousel, Swiper, jQuery
- **Containerization:** Docker (Multi-stage build)

---

## 🚀 Getting Started Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Installation & Run

1. Clone repository:
   ```bash
   git clone https://github.com/Zain1098/Recipe-Project.git
   cd Recipe-Project
   ```

2. Build and run:
   ```bash
   dotnet run --project "Recipe Project/Recipe Project.csproj"
   ```

3. Open your browser at:
   ```
   http://localhost:5195
   ```

The database (`recipes.db`) will automatically initialize and seed sample gourmet recipes and demo accounts on first launch.

---

## 🌐 Free Live Deployment

Follow the step-by-step guide in [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) to deploy to **Render.com** for free with automated GitHub deployments.

---

## 👨‍🍳 Author

- **Zain** — [GitHub Profile](https://github.com/Zain1098)
