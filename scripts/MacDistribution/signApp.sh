#!/bin/bash
APP_PATH=$1
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
ENTITLEMENTS=$SCRIPT_DIR"/CpcSharp.entitlements"
SIGNING_IDENTITY="Developer ID Application: Nathan Randle (AJ9VCT4GE7)"

find "$APP_PATH/Contents/MacOS"|while read fname; do
    if [[ -f $fname ]]; then
        echo "[INFO] Signing $fname"
        codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$fname"
    fi
done

echo "[INFO] Signing app file"

codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$APP_PATH"