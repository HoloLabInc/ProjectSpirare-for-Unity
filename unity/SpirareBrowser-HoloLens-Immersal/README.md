# SpirareBrowser-HoloLens-Immersal

This project is a browser application for Spirare.

## Target Devices

HoloLens 2

## Position Alignment Method

Immersal

## Usage

### Create a configuration file

Please create a configuration file named `immersal-settings.yaml` with the following content.  
You can obtain the developer token from the [Immersal Developer Portal](https://developers.immersal.com).

```
token: <developer token>
map_ids:
  - <map id 1>
  - <map id 2>
  ...
```

For example, if the developer token is "abc" and you want to align the position with Map ID "123", it would look like this:

```
token: abc
map_ids:
  - 123
```

### Place the configuration file

To create a folder for placing the configuration file, please launch `Spirare Browser Immersal` once.  
Open the HoloLens Developer Portal, and place the `immersal-settings.yaml` file inside the `LocalState` folder of `SpirareBrowserImmersal`.

<img width="640" alt="Upload configuration file" src="https://user-images.githubusercontent.com/4415085/230543769-b106a9c5-8cb4-4fc1-85c5-fe684eb68a15.png">

The configuration file is loaded when the app starts, so you will need to restart the app.

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
