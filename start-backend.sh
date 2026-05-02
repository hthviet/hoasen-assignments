#!/usr/bin/env zsh

set -euo pipefail

PORT=5226
APP_DIR="$HOME/Library/Mobile Documents/com~apple~CloudDocs/NHEO/PT_WEB/LaptopStoreWeb"

PIDS=$(lsof -t -iTCP:$PORT -sTCP:LISTEN 2>/dev/null || true)
if [[ -n "$PIDS" ]]; then
  echo "Killing process on port $PORT: $PIDS"
  kill $PIDS || true
  sleep 1
fi

cd "$APP_DIR"
echo "Starting backend at http://localhost:$PORT"
exec dotnet run
