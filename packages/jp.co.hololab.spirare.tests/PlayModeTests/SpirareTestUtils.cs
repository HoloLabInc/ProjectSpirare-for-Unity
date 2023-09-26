using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityEngine.Assertions;
using UnityEngine.TestTools;

public static class SpirareTestUtils
{
    public static PomlLoaderSettings DefaultLoaderSettings
    {
        get
        {
            var settingsPath = "Packages/jp.co.hololab.spirare/Settings/DefaultPomlLoaderSettings.asset";
            var settings = (PomlLoaderSettings)AssetDatabase.LoadAssetAtPath(settingsPath, typeof(PomlLoaderSettings));
            return settings;
        }
    }

    public static void CreateMainCamera()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
    }

    public static void ClearScene()
    {
        var objects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var obj in objects)
        {
            if (obj.name != "Code-based tests runner")
            {
                UnityEngine.Object.Destroy(obj);
            }
        }
    }

    public static void AssertThatMeshIsVisible(GameObject gameObject, Material occlusionMaterial)
    {
        var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        Assert.That(meshRenderers.Length, Is.GreaterThan(0));

        foreach (var meshRenderer in meshRenderers)
        {
            AssertThatMeshIsVisible(meshRenderer, occlusionMaterial);
        }
    }

    public static void AssertThatMeshIsVisible(MeshRenderer meshRenderer, Material occlusionMaterial)
    {
        Assert.That(meshRenderer.gameObject.activeInHierarchy, Is.True);
        Assert.That(meshRenderer.enabled, Is.True);
        Assert.That(meshRenderer.material.shader, Is.Not.EqualTo(occlusionMaterial.shader));
    }

    public static void AssertThatMeshIsInvisible(GameObject gameObject)
    {
        var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
        foreach (var meshRenderer in meshRenderers)
        {
            AssertThatMeshIsInvisible(meshRenderer);
        }
    }

    public static void AssertThatMeshIsInvisible(MeshRenderer meshRenderer)
    {
        Assert.That(meshRenderer.gameObject.activeInHierarchy && meshRenderer.enabled, Is.False);
    }

    public static void AssertThatMeshIsOcclusion(GameObject gameObject, Material occlusionMaterial)
    {
        var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
        Assert.That(meshRenderers.Length, Is.GreaterThan(0));

        foreach (var meshRenderer in meshRenderers)
        {
            Assert.That(meshRenderer.material.shader, Is.EqualTo(occlusionMaterial.shader));
        }
    }
}
