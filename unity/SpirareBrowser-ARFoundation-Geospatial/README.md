# SpirareBrowser-ARFoundation-Geospatial

This project is a browser application for Spirare.

## Target Devices

ARFoundation supported devices

## Position Alignment Method

ARCore Geospatial API

## Project Setup

Create an ARCore API key.  
Please refer to [the documentation](https://developers.google.com/ar/develop/unity-arf/geospatial/enable-android#api_key_authorization) for more information.

Open the unity project.  
Select **Edit** > **ProjectSettings** > **XR Plug-in Management** > **ARCore Extensions** and paste the API key in the API Key field.

<img width="480" alt="ARCore Extensions API key settings" src="https://user-images.githubusercontent.com/4415085/223650258-8157f411-1624-459c-976b-58edb4504569.png">

## Usage

TODO: screenshot

### Alignment

Press the **Align** button.  
By doing this, the alignment will be performed based on the geodetic position obtained from the ARCore Geospatial API.

### Loading POML from web server
Please enter the URL where the POML is being served in the input field and press the **Load** button.

### Loading POML from local files
 
To create a folder for placing the map files, please launch the application once.  

### Android
Place POML files (.poml) or POML.zip files (.poml.zip) under the `Android\data\jp.co.hololab.spirarebrowsergeospatial\files\LocalContent` folder.

### iOS
Open the `Files` app and place POML files (.poml) or POML.zip files (.poml.zip) under the `On My iPad\SB Geospatial\LocalContent` folder.