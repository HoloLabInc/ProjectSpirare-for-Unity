using CesiumForUnity;
using HoloLab.Spirare.Browser.HttpServer;
using HoloLab.UniWebServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps.HttpServer
{
    public class Cesium3dMapsHttpController : MonoBehaviour, IHttpController
    {
        [SerializeField]
        private CesiumGeoreference cesiumGeoreference;

        [SerializeField]
        private Transform cameraOriginTransform;

        private const string latitudeSaveKey = "GeodeticSettingsHttpController_latitude";
        private const string longitudeSaveKey = "GeodeticSettingsHttpController_longitude";
        private const string heightSaveKey = "GeodeticSettingsHttpController_height";

        private void Start()
        {
            var savedLatitudeString = PlayerPrefs.GetString(latitudeSaveKey);
            var savedLongitudeString = PlayerPrefs.GetString(longitudeSaveKey);
            var savedHeight = PlayerPrefs.GetFloat(heightSaveKey);

            var latitude = cesiumGeoreference.latitude;
            var longitude = cesiumGeoreference.longitude;
            var height = cesiumGeoreference.height;

            if (double.TryParse(savedLatitudeString, out var savedLatitude))
            {
                latitude = savedLatitude;
            }
            if (double.TryParse(savedLongitudeString, out var savedLongitude))
            {
                longitude = savedLongitude;
            }
            if (savedHeight != 0)
            {
                height = savedHeight;
            }

            cesiumGeoreference.SetOriginLongitudeLatitudeHeight(longitude, latitude, height);
        }

        [Route("/geodetic-settings")]
        public string Index()
        {
            var latLon = $"{cesiumGeoreference.latitude},{cesiumGeoreference.longitude}";
            var height = cesiumGeoreference.height;
            var html = $@"
<html>
  <body>
    <a href=""/"">Back</a>

    <h2>Settings Page for Geodetic Position</h2>
    <form action=""/geodetic-settings/position"" method=""POST"" accept-charset=""utf-8"">
      <div>
        <label for=""latlon"">Latitude Longitude</label>
        <input
          name=""latlon""
          id=""latlon""
          value=""{latLon}""
          style=""width: 280px""
        />
      </div>
      <div>
        <label for=""height"">Ellipsoidal height</label>
        <input name=""height"" id=""height"" value=""{height}"" />
      </div>
      <div>
        <button type=""submit"">Update</button>
      </div>
    </form>
  </body>
</html>
";
            return html;
        }

        [Route("/geodetic-settings/position")]
        public async Task ChangePosition(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod == "POST")
            {
                var reader = new StreamReader(request.InputStream);
                var body = await reader.ReadToEndAsync();

                var queries = HttpQueryParser.ParseQueryString(body);

                if (queries.TryGetValue("latlon", out var latLonString))
                {
                    if (TryParseLatitudeLongitude(latLonString, out var latitude, out var longitude))
                    {
                        var height = cesiumGeoreference.height;
                        cesiumGeoreference.SetOriginLongitudeLatitudeHeight(longitude, latitude, height);

                        PlayerPrefs.SetString(latitudeSaveKey, latitude.ToString());
                        PlayerPrefs.SetString(longitudeSaveKey, longitude.ToString());
                        PlayerPrefs.Save();
                    }
                }

                if (queries.TryGetValue("height", out var heightString))
                {
                    if (TryParseFloat(heightString, out var height))
                    {
                        var longitude = cesiumGeoreference.longitude;
                        var latitude = cesiumGeoreference.latitude;
                        cesiumGeoreference.SetOriginLongitudeLatitudeHeight(longitude, latitude, height);

                        PlayerPrefs.SetFloat(heightSaveKey, height);
                    }
                }

                cameraOriginTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            response.Redirect("/geodetic-settings");
        }

        private static bool TryParseFloat(string text, out float value)
        {
            try
            {
                var dt = new DataTable();
                value = (float)Convert.ToDouble(dt.Compute(text, ""));
                return true;
            }
            catch (Exception)
            {
                value = 0;
                return false;
            }
        }

        private static bool TryParseLatitudeLongitude(string text, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            var separator = new char[] { ',', ' ' };
            var tokens = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length <= 1)
            {
                return false;
            }

            return double.TryParse(tokens[0], out latitude) && double.TryParse(tokens[1], out longitude);
        }
    }
}
