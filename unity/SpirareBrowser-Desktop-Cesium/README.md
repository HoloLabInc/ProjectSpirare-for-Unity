# SpirareBrowser-Desktop-Cesium

This is a browser application project for Spirare.

[Spirare Browser Desktop Demo](https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/3217887d-a368-41de-b093-8e858fa06a58)

## Target Devices

Desktop computer

## Common Project Setup

### Import Starter Assets

Please import the Starter Assets from the link below.  
https://assetstore.unity.com/packages/essentials/starter-assets-third-person-character-controller-urp-196526

### Import TextMesh Pro

Open the `Assets\App\Scenes\SpirareBrowserCesium_3DTileTerrain.scene` in the Unity Editor.  
When the 'TMP Importer' dialog appears, click on 'Import TMP Essentials'.

## a. Project Setup for Google Photorealistic 3D Tiles

### Get Google API Key

Please obtain the API key, referring to the link below.  
https://developers.google.com/maps/documentation/tile/get-api-key?hl=en

### Open scene

Open the `Assets\App\Scenes\SpirareBrowserCesium_3DTileTerrain.scene` in the Unity Editor.

### Set API Key

Select `Assets\Resources\Spirare\TilesetSourceSettings` and enter the following URL into the URL field.

`https://tile.googleapis.com/v1/3dtiles/root.json?key=<Your API Key>`

<img width="575" alt="image" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/785b0d8e-6bce-44d5-853b-f620321a7c8b">

### Build settings

Select `App/Scenes/SpirareBrowserCesium_3DTileTerrain` in the build settings and then build the project.

<img width="480" alt="Build settings image" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/8038ef8a-382f-4c5a-8c0e-642216f600db">

## b. Project Setup for PLATEAU terrain

### Get Cesium Ion Access Token

Please obtain a Cesium ion access token from the following website.

https://github.com/Project-PLATEAU/plateau-streaming-tutorial/blob/main/terrain/plateau-terrain-streaming.md#21-%E3%82%A2%E3%82%AF%E3%82%BB%E3%82%B9%E3%83%88%E3%83%BC%E3%82%AF%E3%83%B3%E5%8F%8A%E3%81%B3%E3%82%A2%E3%82%BB%E3%83%83%E3%83%88id

### Open scene

Open `Assets\App\Scenes\SpirareBrowserCesium_3DTileTerrain.scene` with Unity Editor.

In the Unity Editor, select **Cesium** > **Cesium**.
Select the **Token** button and enter your access token for Cesium ion.

<img width="480" alt="Cesium ion token" src="https://user-images.githubusercontent.com/4415085/230542155-3748e190-97dd-4b94-b641-c3fc566400c3.png">

### Build settings

Select `App/Scenes/SpirareBrowserCesium_PLATEAU_Terrain` in the build settings and then build the project.

<img width="480" alt="Build settings image" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/6bc341f7-f1d9-4da4-b6e3-4c36121b9016">
