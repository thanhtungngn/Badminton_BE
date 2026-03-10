#!/usr/bin/env bash
set -euo pipefail

if [ -z "${CONNECTION_STRING-}" ]; then
  echo "Usage: CONNECTION_STRING='<conn>' ./scripts/run_migrations.sh"
  echo "Set environment variable CONNECTION_STRING to your DB connection string (e.g. server=...;user=...;password=...;database=BadmintonDb)"
  exit 1
fi

# Run EF Core migrations inside a .NET SDK container using the source mounted
# The script installs dotnet-ef tool (if missing), restores and runs migrations.

docker run --rm \
  -e ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
  -v "$(pwd)":/src -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 bash -lc \
  "dotnet tool install --global dotnet-ef --version 9.* || true && export PATH=\"\$PATH:/root/.dotnet/tools\" && dotnet restore && dotnet ef database update --project Badminton_BE --startup-project Badminton_BE"

echo "Migrations applied."
