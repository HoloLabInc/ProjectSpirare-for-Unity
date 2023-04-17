# SpirareBrowser-ARFoundation-VAT

This project is a browser application for Spirare.

## Target Devices
ARFoundation supported devices

## Position Alignment Method
Vuforia Area Target

## Project Setup

### Import Vuforia Engine
Download the unitypackage for installing Vuforia Engine from the [developer portal](https://developer.vuforia.com/downloads/SDK).

Open the unity project in UnityEditor and import the package.

### Set Vuforia license key
Select `Assets\Resources\VuforiaLicenseKeySettings` and enter your Vuforia license key.  
If `VuforiaLicenseKeySettings` does not exist, play the `SpirareBrowser_VuforiaAreaTarget` scene once.

<img width="640" alt="Vuforia License settings" src="https://user-images.githubusercontent.com/4415085/230545525-f11b9075-3793-48cd-91d7-c978dcc7279e.png">

## Usage
To create a folder for placing the map files, please launch the application once.  

### Android
Place Vuforia map files (.dat, .xml) under the `Android\data\jp.co.hololab.SpirareBrowserVAT\files\AreaTargetData` folder.

### iOS
Open the `Files` app and place Vuforia map files (.dat, .xml) under the `On My iPad\Spirare VAT\AreaTargetData` folder.