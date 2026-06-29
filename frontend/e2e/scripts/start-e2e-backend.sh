#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPO_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../../.." && pwd)
COMPOSE_FILE="$REPO_ROOT/backend/docker-compose-e2e.yml"

if nc -z 127.0.0.1 5154 >/dev/null 2>&1; then
  echo "Cannot start E2E backend: port 5154 is already in use." >&2
  echo "Stop the existing backend process before running ng e2e again." >&2
  exit 1
fi

cleanup() {
  docker compose -f "$COMPOSE_FILE" down --volumes --remove-orphans >/dev/null 2>&1 || true
}

trap cleanup EXIT INT TERM

cleanup
docker compose -f "$COMPOSE_FILE" up -d

for _ in $(seq 1 30); do
  if docker compose -f "$COMPOSE_FILE" exec -T postgres pg_isready -U myuser -d paytrack_e2e >/dev/null 2>&1; then
    break
  fi

  sleep 1
done

cd "$REPO_ROOT"
ASPNETCORE_ENVIRONMENT=E2E dotnet run --no-launch-profile --project backend/PayTrack/PayTrack.csproj --urls http://localhost:5154
