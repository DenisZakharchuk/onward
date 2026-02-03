# Installing APK on Android Device

## Quick Install Methods

### Method 1: ADB (Recommended)
```bash
# Install via USB debugging
adb install dist-android/app-debug.apk

# Or if device not detected
adb devices
adb install -r dist-android/app-debug.apk
```

### Method 2: Direct Transfer + Install

#### Step 1: Enable Developer Options
1. Go to **Settings** → **About Phone**
2. Tap **Build Number** 7 times
3. Go back to **Settings** → **Developer Options**
4. Enable **USB Debugging**

#### Step 2: Enable Unknown Sources
1. Go to **Settings** → **Security** (or **Apps**)
2. Enable **Install Unknown Apps**
3. Select your file manager app and allow installations

#### Step 3: Transfer & Install
```bash
# Transfer APK to device
adb push dist-android/app-debug.apk /sdcard/Download/

# Or use file transfer manually, then on device:
# 1. Open file manager
# 2. Navigate to Downloads folder
# 3. Tap app-debug.apk
# 4. Tap "Install"
```

## Troubleshooting

### "App not installed as package appears to be invalid"

**Cause:** Release APK not signed, or corrupted during transfer

**Solution 1: Use Debug APK** (easier for testing)
```bash
cd /home/zakharchukd/develop/onward/frontend/quasar
./build-android.sh  # Now builds debug APK by default
```

**Solution 2: Sign Release APK**

Create signing keystore:
```bash
keytool -genkey -v -keystore my-release-key.jks \
  -keyalg RSA -keysize 2048 -validity 10000 \
  -alias my-key-alias
```

Configure in `src-capacitor/android/app/build.gradle`:
```gradle
android {
    signingConfigs {
        release {
            storeFile file('../../my-release-key.jks')
            storePassword 'your-password'
            keyAlias 'my-key-alias'
            keyPassword 'your-password'
        }
    }
    buildTypes {
        release {
            signingConfig signingConfigs.release
        }
    }
}
```

### "Installation blocked"

**Cause:** Security settings prevent installation

**Solution:**
1. Settings → Security → **Unknown Sources** (enable)
2. Or Settings → Apps → Special Access → **Install Unknown Apps** → Enable for your file manager

### "App not compatible with this device"

**Cause:** APK architecture mismatch

**Solution:** Check device architecture and rebuild for correct platform:
```bash
# Check device architecture
adb shell getprop ro.product.cpu.abi

# Most devices are arm64-v8a, but if needed, configure in:
# src-capacitor/android/app/build.gradle
android {
    defaultConfig {
        ndk {
            abiFilters 'arm64-v8a', 'armeabi-v7a', 'x86', 'x86_64'
        }
    }
}
```

### APK corrupted during transfer

**Cause:** File transfer interrupted or filesystem corruption

**Solution:**
```bash
# Verify APK integrity on device
adb shell md5sum /sdcard/Download/app-debug.apk

# Compare with source
md5sum dist-android/app-debug.apk

# If different, retransfer
adb push dist-android/app-debug.apk /sdcard/Download/app-debug.apk
```

## Installation via ADB (Detailed)

### Prerequisites
Install ADB on your computer:
```bash
# Ubuntu/Debian
sudo apt install android-tools-adb

# Or download Android SDK Platform Tools
# https://developer.android.com/tools/releases/platform-tools
```

### Enable USB Debugging on Device
1. Settings → About Phone → Tap "Build Number" 7 times
2. Settings → Developer Options → Enable "USB Debugging"
3. Connect device via USB
4. Accept "Allow USB Debugging" prompt on device

### Install APK
```bash
# Check device connected
adb devices

# Install APK
adb install dist-android/app-debug.apk

# Reinstall (if already installed)
adb install -r dist-android/app-debug.apk

# Uninstall first, then install
adb uninstall dzak.inventory.dashboard
adb install dist-android/app-debug.apk
```

### Useful ADB Commands
```bash
# View device logs
adb logcat | grep -i inventory

# Launch app
adb shell am start -n dzak.inventory.dashboard/.MainActivity

# Clear app data
adb shell pm clear dzak.inventory.dashboard

# Uninstall app
adb uninstall dzak.inventory.dashboard

# List installed packages
adb shell pm list packages | grep inventory

# Take screenshot
adb shell screencap /sdcard/screenshot.png
adb pull /sdcard/screenshot.png
```

## Wireless ADB (No USB Cable)

### Setup
```bash
# Connect device via USB first
adb tcpip 5555

# Find device IP: Settings → About Phone → Status → IP Address
# Or: adb shell ip addr show wlan0 | grep inet

# Connect wirelessly (replace with your device IP)
adb connect 192.168.0.XXX:5555

# Disconnect USB cable

# Verify connection
adb devices

# Install APK wirelessly
adb install dist-android/app-debug.apk
```

### Disconnect
```bash
adb disconnect
```

## Building Different Variants

### Debug APK (default, no signing needed)
```bash
./build-android.sh
# Output: dist-android/app-debug.apk
```

### Release APK (needs signing)
Modify Dockerfile.android CMD to remove `--debug` flag, then:
```bash
docker build -f Dockerfile.android -t quasar-android-builder .
./build-android.sh
# Output: dist-android/app-release.apk (unsigned - won't install)
```

### Signed Release APK
1. Create keystore (see "Sign Release APK" above)
2. Configure signing in build.gradle
3. Build release APK
4. APK will be signed automatically

## Testing on Emulator

### Install Android Emulator
1. Download Android Studio
2. Tools → AVD Manager → Create Virtual Device
3. Start emulator

### Install APK on Emulator
```bash
# List running emulators
adb devices

# Install APK
adb install dist-android/app-debug.apk
```

## Common Issues

| Error | Solution |
|-------|----------|
| `adb: device unauthorized` | Accept prompt on device, or: `adb kill-server && adb start-server` |
| `adb: no devices/emulators found` | Enable USB debugging, check USB cable, install drivers |
| `INSTALL_FAILED_UPDATE_INCOMPATIBLE` | Uninstall old version first: `adb uninstall dzak.inventory.dashboard` |
| `INSTALL_FAILED_INSUFFICIENT_STORAGE` | Free up device storage |
| `INSTALL_PARSE_FAILED_NO_CERTIFICATES` | Use debug APK or sign release APK |

## App Info

- **Package Name:** `dzak.inventory.dashboard`
- **App Name:** Inventory Dashboard
- **Debug APK:** `dist-android/app-debug.apk`
- **Release APK:** `dist-android/app-release.apk`
