using Cysharp.Threading.Tasks;
using HoloLab.PositioningTools;
using HoloLab.PositioningTools.CoordinateSystem;
using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace HoloLab.Spirare
{
    public sealed class WebPomlClient : PomlClientBase
    {
        [SerializeField]
        private string url;

        // [SerializeField]
        // private bool sendGeolocationWithHeader = false;

        private Camera mainCamera;

        private CancellationTokenSource nextReloadTokenSource;

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
            nextReloadTokenSource?.Cancel();
            nextReloadTokenSource = null;

            var request = UnityWebRequest.Get(path);

            // Set request headers
            var (geolocation, geoheading) = await GetGeolocationHeaderAsync();
            SetRequestHeaders(request, new Dictionary<string, string>()
            {
                { "Geolocation", geolocation },
                { "Geoheading", geoheading },
            });

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new HttpRequestException(request.error);
            }

            var contentString = request.downloadHandler.text;
            var responseHeaders = request.GetResponseHeaders();

            ReloadWithRefreshHeader(responseHeaders).Forget();

            request.Dispose();
            return contentString;
        }

        private void SetRequestHeaders(UnityWebRequest request, Dictionary<string, string> headers)
        {
            foreach (var pair in headers)
            {
                if (pair.Value != null)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }
            }
        }

        private async UniTask ReloadWithRefreshHeader(Dictionary<string, string> responseHeaders)
        {
            if (responseHeaders.TryGetValue("Refresh", out var refreshHeader) == false)
            {
                return;
            }

            await ReloadWithRefreshHeader(refreshHeader);
        }

        private async UniTask ReloadWithRefreshHeader(string refreshHeader)
        {
            var refreshHeaderParts = refreshHeader.Split(';');
            if (refreshHeaderParts.Length == 1)
            {
                var delayString = refreshHeaderParts[0];
                if (float.TryParse(delayString, out var delaySecond))
                {
                    nextReloadTokenSource = CancellationTokenSource.CreateLinkedTokenSource(gameObject.GetCancellationTokenOnDestroy());
                    var token = nextReloadTokenSource.Token;
                    await UniTask.Delay(TimeSpan.FromSeconds(delaySecond), cancellationToken: token);

                    await ReloadAsync();
                }
            }
            else
            {
                Debug.LogWarning("Redirect with Refresh header is not supported");
            }
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
