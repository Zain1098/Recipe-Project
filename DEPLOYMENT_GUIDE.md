# Free Live Deployment Guide (Render.com)

Aapka project ab **Docker-ready** aur **Cloud-ready** hai. Aap isko **Render.com** par 100% free live host kar sakte hain.

---

## Step 1: Code Ko GitHub Par Push Karein

Apne terminal mein yeh commands run karein:

```bash
git add .
git commit -m "Add full-stack features: authentication, recipe CRUD, search, reviews, and Docker support"
git push origin main
```

---

## Step 2: Render.com Par Free Account Banayein

1. Website par jayein: **[https://render.com](https://render.com)**
2. **"Get Started"** ya **"Sign in with GitHub"** par click karein (apne usi GitHub account se login karein jismein `Recipe-Project` repository hai).

---

## Step 3: Web Service Create Karein

1. Render Dashboard par **"New +"** button click karein aur **"Web Service"** select karein.
2. **"Build and deploy from a Git repository"** choose karein.
3. List mein se apni repository **`Recipe-Project`** select karein (ya repository ka URL paste karein).
4. Settings form mein:
   - **Name:** `recipe-project` (ya koi bhi pasandeeda naam)
   - **Region:** Default (e.g. Oregon ya Frankfurt)
   - **Branch:** `main`
   - **Language:** **`Docker`** (Render automatically repository mein maujood `Dockerfile` detect kar lega)
   - **Instance Type:** **Free** ($0 / month)
5. **"Deploy Web Service"** par click kar dein!

---

## Step 4: Live Link Hasil Karein! 🎉

Render 2-3 minute mein project ko build karega aur aapko ek live public URL de dega:
👉 **`https://recipe-project-xxxx.onrender.com`**

Is link ko aap apne:
- **CV / Resume**
- **LinkedIn Profile**
- **Portfolio Website**

par share kar sakte hain!
