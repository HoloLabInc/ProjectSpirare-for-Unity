# SpirareBrowser-visionOS-3DMaps

This project is a browser application for Spirare that is capable of displaying a 3D map and overlaying POML data on it.

<img width="480" alt="Spirare Browser visionOS 3DMaps Demo" src="https://github.com/user-attachments/assets/be87fde1-22a5-4ce0-841b-ba4540c6b0d1">

## Target Devices

Apple Vision Pro

## Project Setup

### Edit Cesium for Unity

#### Move Cesium for Unity package

Move the `Library/PackageCache/com.cesium.unity@x.x.x` folder into the `Packages` folder.

#### Change the codes

Change the codes by executing the following commands.

```bash
cd Packages/com.cesium.unity@1.11.1/Runtime/generated/Reinterop/Reinterop.RoslynSourceGenerator
sed -i '' 's/UNITY_IOS/(UNITY_IOS || UNITY_VISIONOS)/g' *.cs
sed -i '' 's/makeNoLongerReadable != 0/false/g' ReinteropInitializer.cs
```

#### Edit `CesiumSelectTexCoords.shadersubgraph` in `Cesium for Unity/Runtime/Resources` folder

Due to a conversion error to MaterialX in `CesiumSelectTexCoords.shadersubgraph`, modify it as shown in the images.

Before:

<img width="480" src="https://github.com/user-attachments/assets/63e05475-ac73-40a8-b426-ad20fc098baa">

After:

<img width="480" src="https://github.com/user-attachments/assets/107a0de5-b70b-4b30-a494-fad279911cfd">

#### Edit `CesiumUnlitTilesetShader.shadergraph` in `Cesium for Unity/Runtime/Resources` folder

In the **Graph Settings**, make the following changes:

- Change Built-In Material to Unlit
- Check the box for Built-In Alpha Clipping
- Check the box for Universal Alpha Clipping

<img width="360" src="https://github.com/user-attachments/assets/74f839be-0b6d-4789-b5c2-46f20ee951c4">

Delete **Smoothness** from **Fragment**

<img width="240" src="https://github.com/user-attachments/assets/c902b852-3c02-41e8-96d4-67bc15fbef4c">

### Get Google API Key

Please obtain the API key, referring to the link below.  
https://developers.google.com/maps/documentation/tile/get-api-key?hl=en

### Set API Key

Select `Assets\Resources\Spirare\GooglePhotorealistic3DTilesSourceSettings` and enter the following URL into the URL field.

`https://tile.googleapis.com/v1/3dtiles/root.json?key=<Your API Key>`

<img width="428" alt="Tileset API Key Settings" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/47df6956-0be4-40c6-8c95-a440e8c2ff21">

### Build settings

Switch the platform to visionOS and build the project.

## Usage

### How to operate

You can change the position of the map object by manipulating the sides of the map base.
By manipulating the top surface of the map, you can move the center of the map and zoom in or out.

### Load POML from URL

Access `http://<Apple Vision Pro IP Address>:8080` in your PC's browser to open the management page.

<img width="480" alt="App management page" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/1c2b97a3-fb65-4256-b950-124c5e2dc7a0">

Load the POML file by entering the URL in the input field and pressing the `Load` button.