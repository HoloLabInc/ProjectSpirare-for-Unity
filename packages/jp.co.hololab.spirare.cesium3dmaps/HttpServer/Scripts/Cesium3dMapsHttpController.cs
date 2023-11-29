using HoloLab.PositioningTools.GeographicCoordinate;
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
        private CesiumRectangleMap cesiumRectangleMap;

        [Route("/map")]
        public string Index()
        {
            var center = cesiumRectangleMap.Center;
            var latLon = $"{center.Latitude},{center.Longitude}";
            var height = center.EllipsoidalHeight.ToString();
            var scale = cesiumRectangleMap.Scale.ToString();

            var html = $@"
<html>
  <body>
    <a href=""/"">Back</a>

    <h2>Settings Page for Map</h2>
    <form action=""/map/position"" method=""POST"" accept-charset=""utf-8"">
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
        <label for=""scale"">Map scale</label>
        <input name=""scale"" id=""scale"" value=""{scale}"" />
      <div>
        <button type=""submit"">Update</button>
      </div>
    </form>
  </body>
</html>
";
            return html;
        }

        [Route("/map/position")]
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
                        var center = cesiumRectangleMap.Center;
                        cesiumRectangleMap.Center = new GeodeticPosition(latitude, longitude, center.EllipsoidalHeight);
                    }
                }

                if (queries.TryGetValue("height", out var heightString))
                {
                    if (TryParseFloat(heightString, out var height))
                    {
                        var center = cesiumRectangleMap.Center;
                        cesiumRectangleMap.Center = new GeodeticPosition(center.Latitude, center.Longitude, height);
                    }
                }

                if (queries.TryGetValue("scale", out var scaleString))
                {
                    if (TryParseFloat(scaleString, out var scale))
                    {
                        cesiumRectangleMap.Scale = scale;
                    }
                }
            }

            response.Redirect("/map");
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
