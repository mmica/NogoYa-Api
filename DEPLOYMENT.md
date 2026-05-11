# Nogo-Ya Deployment

Hosting architecture:

| Layer     | Platform             | Free tier | Notes                                       |
|-----------|----------------------|-----------|---------------------------------------------|
| Frontend  | **Vercel**           | Yes       | Static Angular SPA                          |
| Backend   | **Render** (Docker)  | Yes       | .NET 10 running in a container              |
| Database  | **Render Postgres**  | Yes (90d) | Same region as the backend                  |
| CI/CD     | **GitHub Actions**   | Yes       | CI on every PR + CD to main                 |

> **Why not everything on Vercel:** Vercel does not support .NET runtimes or Postgres hosting. It means renting two platforms, but both free tiers cover the MVP without requiring a credit card.

---

## Prerequisites

1. Accounts on [GitHub](https://github.com), [Vercel](https://vercel.com) and [Render](https://render.com).
2. A GitHub repository with this codebase (public or private, both work).

---

## Step 1 — Push to GitHub

```bash
git init
git add .
git commit -m "Initial: Nogo-Ya MVP"
git branch -M main
git remote add origin git@github.com:YOUR_USERNAME/nogoya.git
git push -u origin main
```

---

## Step 2 — Provision the backend + DB on Render

1. Render dashboard → **New → Blueprint**.
2. Connect your GitHub repository.
3. Render detects `render.yaml` and proposes:
   - `nogoya-api` (Docker web service)
   - `nogoya-db` (Postgres)
4. Click **Apply**. Render will:
   - Create the database.
   - Automatically wire `DATABASE_URL` into the web service.
   - Build the image from `backend/Dockerfile`.
   - Apply migrations (they run on startup).
5. Once it shows as **live**, copy the public URL (something like `https://nogoya-api.onrender.com`).

> **Important:** after the first deploy, edit the `Cors__AllowedOrigins` env var in Render with the final Vercel URL (see Step 3).

---

## Step 3 — Frontend on Vercel

1. Vercel dashboard → **Add New → Project** → import the same repository.
2. **Root Directory:** `frontend`.
3. **Framework Preset:** Other (detected automatically by `vercel.json`).
4. **Environment Variables** → add:
   - `NG_APP_API_BASE_URL` = `https://nogoya-api.onrender.com/api/v1`
5. Click **Deploy**.
6. When it finishes, copy the final URL (e.g. `https://nogoya.vercel.app`).
7. Go back to Render → service `nogoya-api` → **Environment** → set:
   - `Cors__AllowedOrigins` = `https://nogoya.vercel.app`
8. Render redeploys automatically with the new CORS configuration.

---

## Step 4 — Wire up GitHub Actions

In GitHub: **Settings → Secrets and variables → Actions → New repository secret**.

| Secret                    | How to obtain it                                                                |
|---------------------------|---------------------------------------------------------------------------------|
| `VERCEL_TOKEN`            | Vercel → Account Settings → Tokens                                              |
| `VERCEL_ORG_ID`           | Run `vercel link` locally → `.vercel/project.json` (`orgId` field)              |
| `VERCEL_PROJECT_ID`       | Same file, `projectId` field                                                    |
| `RENDER_DEPLOY_HOOK_URL`  | Render → service → Settings → Deploy Hook                                       |
| `RENDER_API_KEY`          | Render → Account Settings → API Keys (used to poll the deploy status)           |
| `RENDER_SERVICE_ID`       | Service URL: `https://dashboard.render.com/web/srv-XXXXX` → copy the `srv-...`  |

The workflows are wired as follows:

- `.github/workflows/ci.yml` runs on every PR and push to `main`: build + test the side that changed.
- `.github/workflows/deploy.yml` triggers when CI passes on `main`: deploys to Vercel and fires the Render deploy hook.

---

## Step 5 — Verification

1. `https://nogoya-api.onrender.com/health` → `{ "status": "ok", "time": "..." }`
2. `https://nogoya-api.onrender.com/swagger` → Swagger UI.
3. `https://nogoya.vercel.app` → frontend loading products from the backend.

---

## Operational tips

- **Cold starts on Render free:** after 15 minutes without traffic the container sleeps. The first request takes ~30s. For real production → upgrade to `starter` ($7/month).
- **Postgres free lasts 90 days.** Render warns you in advance; either move to a paid plan or migrate to Neon/Supabase and update `DATABASE_URL`.
- **Logs:** Render → service → Logs (stdout via Serilog).
- **New migrations:** add them with `dotnet ef migrations add MigrationName -p src/NogoYa.Infrastructure -s src/NogoYa.API`. They are applied automatically on the next deploy thanks to `MigrateAsync()` at startup.
- **Rollback:** Render → service → Deploys → Rollback. Vercel: any preview deploy can be promoted to production.

---

## Troubleshooting

| Symptom                                            | Likely cause                                          | Fix                                                |
|----------------------------------------------------|-------------------------------------------------------|----------------------------------------------------|
| 500 when calling the API from Vercel               | CORS misconfigured                                    | Set `Cors__AllowedOrigins` in Render               |
| Frontend points to `localhost` in production       | `NG_APP_API_BASE_URL` not set in Vercel               | Project Settings → Environment Variables           |
| `relation "stores" does not exist`                 | Migrations did not run                                | Check first-deploy logs; verify `DATABASE_URL`     |
| Docker build fails on `dotnet restore`             | `NogoYa.sln` does not include all four projects       | Verify project paths in the solution               |
| Render does not redeploy on push                   | `autoDeploy` disabled / wrong branch                  | Render → service → Settings → Auto-Deploy = Yes    |
