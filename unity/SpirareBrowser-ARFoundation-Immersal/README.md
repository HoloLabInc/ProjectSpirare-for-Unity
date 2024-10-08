# SpirareBrowser-ARFoundation-Immersal

This project is a browser application for Spirare.

## Target Devices

ARFoundation supported devices

## Position Alignment Method

Immersal

## Project Setup

Download the Immersal SDK Core unitypackage from the [developer portal](https://developers.immersal.com) and import it.  
The tested version is Immseral SDK v1.20.0.

## Usage

### Alignment with Immersal

Enter your email address and password and press **Sign In** button.

<img width="640" alt="SB Immersal Sign In Menu" src="https://user-images.githubusercontent.com/4415085/234493232-f2bafb64-b0fc-470a-92dd-d1fa1e959c85.jpg">

Enter the map ID and press the **Load Map** button.

<img width="640" alt="SB Immersal Load Map Menu" src="https://user-images.githubusercontent.com/4415085/234493301-233e2847-f78e-484d-8321-d37c3e1edcfc.jpg">

### Loading POML from web server

Please enter the URL where the POML is being served in the input field and press the **Load** button.

### Loading POML from local files

To create a folder for placing the POML files, please launch the application once.

#### Android

Place POML files (.poml) or POML.zip files (.poml.zip) under the `Android\data\jp.co.hololab.spirarebrowserimmersal\files\LocalContent` folder.

#### iOS

Open the `Files` app and place POML files (.poml) or POML.zip files (.poml.zip) under the `On My iPad\SB Immersal\LocalContent` folder.
