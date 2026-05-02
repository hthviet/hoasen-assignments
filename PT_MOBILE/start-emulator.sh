#!/usr/bin/env zsh

set -euo pipefail

ADB="$HOME/Library/Android/sdk/platform-tools/adb"
EMULATOR="$HOME/Library/Android/sdk/emulator/emulator"
AVD_NAME="Pixel_7_API_35"

if [[ ! -x "$EMULATOR" ]]; then
  echo "Android emulator binary not found at $EMULATOR"
  exit 1
fi

# If any device is online already, do not start another emulator.
if [[ -n "$($ADB devices | awk 'NR>1 && $2=="device" {print $1}' | head -n1)" ]]; then
  echo "An Android device/emulator is already connected."
  exit 0
fi

nohup "$EMULATOR" -avd "$AVD_NAME" > /tmp/pt_mobile_emulator.log 2>&1 &
echo "Emulator starting: $AVD_NAME"
echo "Log: /tmp/pt_mobile_emulator.log"
