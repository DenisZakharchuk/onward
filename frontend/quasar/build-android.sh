#!/bin/bash
set -e

# Android APK build script using Docker
# No local Java installation required!

# Ensure we're in the right directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "🚀 Building Android APK using Docker..."
echo "📂 Working directory: $SCRIPT_DIR"
echo ""

# Build Docker image if it doesn't exist
if ! docker images | grep -q "quasar-android-builder"; then
    echo "📦 Building Docker image (this may take a few minutes on first run)..."
    docker build -f Dockerfile.android -t quasar-android-builder .
    echo ""
fi

# Run build in container
echo "🔨 Compiling Android APK..."
docker run --rm \
    -v "$(pwd):/app" \
    -v "$(pwd)/src-capacitor/android/app/build:/build-output" \
    quasar-android-builder

echo ""
echo "📦 Extracting APK from container..."

# Find and copy APK
APK_PATH=$(find src-capacitor/android/app/build/outputs/apk -name "*.apk" | head -n 1)

if [ -n "$APK_PATH" ]; then
    mkdir -p ./dist-android
    cp "$APK_PATH" ./dist-android/
    APK_NAME=$(basename "$APK_PATH")
    
    echo "✅ SUCCESS!"
    echo ""
    echo "📱 APK Location: ./dist-android/$APK_NAME"
    echo "📊 File Size: $(du -h ./dist-android/$APK_NAME | cut -f1)"
    echo ""
    echo "To install on device:"
    echo "  adb install ./dist-android/$APK_NAME"
    echo ""
else
    echo "❌ ERROR: APK not found!"
    exit 1
fi
