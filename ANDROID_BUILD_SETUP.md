# Android Build Setup

## ✅ Completed Steps

### 1. Capacitor Integration
- ✅ Added Capacitor mode to Quasar project
- ✅ Installed Capacitor v8 dependencies (`@capacitor/core`, `@capacitor/cli`, `@capacitor/android`)
- ✅ Created Android platform in `src-capacitor/android/`
- ✅ Configured `capacitor.config.json` with app metadata:
  - App ID: `dzak.inventory.dashboard`
  - App Name: `Inventory Dashboard`
  - Web Dir: `www`
  - Android scheme: HTTPS with cleartext support for dev API

### 2. Environment Configuration
- ✅ Created `.env.capacitor` for mobile builds with network IP:
  ```
  VITE_API_BASE_URL=http://192.168.0.102:5002/api
  ```
- ✅ Updated `quasar.config.ts` to load `.env.capacitor` for Capacitor builds
- ✅ Existing `.env` remains for web development (localhost)

### 3. Build Configuration
- ✅ Fixed all TypeScript ESLint errors:
  - Removed unused error variables in catch blocks
  - Added `void` operator for floating promises
  - Fixed `async` callbacks in `onOk` handlers
- ✅ Successfully built web assets for Capacitor
- ✅ Synced assets to Android project structure

## 📋 Build Output

```
Total JS: 494.88 KB (18 files)
Total CSS: 195.37 KB (1 file)
```

Web assets successfully compiled and synced to:
- `src-capacitor/android/app/src/main/assets/public/`

## 🚀 Building Android APK

### Option 1: Docker Build (Recommended - No Java Required!)

Build the APK using Docker without installing Java locally:

```bash
cd /home/zakharchukd/develop/onward/frontend/quasar
./build-android.sh
```

**First run:** Docker image build takes ~5 minutes (downloads Java 17 + Android SDK)  
**Subsequent runs:** ~2-3 minutes (image cached)

Output: `dist-android/app-release.apk`

**What the script does:**
1. Builds Docker image with Java 17 + Android SDK (first run only)
2. Compiles Quasar app for Capacitor
3. Builds Android APK using Gradle
4. Extracts APK to `dist-android/` folder

### Option 2: Local Build (Requires Java)

Install Java 17+ locally:

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install openjdk-17-jdk

# Set environment
export JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
export PATH=$JAVA_HOME/bin:$PATH
```

Build APK:
```bash
cd /home/zakharchukd/develop/onward/frontend/quasar
npx quasar build -m capacitor -T android
```

Output: `src-capacitor/android/app/build/outputs/apk/release/app-release.apk`

### Option 3: Android Studio (Full IDE)
```bash
npx quasar build -m capacitor -T android --ide
```

Opens Android Studio for visual debugging and building.

### Testing on Device

#### Option 1: Install APK directly
```bash
# Transfer APK to device and install
adb install src-capacitor/android/app/build/outputs/apk/release/app-release.apk
```

#### Option 2: Run in Android Studio
1. Open Android Studio
2. File → Open → Select `src-capacitor/android/`
3. Connect device or start emulator
4. Click "Run" button

## 🔧 Configuration Files

### capacitor.config.json (root)
```json
{
  "appId": "dzak.inventory.dashboard",
  "appName": "Inventory Dashboard",
  "webDir": "www",
  "bundledWebRuntime": false,
  "server": {
    "androidScheme": "https",
    "cleartext": true
  },
  "android": {
    "allowMixedContent": true
  }
}
```

### .env.capacitor
```
VITE_API_BASE_URL=http://192.168.0.102:5002/api
```

## 📱 App Icons & Splash Screens

To customize app icons and splash screens:

1. Generate icon assets:
   ```bash
   npm install -g @capacitor/assets
   cd /home/zakharchukd/develop/onward/frontend/quasar
   ```

2. Create source images:
   - `resources/icon.png` (1024x1024px)
   - `resources/splash.png` (2732x2732px)

3. Generate platform assets:
   ```bash
   npx capacitor-assets generate
   ```

## 🚀 Quick Build Commands

### Docker build (no Java required)
```bash
cd /home/zakharchukd/develop/onward/frontend/quasar
./build-android.sh
```

### Docker build - rebuild image
```bash
docker build -f Dockerfile.android -t quasar-android-builder . --no-cache
./build-android.sh
```

### Local build - production APK
```bash
npx quasar build -m capacitor -T android
```

### Local build - debug APK
```bash
npx quasar build -m capacitor -T android --debug
```

### Live reload on device (requires local Java)
```bash
npx quasar dev -m capacitor -T android
```

## 📦 Package Structure

```
frontend/quasar/
├── capacitor.config.json          # Capacitor config (root)
├── .env                          # Web dev environment (localhost)
├── .env.capacitor                # Mobile build environment (network IP)
├── src-capacitor/
│   ├── capacitor.config.json     # Capacitor config (generated)
│   ├── package.json              # Capacitor dependencies
│   └── android/                  # Native Android project
│   Docker build is slow
First run downloads ~1GB (Java + Android SDK). Subsequent builds are cached and much faster.

### Build fails with "JAVA_HOME not set"
Use Docker build method (`./build-android.sh`) to avoid needing local Java installation
│       │   └── src/main/
│       │       ├── AndroidManifest.xml
│       │       └── assets/public/  # Web app assets
│       └── build/outputs/apk/    # Built APK files
└── www/                          # Built web assets (generated)
```

## 🔍 Troubleshooting

### Build fails with "JAVA_HOME not set"
Install JDK 17+ and set environment variables (see above).

### APK doesn't connect to backend
- Verify `.env.capacitor` has correct IP address (not localhost)
- Ensure backend is accessible on network (not bound to 127.0.0.1)
- Check Android manifest allows cleartext traffic
- Verify device is on same network

### "Failed to load module script" error on device
- Rebuild with `npx quasar clean` first
- Ensure `bundledWebRuntime: false` in capacitor.config.json
- Check `androidScheme: "https"` is set correctly

### App crashes on launch
- Check Android Studio Logcat for errors
- Verify all Capacitor dependencies are version 8
- Ensure `@capacitor/app` plugin is installed

## 📚 Resources

- [Quasar Capacitor Guide](https://quasar.dev/quasar-cli-vite/developing-capacitor-apps/configuring-capacitor)
- [Capacitor Android Documentation](https://capacitorjs.com/docs/android)
- [Android Studio Download](https://developer.android.com/studio)
**Build APK**: Run `./build-android.sh` (Docker, no Java needed)
2. **Test on device**: `adb install dist-android/app-release.apk`
3. **Customize icons**: Use `@capacitor/assets` to generate app icons
4. **Play Store release**: Configure signing in `src-capacitor/android/app/build.gradle`

## 🐳 Docker Build Details

### Files
- `Dockerfile.android` - Android build environment (Java 17 + Android SDK 34)
- `build-android.sh` - Build script that uses Docker
- `.dockerignore` - Excludes node_modules and build artifacts

### Docker Image Contents
- Base: Eclipse Temurin Java 17 (Ubuntu Jammy)
- Node.js 20 + npm latest
- Android SDK 34 with build-tools
- Platform tools and Gradle wrapper

### Rebuilding Docker Image
```bash
docker build -f Dockerfile.android -t quasar-android-builder . --no-cache
```

### Manual Docker Build
```bash
# Build image
docker build -f Dockerfile.android -t quasar-android-builder .

# Run build
docker run --rm -v "$(pwd):/app" quasar-android-builder

# Extract APK
docker cp $(docker ps -lq):/app/src-capacitor/android/app/build/outputs/apk/release/app-release.apk ./
```udio
3. Test on Android device or emulator
4. Customize app icons and splash screens
5. Configure signing for Play Store release
