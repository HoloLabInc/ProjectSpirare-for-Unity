using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FilePathUtilityTests
{
    [TestCase("")]
    [TestCase("http://example.net/poml")]
    [TestCase("/test/poml")]
    [TestCase("C:\\test\\poml")]
    public void GetAbsolutePath_PathIsAbsoulteUrl_ReturnPath(string basePath)
    {
        var path = "http://example.com/test";
        var absolutePath = FilePathUtility.GetAbsolutePath(path, basePath);

        Assert.That(absolutePath, Is.EqualTo(path));
    }

    [TestCase("http://example.net/dir/poml", "test.jpg", "http://example.net/dir/test.jpg")]
    [TestCase("http://example.net/dir/poml", "./test.jpg", "http://example.net/dir/test.jpg")]
    [TestCase("http://example.net/dir/poml", "../test.jpg", "http://example.net/test.jpg")]
    public void GetAbsolutePath_PathIsRelativeUrl_CombinePath(string basePath, string relativePath, string expectedPath)
    {
        var absolutePath = FilePathUtility.GetAbsolutePath(relativePath, basePath);
        Assert.That(absolutePath, Is.EqualTo(expectedPath));
    }

    [TestCase("http://example.net/poml", "/test.jpg", "http://example.net/test.jpg")]
    [TestCase("http://example.net/dir/poml", "/test.jpg", "http://example.net/test.jpg")]
    [TestCase("http://example.net/poml", "/subdir/test.jpg", "http://example.net/subdir/test.jpg")]
    [TestCase("http://example.net/dir/poml", "/subdir/test.jpg", "http://example.net/subdir/test.jpg")]
    public void GetAbsolutePath_PathIsDomainRelativeUrl_CombinePath(string basePath, string relativePath, string expectedPath)
    {
        var absolutePath = FilePathUtility.GetAbsolutePath(relativePath, basePath);
        Assert.That(absolutePath, Is.EqualTo(expectedPath));
    }

    [TestCase("C:\\test\\poml", "test.jpg", "file://C:\\test\\test.jpg")]
    [TestCase("C:\\test\\poml", "./test.jpg", "file://C:\\test\\test.jpg")]
    [TestCase("C:\\test\\poml", "../test.jpg", "file://C:\\test.jpg")]
    public void GetAbsolutePath_PathIsRelative_CombinePath(string basePath, string relativePath, string expectedPath)
    {
        var absolutePath = FilePathUtility.GetAbsolutePath(relativePath, basePath);
        Assert.That(absolutePath, Is.EqualTo(expectedPath));
    }
}
