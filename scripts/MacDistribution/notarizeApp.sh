#!/bin/bash
set -e

APP_PATH=$1

# Check if app path is provided
if [ -z "$APP_PATH" ]; then
    echo "[ERROR] App path not provided"
    echo "Usage: $0 <path-to-app>"
    exit 1
fi

# Check if app exists
if [ ! -d "$APP_PATH" ]; then
    echo "[ERROR] App not found at: $APP_PATH"
    exit 1
fi

# Check for required environment variables
if [ -z "$APPLE_ID" ]; then
    echo "[ERROR] APPLE_ID environment variable not set"
    exit 1
fi

if [ -z "$APPLE_ID_PASSWORD" ]; then
    echo "[ERROR] APPLE_ID_PASSWORD environment variable not set"
    exit 1
fi

if [ -z "$APPLE_TEAM_ID" ]; then
    echo "[ERROR] APPLE_TEAM_ID environment variable not set"
    exit 1
fi

APP_NAME=$(basename "$APP_PATH")
ZIP_PATH="${APP_PATH}.zip"

echo "[INFO] Creating zip archive for notarization..."
ditto -c -k --keepParent "$APP_PATH" "$ZIP_PATH"

echo "[INFO] Submitting app for notarization..."
SUBMIT_OUTPUT=$(xcrun notarytool submit "$ZIP_PATH" \
    --apple-id "$APPLE_ID" \
    --password "$APPLE_ID_PASSWORD" \
    --team-id "$APPLE_TEAM_ID" \
    --wait)

echo "$SUBMIT_OUTPUT"

# Extract the submission ID from the output
SUBMISSION_ID=$(echo "$SUBMIT_OUTPUT" | grep -E "^\s+id:" | awk '{print $2}')

if [ -z "$SUBMISSION_ID" ]; then
    echo "[ERROR] Failed to extract submission ID from notarytool output"
    exit 1
fi

echo "[INFO] Submission ID: $SUBMISSION_ID"

# Check if notarization was successful
if echo "$SUBMIT_OUTPUT" | grep -q "status: Accepted"; then
    echo "[INFO] Notarization successful!"

    echo "[INFO] Stapling notarization ticket to app..."
    xcrun stapler staple "$APP_PATH"

    echo "[INFO] Verifying stapled app..."
    xcrun stapler validate "$APP_PATH"

    echo "[INFO] Cleaning up zip file..."
    rm -f "$ZIP_PATH"

    echo "[SUCCESS] App successfully notarized and stapled!"
else
    echo "[ERROR] Notarization failed!"
    echo "[INFO] Getting detailed log..."
    xcrun notarytool log "$SUBMISSION_ID" \
        --apple-id "$APPLE_ID" \
        --password "$APPLE_ID_PASSWORD" \
        --team-id "$APPLE_TEAM_ID"

    rm -f "$ZIP_PATH"
    exit 1
fi