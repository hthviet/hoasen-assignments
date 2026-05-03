#!/usr/bin/env zsh

set -euo pipefail

ROOT_DIR="$HOME/Library/Mobile Documents/com~apple~CloudDocs/NHEO"
BACKEND_SCRIPT="$ROOT_DIR/start-backend.sh"
RN_DIR="$ROOT_DIR/PT_MOBILE_RN"
BACKEND_PORT=5226
BACKEND_LOG="/tmp/pt_backend.log"
DEFAULT_EXPO_PORT=8081

if [[ ! -x "$BACKEND_SCRIPT" ]]; then
  echo "Missing or non-executable backend script: $BACKEND_SCRIPT"
  exit 1
fi

if [[ ! -d "$RN_DIR" ]]; then
  echo "React Native directory not found: $RN_DIR"
  exit 1
fi

if ! command -v npm >/dev/null 2>&1; then
  echo "npm is not installed or not in PATH"
  exit 1
fi

if ! command -v nc >/dev/null 2>&1; then
  echo "nc is required but not found in PATH"
  exit 1
fi

BACKEND_PID=""

find_free_port() {
  local port=$1
  while nc -z localhost "$port" >/dev/null 2>&1; do
    port=$((port + 1))
  done
  echo "$port"
}

cleanup() {
  if [[ -n "${BACKEND_PID:-}" ]] && kill -0 "$BACKEND_PID" 2>/dev/null; then
    echo "Stopping backend (PID: $BACKEND_PID)"
    kill "$BACKEND_PID" 2>/dev/null || true
  fi
}

trap cleanup EXIT INT TERM

echo "Starting backend..."
"$BACKEND_SCRIPT" > "$BACKEND_LOG" 2>&1 &
BACKEND_PID=$!
echo "Backend log: $BACKEND_LOG"

for i in {1..60}; do
  if nc -z localhost "$BACKEND_PORT" >/dev/null 2>&1; then
    echo "Backend is up on http://localhost:$BACKEND_PORT"
    break
  fi

  if ! kill -0 "$BACKEND_PID" 2>/dev/null; then
    echo "Backend exited unexpectedly. Check $BACKEND_LOG"
    exit 1
  fi

  sleep 1

  if [[ "$i" -eq 60 ]]; then
    echo "Timed out waiting for backend on port $BACKEND_PORT"
    exit 1
  fi
done

echo "Starting Expo Android app..."
cd "$RN_DIR"
EXPO_PORT=$(find_free_port "$DEFAULT_EXPO_PORT")
if [[ "$EXPO_PORT" -ne "$DEFAULT_EXPO_PORT" ]]; then
  echo "Metro default port $DEFAULT_EXPO_PORT is busy, using $EXPO_PORT"
fi

export EXPO_NO_INTERACTIVE=1
npm run android -- --port "$EXPO_PORT"
