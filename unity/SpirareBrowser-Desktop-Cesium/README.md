# SpirareBrowser-Desktop-Cesium

This is a browser application project for Spirare.

<img width="480" alt="Spirare Browser Desktop Demo" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/3217887d-a368-41de-b093-8e858fa06a58">

## Target Devices

Desktop computers

## Common Project Setup

### Import Starter Assets

Please import the Starter Assets from the link below.  
https://assetstore.unity.com/packages/essentials/starter-assets-third-person-character-controller-urp-196526

### Import TextMesh Pro

Open the `Assets\App\Scenes\SpirareBrowserCesium_Photorealistic3DTilesTerrain.unity` in the Unity Editor.  
When the 'TMP Importer' dialog appears, click on 'Import TMP Essentials'.

<img width="480" alt="TMP Importer" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/34afda65-262a-40ea-aac9-628795d7af5e">

### (Optional) Customize Cesium for Unity to enable changing point cloud size

If you want to display point cloud, it is recommended to make the following changes to allow for adjustments to the point size of point cloud.

#### Move Cesium for Unity package

Move the `Library\PackageCache\com.cesium.unity@<version>` folder to the `Packages` folder in order to enable changes.

#### Change `CesiumPointCloudShading.cs`

Edit the `CesiumPointCloudShading.cs` file in the `Packages\com.cesium.unity@<version>\Runtime` folder.

```diff
        public float baseResolution
        {
            get => this._baseResolution;
            set => this._baseResolution = Mathf.Max(value, 0.0f);
        }
+
+       [SerializeField]
+       private float _pointSize = 0;
+
+       public float pointSize
+       {
+           get => this._pointSize;
+           set => this._pointSize = Mathf.Max(value, 0.0f);
+       }
    }
}
```

### Change `CesiumPointCloudRenderer.cs`

Edit the `UpdateAttenuationParameters` method in the `CesiumPointCloudRenderer.cs` file in the `Packages\com.cesium.unity@<version>\Runtime` folder.

```diff
            this._attenuationParameters =
-               new Vector4(maximumPointSize, geometricError, depthMultplier, 0);
+               new Vector4(maximumPointSize, geometricError, depthMultplier, pointCloudShading.pointSize);
```

### Change `Cesium3DTilesetEditor.cs`

Edit the `DrawPointCloudShadingProperties` method in the `Cesium3DTilesetEditor.cs` file in the `Packages\com.cesium.unity@<version>\Editor` folder.

```diff
            float baseResolutionValue = EditorGUILayout.FloatField(
                baseResolutionContent,
                baseResolutionProperty.floatValue);

+           SerializedProperty pointSizeProperty =
+               this._pointCloudShading.FindPropertyRelative("_pointSize");
+           GUIContent pointSizeContent = new GUIContent(
+               "Point Size",
+               "");
+           float pointSizeValue = EditorGUILayout.FloatField(
+               pointSizeContent,
+               pointSizeProperty.floatValue);
+
           if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(
                    this._tileset,
                    "Modified Point Cloud Shading in " + this._tileset.gameObject.name);
                this._tileset.pointCloudShading.attenuation = attenuationValue;
                this._tileset.pointCloudShading.geometricErrorScale = geometricErrorScaleValue;
                this._tileset.pointCloudShading.maximumAttenuation = maximumAttenuationValue;
                this._tileset.pointCloudShading.baseResolution = baseResolutionValue;
+               this._tileset.pointCloudShading.pointSize = pointSizeValue;
                // Force the scene view to repaint so that the changes are immediately reflected.
                SceneView.RepaintAll();
            }
```

### Chagne `CesiumPointCloudShading.hlsl`

Edit the `Vertex` function in the `CesiumPointCloudShading.hlsl` file in the `Packages\com.cesium.unity@<version>\Runtime\Resources` folder.

```diff
-	float pointSize = min((geometricError / depth) * depthMultiplier, maximumPointSize);
+	float pointSizeInSpace = _attenuationParameters.w;
+	float pointSize = pointSizeInSpace * 2000 / depth;
```

## a. Project Setup for Google Photorealistic 3D Tiles

### Get Google API Key

Please obtain the API key, referring to the link below.  
https://developers.google.com/maps/documentation/tile/get-api-key?hl=en

### Set API Key

Select `Assets\Resources\Spirare\TilesetSourceSettings` and enter the following URL into the URL field.

`https://tile.googleapis.com/v1/3dtiles/root.json?key=<Your API Key>`

<img width="428" alt="Tileset API Key Settings" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/d34606d9-ff28-4005-8272-ad8d6c8ae9e9">

### Build settings

Select `App/Scenes/SpirareBrowserCesium_Photorealistic3DTilesTerrain` in the build settings and then build the project.

<img width="480" alt="Build settings image" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/d1616b1b-b7d1-4bec-ae15-881a2653ba30">

## b. Project Setup for PLATEAU terrain

### Get Cesium Ion Access Token

Please obtain a Cesium ion access token from the following website.

https://github.com/Project-PLATEAU/plateau-streaming-tutorial/blob/main/terrain/plateau-terrain-streaming.md#21-%E3%82%A2%E3%82%AF%E3%82%BB%E3%82%B9%E3%83%88%E3%83%BC%E3%82%AF%E3%83%B3%E5%8F%8A%E3%81%B3%E3%82%A2%E3%82%BB%E3%83%83%E3%83%88id

### Set Cesium Ion Access Token

In the Unity Editor, select **Cesium** > **Cesium**.
Select the **Token** button and enter your access token for Cesium ion.

<img width="480" alt="Cesium ion token" src="https://user-images.githubusercontent.com/4415085/230542155-3748e190-97dd-4b94-b641-c3fc566400c3.png">

### Build settings

Select `App/Scenes/SpirareBrowserCesium_PLATEAU_Terrain` in the build settings and then build the project.

<img width="480" alt="Build settings image" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/98ffd417-ed92-4bf6-bad8-1233e7044fbc">

## Usage

### Menu

Please press the `Esc` key to open the menu.

<img width="469" alt="App menu" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/23b87ec8-3e07-4274-bca8-ae7557a581e6">

#### Teleport

Enter the latitude and longitude in the format `latitude, longitude`, and press the `Teleport` button to move to that location.  
You can copy this format of latitude and longitude from the menu that appears when you right-click on Google Maps.

#### Load POML

To load POML, enter the URL into the input field and press the `Load POML` button.
The content will be loaded.

In addition, you can also load a `.poml` or `.poml.zip` file by dragging and dropping it from your local machine into the window.

### Movement

Please close the menu using the `Esc` key when you want to move the charactor.

| Key   | Action        |
| ----- | ------------- |
| W     | Move Forward  |
| A     | Move Left     |
| S     | Move Backward |
| D     | Move Right    |
| Shift | Speed Up      |
| Space | Jump          |
