#!/usr/bin/env zsh

set -euo pipefail

ADB="$HOME/Library/Android/sdk/platform-tools/adb"
if [[ ! -x "$ADB" ]]; then
  echo "adb not found at $ADB"
  exit 1
fi

if [[ -z "$($ADB devices | awk 'NR>1 && $2=="device" {print $1}' | head -n1)" ]]; then
  echo "No emulator/device found. Start an emulator first."
  exit 1
fi

$ADB shell am start -n com.laptopstore.ptmobile/com.laptopstore.ptmobile.activities.SplashActivity

echo "App launched."
