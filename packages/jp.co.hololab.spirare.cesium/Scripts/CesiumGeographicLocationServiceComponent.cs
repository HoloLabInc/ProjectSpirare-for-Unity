using HoloLab.PositioningTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CesiumGeographicLocationServiceComponent : MonoBehaviour, IGeographicLocationService, ICardinalDirectionService
{
    private bool serviceEnabled;

    public event Action<GeographicLocation> OnLocationUpdated;
    public event Action<CardinalDirection> OnDirectionUpdated;

    private void Update()
    {
        if (serviceEnabled)
        {
            // TODO implement
        }
    }

    public void StartService()
    {
        _ = StartServiceAsync();
    }

    public Task<(bool ok, Exception exception)> StartServiceAsync()
    {
        serviceEnabled = true;
        return Task.FromResult<(bool, Exception)>((true, null));
    }

    public void StopService()
    {
        _ = StopServiceAsync();
    }

    public Task<(bool ok, Exception exception)> StopServiceAsync()
    {
        serviceEnabled = false;
        return Task.FromResult<(bool, Exception)>((true, null));
    }
}
