#if SPIRAREBROWSER_HTTPSERVER_PRESENT
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
            var baseMapSettings = cesiumRectangleMap.BaseMapSettings;
            string baseMapSelectionOptions = "";
            if (baseMapSettings != null && baseMapSettings.BaseMaps.Count >= 1)
            {
                for (var i = 0; i < baseMapSettings.BaseMaps.Count; i++)
                {
                    var baseMapSetting = baseMapSettings.BaseMaps[i];
                    baseMapSelectionOptions += $@"
          <option value=""{i}"">{baseMapSetting.MapName}</option>"
;
                }
            }
            else
            {
                baseMapSelectionOptions = @"
          <option value=""""></option>
";
            }

            var baseMapSelectionHtml = $@"
      <div>
        <label for=""base-map"">Base map</label>
        <select name=""base-map"" id=""base-map"">
{baseMapSelectionOptions}
        </select>
      </div>
";

            var center = cesiumRectangleMap.Center;
            var latLon = $"{center.Latitude},{center.Longitude}";
            var height = center.EllipsoidalHeight.ToString();
            var scale = cesiumRectangleMap.Scale.ToString();

            var autoAdjustHeightChecked = cesiumRectangleMap.AutoAdjustCenterHeight ? "checked" : "";

            var html = $@"
<html>
  <head>
    <meta charset=""UTF-8"">
  </head>

  <body>
    <a href=""/"">Back</a>

    <h2>Settings Page for Map</h2>
    <form action=""/map/position"" method=""POST"" accept-charset=""utf-8"">
      {baseMapSelectionHtml}
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
        <label for=""auto-adjust-height"">Auto adjust height</label>
        <input type=""checkbox"" id=""auto-adjust-height"" name=""auto-adjust-height"" {autoAdjustHeightChecked} />
      </div>
      <div>
        <label for=""scale"">Map scale</label>
        <input name=""scale"" id=""scale"" value=""{scale}"" />
      </div>

      <input type=""hidden"" name=""request-source"" value=""form"">

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

                if (queries.TryGetValue("auto-adjust-height", out var autoAdjustHeightString))
                {
                    switch (autoAdjustHeightString)
                    {
                        case "on":
                            cesiumRectangleMap.AutoAdjustCenterHeight = true;
                            break;
                        case "off":
                            cesiumRectangleMap.AutoAdjustCenterHeight = false;
                            break;
                    }
                }
                else
                {
                    // When checkbox is not checked in the web form, the value is not included in query.
                    if (queries.TryGetValue("request-source", out var requestSource) && requestSource == "form")
                    {
                        cesiumRectangleMap.AutoAdjustCenterHeight = false;
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
#endif
