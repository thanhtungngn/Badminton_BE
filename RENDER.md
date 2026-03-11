Deploying Badminton_BE to Render

This repository already includes a `Dockerfile`. Follow these steps to deploy to Render and run EF Core migrations.

1) Push code to GitHub
   - Commit your changes and push to the branch you will use (e.g. `master` or `main`).

2) Create a MySQL database
   - Render does not provide managed MySQL. Use an external MySQL provider (Amazon RDS, PlanetScale, ClearDB, etc.).
   - Note the connection string in this format:
     server=<HOST>;port=3306;database=BadmintonDb;user=<USER>;password=<PASSWORD>;

3) Create a Web Service on Render
   - Sign in to https://render.com and connect your GitHub account.
   - Click New -> Web Service.
   - Select your repository and branch.
   - Environment: `Docker`.
   - Dockerfile Path: `/Dockerfile` (project root).
   - Name: choose `badminton-be`.
   - Plan: `Free` or `Starter`.
   - Click Create Web Service.

4) Configure environment variables (in Render UI)
   - In the service settings, set an environment variable:
     - Key: `ConnectionStrings__DefaultConnection`
     - Value: `server=...;port=3306;database=BadmintonDb;user=...;password=...;`
   - Save and trigger a redeploy.

5) Run EF Core migrations

Option A — Run migrations locally (recommended):
   CONNECTION_STRING="server=...;port=3306;database=BadmintonDb;user=...;password=...;" ./scripts/run_migrations.sh

Option B — Run a one-off Shell / Job in Render (use Render dashboard "Shell" or Jobs feature):
   # replace $CONN with the connection string you set in Render env if referencing it directly
   /bin/bash -lc "export ConnectionStrings__DefaultConnection='$CONN' && dotnet tool install --global dotnet-ef --version 9.* || true && export PATH=\"$PATH:/root/.dotnet/tools\" && dotnet restore && dotnet ef database update --project Badminton_BE --startup-project Badminton_BE"

Notes and troubleshooting
- Ensure the DB allows connections from Render (public endpoint). Render instances use dynamic IP pools; public DB endpoint is easiest for small projects.
- Check Render logs for build and runtime output if the service fails to start.
- For production, use a managed secrets store and restrict DB access with a private network/VPC.

If you want, I can also create a `render.yaml` manifest for this repo to enable infra-as-code for Render.
