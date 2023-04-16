using Cysharp.Threading.Tasks;
using HoloLab.PositioningTools;
using HoloLab.PositioningTools.CoordinateSystem;
using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class WebPomlClient : PomlClientBase
    {
        [SerializeField]
        private string url;

        // [SerializeField]
        // private bool sendGeolocationWithHeader = false;

        private Camera mainCamera;

        private static HttpClient httpClient = new HttpClient();


        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private async void Start()
        {
            if (string.IsNullOrEmpty(url) == false)
            {
                await Task.Delay(100);
                await LoadAsync(url);
            }
        }

        protected override async Task<string> GetContentXml(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            var geoheader = await GetGeolocationHeaderAsync();
            if (geoheader.Geolocation != null)
            {
                request.Headers.Add("Geolocation", geoheader.Geolocation);
            }
            if (geoheader.Geoheading != null)
            {
                request.Headers.Add("Geoheading", geoheader.Geoheading);
            }

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
            return await response.Content.ReadAsStringAsync();
        }


        // TODO: Move the code to the Positioning Tools
        private async UniTask<(bool Success, CardinalDirection CardinalDirection)> GetCardinalDirectionAsync(int timeoutMilliseconds)
        {
            var cardinalDirectionService = FindObjectOfType<CardinalDirectionServiceComponent>();
            if (cardinalDirectionService == null)
            {
                return (false, new CardinalDirection());
            }

            var completionSource = new UniTaskCompletionSource<CardinalDirection>();
            cardinalDirectionService.OnDirectionUpdated +=
                direction => completionSource.TrySetResult(direction);

            var result = await UniTask.WhenAny(completionSource.Task, UniTask.Delay(timeoutMilliseconds));
            return result;
        }

        private async UniTask<(bool Success, GeographicLocation GeographicLocation)> GetGeographicLocationAsync(int timeoutMilliseconds)
        {
            var geographicLocationService = FindObjectOfType<GeographicLocationServiceComponent>();
            if (geographicLocationService == null)
            {
                return (false, new GeographicLocation());
            }

            var completionSource = new UniTaskCompletionSource<GeographicLocation>();
            geographicLocationService.OnLocationUpdated +=
                location => completionSource.TrySetResult(location);

            var result = await UniTask.WhenAny(completionSource.Task, UniTask.Delay(timeoutMilliseconds));
            return result;
        }

        /// <summary>
        /// Gets the header information of the device position.
        /// </summary>
        /// <param name="geolocation"></param>
        /// <returns></returns>
        private async Task<(string Geolocation, string Geoheading)> GetGeolocationHeaderAsync()
        {
            string geolocationHeader = null;
            string geoheadingHeader = null;

            var timeoutMilliseconds = 10 * 1000;

            // Get geographic location.
            var geographicLocationResult = await GetGeographicLocationAsync(timeoutMilliseconds);

            if (geographicLocationResult.Success)
            {
                geolocationHeader = GeographicLocationToGeolocationHeader(geographicLocationResult.GeographicLocation);
            }
            else
            {
                // TODO: Handle for map display.
                var coordinateManager = CoordinateManager.Instance;
                if (coordinateManager != null)
                {
                    var worldBinding = coordinateManager.LatestWorldBinding;
                    if (worldBinding != null)
                    {
                        // TODO: Refactor.
                        var cameraPosition = mainCamera.transform.position;

                        var boundPoseInUnity = worldBinding.ApplicationPose;
                        var enuPositionVector = Quaternion.Inverse(boundPoseInUnity.rotation) * (cameraPosition - boundPoseInUnity.position);
                        var enuPosition = new EnuPosition(enuPositionVector.x, enuPositionVector.z, enuPositionVector.y);

                        // Convert to latitude and longitude.
                        var geodeticPosition =
                            GeographicCoordinateConversion.EnuToGeodetic(enuPosition, worldBinding.GeodeticPose.GeodeticPosition);

                        geolocationHeader = $"{geodeticPosition.Latitude},{geodeticPosition.Longitude},{geodeticPosition.EllipsoidalHeight}";
                    }
                }
            }

            // Get cardinal direction.
            var cardinalDirectionResult = await GetCardinalDirectionAsync(timeoutMilliseconds);
            if (cardinalDirectionResult.Success)
            {
                geoheadingHeader = cardinalDirectionResult.CardinalDirection.HeadingDegrees.ToString();
            }

            return (geolocationHeader, geoheadingHeader);
        }

        private static string GeographicLocationToGeolocationHeader(GeographicLocation geographicLocation)
        {
            return $"{geographicLocation.Latitude},{geographicLocation.Longitude},{geographicLocation.Height}";
        }
    }
}
