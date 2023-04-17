# SpirareBrowser-HoloLens-Immersal

This project is a browser application for Spirare.

## Target Devices

HoloLens 2

## Position Alignment Method

Vuforia Area Target

## Project Setup

### Import Vuforia Engine
Download the unitypackage for installing Vuforia Engine from the [developer portal](https://developer.vuforia.com/downloads/SDK).

Open the unity project in UnityEditor and import the package.

### Set Vuforia license key
Select `Assets\Resources\VuforiaLicenseKeySettings` and enter your Vuforia license key.  
If `VuforiaLicenseKeySettings` does not exist, play the `SpirareBrowser-HoloLens` scene once.

<img width="640" alt="Vuforia License settings" src="https://user-images.githubusercontent.com/4415085/230548660-7d367480-27a6-4cf4-827c-7b960c197378.png">

## Usage

### Place Vuforia map files

To create a folder for placing the map files, please launch `Spirare Browser VAT` once.  
Open `Device Portal` and place the map files (.dat, .xml) under the `LocalState\AreaTargetData` folder.

<img width="640" alt="Content loading page" src="https://user-images.githubusercontent.com/4415085/230549728-818096a9-0350-4f4f-8a1d-624f6920a7a2.png">

The map files are loaded when the app starts, so you will need to restart the app.

### Load content using PC
With the Spirare Browser app running on HoloLens, access `http://<HoloLens IP address>:8080` from your PC's browser.  
A page similar to the one below will be displayed.

<img width="640" alt="Content loading page" src="https://user-images.githubusercontent.com/4415085/230544381-522260c0-b6ff-4da2-89df-88c67ec966fd.png">

Enter the URL of the content you want to load in the Content URL field, and click the "Load" button.

By checking the "Auto Reload" option, the content will be reloaded every 5 seconds.  
This is useful when you want to check the placement of content while using the Spirare Editor.

### Load content
In the hand menu, enter the URL of the content you want to load and press the "Load" button.

<img width="480" alt="Content loading hand menu" src="https://user-images.githubusercontent.com/4415085/230544480-6fd7013f-794d-40a9-8fd8-81d98e46da8b.png">

