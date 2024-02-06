# SpirareBrowser-Quest-3DMaps

This project is a browser application for Spirare that is capable of displaying a 3D map and overlaying POML data on it.

<img width="480" alt="Spirare Browser Qeust 3DMaps Demo" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/c65bafcd-2058-4510-9d19-7f239d6c9f76">

## Target Devices

Meta Quest 3

## Project Setup

### Get Google API Key

Please obtain the API key, referring to the link below.  
https://developers.google.com/maps/documentation/tile/get-api-key?hl=en

### Set API Key

Select `Assets\Resources\Spirare\GooglePhotorealistic3DTilesSourceSettings` and enter the following URL into the URL field.

`https://tile.googleapis.com/v1/3dtiles/root.json?key=<Your API Key>`

<img width="428" alt="Tileset API Key Settings" src="https://github.com/HoloLabInc/ProjectSpirare-for-Unity/assets/4415085/47df6956-0be4-40c6-8c95-a440e8c2ff21">

### Build settings

Switch the platform to Android and build the project.

## Usage

### How to Operate

You can change the position and size of map objects by manipulating the sides of the map base with the controller. By operating the top surface of the map with the controller, you can move the center of the map and zoom in or out.
