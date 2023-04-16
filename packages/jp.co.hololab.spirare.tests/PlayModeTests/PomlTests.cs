using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PomlTests
{
    [TestCase(PomlDisplayType.Visible, PomlDisplayType.Visible, PomlDisplayType.Visible, PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.Visible, PomlDisplayType.Visible, PomlDisplayType.Occlusion, PomlDisplayType.Occlusion)]
    [TestCase(PomlDisplayType.Visible, PomlDisplayType.Visible, PomlDisplayType.None, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.Occlusion, PomlDisplayType.Occlusion, PomlDisplayType.Visible, PomlDisplayType.Occlusion)]
    [TestCase(PomlDisplayType.Occlusion, PomlDisplayType.Occlusion, PomlDisplayType.Occlusion, PomlDisplayType.Occlusion)]
    [TestCase(PomlDisplayType.Occlusion, PomlDisplayType.Occlusion, PomlDisplayType.None, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.None, PomlDisplayType.None, PomlDisplayType.Visible, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.None, PomlDisplayType.None, PomlDisplayType.Occlusion, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.None, PomlDisplayType.None, PomlDisplayType.None, PomlDisplayType.None)]
    public void DisplayInHierarchy(
        PomlDisplayType parentDisplay, PomlDisplayType parentDisplayInHierarchy,
        PomlDisplayType childDisplay, PomlDisplayType childDisplayInHierarchy)
    {
        var parentElement = new PomlEmptyElement()
        {
            Display = parentDisplay,
        };

        var childElement = new PomlEmptyElement()
        {
            Display = childDisplay,
            Parent = parentElement,
        };

        parentElement.Children = new PomlElement[] { childElement };

        Assert.That(parentElement.DisplayInHierarchy, Is.EqualTo(parentDisplayInHierarchy));
        Assert.That(childElement.DisplayInHierarchy, Is.EqualTo(childDisplayInHierarchy));
    }

    [TestCase(PomlDisplayType.Visible, PomlArDisplayType.SameAsDisplay, PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.Occlusion, PomlArDisplayType.SameAsDisplay, PomlDisplayType.Occlusion)]
    [TestCase(PomlDisplayType.None, PomlArDisplayType.SameAsDisplay, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.Visible, PomlArDisplayType.Visible, PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.Occlusion, PomlArDisplayType.Visible, PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.None, PomlArDisplayType.Visible, PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.Visible, PomlArDisplayType.Occlusion, PomlDisplayType.Occlusion)]
    [TestCase(PomlDisplayType.Occlusion, PomlArDisplayType.Occlusion, PomlDisplayType.Occlusion)]
    [TestCase(PomlDisplayType.None, PomlArDisplayType.Occlusion, PomlDisplayType.Occlusion)]
    [TestCase(PomlDisplayType.Visible, PomlArDisplayType.None, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.Occlusion, PomlArDisplayType.None, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.None, PomlArDisplayType.None, PomlDisplayType.None)]
    public void ArDisplayInHierarchy_IsolatedElement(PomlDisplayType display, PomlArDisplayType arDisplay, PomlDisplayType arDisplayInHierarchy)
    {
        var element = new PomlEmptyElement()
        {
            Display = display,
            ArDisplay = arDisplay
        };

        Assert.That(element.ArDisplayInHierarchy, Is.EqualTo(arDisplayInHierarchy));
    }

    [TestCase(PomlArDisplayType.Visible, PomlArDisplayType.Visible, PomlDisplayType.Visible)]
    [TestCase(PomlArDisplayType.Occlusion, PomlArDisplayType.Visible, PomlDisplayType.Occlusion)]
    [TestCase(PomlArDisplayType.None, PomlArDisplayType.Visible, PomlDisplayType.None)]
    [TestCase(PomlArDisplayType.Visible, PomlArDisplayType.Occlusion, PomlDisplayType.Occlusion)]
    [TestCase(PomlArDisplayType.Occlusion, PomlArDisplayType.Occlusion, PomlDisplayType.Occlusion)]
    [TestCase(PomlArDisplayType.None, PomlArDisplayType.Occlusion, PomlDisplayType.None)]
    [TestCase(PomlArDisplayType.Visible, PomlArDisplayType.None, PomlDisplayType.None)]
    [TestCase(PomlArDisplayType.Occlusion, PomlArDisplayType.None, PomlDisplayType.None)]
    [TestCase(PomlArDisplayType.None, PomlArDisplayType.None, PomlDisplayType.None)]
    public void ArDisplayInHierarchy(PomlArDisplayType parentArDisplay, PomlArDisplayType childArDisplay, PomlDisplayType childArDisplayInHierarchy)
    {
        var parentElement = new PomlEmptyElement()
        {
            ArDisplay = parentArDisplay,
        };

        var childElement = new PomlEmptyElement()
        {
            ArDisplay = childArDisplay,
            Parent = parentElement,
        };

        parentElement.Children = new PomlElement[] { childElement };

        Assert.That(childElement.ArDisplayInHierarchy, Is.EqualTo(childArDisplayInHierarchy));
    }
}
