# Deploy de Nogo-Ya

Arquitectura de hosting:

| Capa      | Plataforma           | Plan free | Notas                                     |
|-----------|----------------------|-----------|-------------------------------------------|
| Frontend  | **Vercel**           | Sí        | Angular SPA estático                      |
| Backend   | **Render** (Docker)  | Sí        | .NET 10 corriendo en contenedor           |
| Database  | **Render Postgres**  | Sí (90d)  | En la misma región que el backend         |
| CI/CD     | **GitHub Actions**   | Sí        | CI por PR + CD a main                     |

> **Por qué no todo en Vercel:** Vercel no soporta runtimes .NET ni hosting de Postgres. Renta dos plataformas, pero los dos free tiers cubren el MVP sin tarjeta.

---

## Pre-requisitos

1. Cuenta en [GitHub](https://github.com), [Vercel](https://vercel.com) y [Render](https://render.com).
2. Repo de GitHub con este código (público o privado, ambos funcionan).

---

## Paso 1 — Subir a GitHub

```bash
git init
git add .
git commit -m "Initial: Nogo-Ya MVP"
git branch -M main
git remote add origin git@github.com:TU_USUARIO/nogoya.git
git push -u origin main
```

---

## Paso 2 — Provisionar backend + DB en Render

1. En Render dashboard → **New → Blueprint**.
2. Conectá tu repo de GitHub.
3. Render detecta `render.yaml` y propone:
   - `nogoya-api` (web service Docker)
   - `nogoya-db` (Postgres)
4. Hacé clic en **Apply**. Render:
   - Crea la base de datos.
   - Cablea automáticamente `DATABASE_URL` al servicio web.
   - Buildea la imagen desde `backend/Dockerfile`.
   - Aplica las migraciones (corren al startup).
5. Cuando aparezca como **live**, copiá la URL pública (algo como `https://nogoya-api.onrender.com`).

> **Importante:** después del primer deploy editá la env var `Cors__AllowedOrigins` en Render con la URL final de Vercel (ver Paso 3).

---

## Paso 3 — Frontend en Vercel

1. Vercel dashboard → **Add New → Project** → importá el mismo repo.
2. **Root Directory:** `frontend`.
3. **Framework Preset:** Other (queda detectado por `vercel.json`).
4. **Environment Variables** → agregá:
   - `NG_APP_API_BASE_URL` = `https://nogoya-api.onrender.com/api/v1`
5. Hacé clic en **Deploy**.
6. Cuando termine, copiá la URL final (ej. `https://nogoya.vercel.app`).
7. Volvé a Render → service `nogoya-api` → **Environment** → seteá:
   - `Cors__AllowedOrigins` = `https://nogoya.vercel.app`
8. Render redeploya automáticamente con la nueva CORS.

---

## Paso 4 — Conectar GitHub Actions

En GitHub: **Settings → Secrets and variables → Actions → New repository secret**.

| Secret                    | Cómo obtenerlo                                                                 |
|---------------------------|--------------------------------------------------------------------------------|
| `VERCEL_TOKEN`            | Vercel → Account Settings → Tokens                                             |
| `VERCEL_ORG_ID`           | `vercel link` localmente → `.vercel/project.json` (campo `orgId`)              |
| `VERCEL_PROJECT_ID`       | Mismo archivo, campo `projectId`                                               |
| `RENDER_DEPLOY_HOOK_URL`  | Render → service → Settings → Deploy Hook                                      |
| `RENDER_API_KEY`          | Render → Account Settings → API Keys (para esperar el deploy)                  |
| `RENDER_SERVICE_ID`       | URL del servicio: `https://dashboard.render.com/web/srv-XXXXX` → copia `srv-…` |

Los workflows quedan armados así:

- `.github/workflows/ci.yml` corre en cada PR y push a `main`: build + test del lado que cambió.
- `.github/workflows/deploy.yml` se dispara cuando CI pasa en `main`: deploya a Vercel y dispara el hook de Render.

---

## Paso 5 — Verificación

1. `https://nogoya-api.onrender.com/health` → `{ "status": "ok", "time": "..." }`
2. `https://nogoya-api.onrender.com/swagger` → UI de Swagger.
3. `https://nogoya.vercel.app` → frontend cargando productos desde el backend.

---

## Tips operativos

- **Cold starts en Render free:** después de 15 minutos sin tráfico el contenedor duerme. La primera petición tarda ~30s. En producción real → upgrade a `starter` ($7/mes).
- **Postgres free dura 90 días.** Render avisa antes; o pasás a paid o migrás a Neon/Supabase y actualizás `DATABASE_URL`.
- **Logs**: Render → service → Logs (stdout vía Serilog).
- **Migraciones nuevas**: agregar con `dotnet ef migrations add NombreMigracion -p src/NogoYa.Infrastructure -s src/NogoYa.API`. Se aplican solas en el siguiente deploy gracias al `MigrateAsync()` en startup.
- **Rollback**: Render → service → Deploys → Rollback. Vercel: cualquier deploy preview se puede promover.

---

## Troubleshooting

| Síntoma                                           | Causa probable                                       | Solución                                          |
|---------------------------------------------------|------------------------------------------------------|---------------------------------------------------|
| 500 al llamar al API desde Vercel                 | CORS mal configurado                                 | Setear `Cors__AllowedOrigins` en Render           |
| Frontend muestra `localhost` en producción        | `NG_APP_API_BASE_URL` no seteada en Vercel           | Project Settings → Environment Variables          |
| `relation "stores" does not exist`                | Las migrations no corrieron                           | Ver logs del primer deploy; revisar `DATABASE_URL`|
| Build de Docker falla en `dotnet restore`         | `NogoYa.sln` no incluye los 4 proyectos              | Verificar paths en la solution                    |
| Render no se redeploya tras push                  | `autoDeploy` desactivado / branch incorrecta         | Render → service → Settings → Auto-Deploy = Yes   |
