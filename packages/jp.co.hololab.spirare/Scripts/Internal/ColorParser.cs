using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    internal static class ColorParser
    {
        private static readonly Dictionary<string, Color> _htmlColors = new Dictionary<string, Color>()
        {
            // black (r, g, b, a) = (0, 0, 0, 255)
            ["black"] = new Color(0f, 0f, 0f, 1f),
            // aliceblue (r, g, b, a) = (240, 248, 255, 255)
            ["aliceblue"] = new Color(0.9411765f, 0.972549f, 1f, 1f),
            // darkcyan (r, g, b, a) = (0, 139, 139, 255)
            ["darkcyan"] = new Color(0f, 0.54509807f, 0.54509807f, 1f),
            // lightyellow (r, g, b, a) = (255, 255, 224, 255)
            ["lightyellow"] = new Color(1f, 1f, 0.8784314f, 1f),
            // coral (r, g, b, a) = (255, 127, 80, 255)
            ["coral"] = new Color(1f, 0.49803922f, 0.3137255f, 1f),
            // dimgray (r, g, b, a) = (105, 105, 105, 255)
            ["dimgray"] = new Color(0.4117647f, 0.4117647f, 0.4117647f, 1f),
            // lavender (r, g, b, a) = (230, 230, 250, 255)
            ["lavender"] = new Color(0.9019608f, 0.9019608f, 0.98039216f, 1f),
            // teal (r, g, b, a) = (0, 128, 128, 255)
            ["teal"] = new Color(0f, 0.5019608f, 0.5019608f, 1f),
            // lightgoldenrodyellow (r, g, b, a) = (250, 250, 210, 255)
            ["lightgoldenrodyellow"] = new Color(0.98039216f, 0.98039216f, 0.8235294f, 1f),
            // tomato (r, g, b, a) = (255, 99, 71, 255)
            ["tomato"] = new Color(1f, 0.3882353f, 0.2784314f, 1f),
            // gray (r, g, b, a) = (128, 128, 128, 255)
            ["gray"] = new Color(0.5019608f, 0.5019608f, 0.5019608f, 1f),
            // lightsteelblue (r, g, b, a) = (176, 196, 222, 255)
            ["lightsteelblue"] = new Color(0.6901961f, 0.76862746f, 0.87058824f, 1f),
            // darkslategray (r, g, b, a) = (47, 79, 79, 255)
            ["darkslategray"] = new Color(0.18431373f, 0.30980393f, 0.30980393f, 1f),
            // lemonchiffon (r, g, b, a) = (255, 250, 205, 255)
            ["lemonchiffon"] = new Color(1f, 0.98039216f, 0.8039216f, 1f),
            // orangered (r, g, b, a) = (255, 69, 0, 255)
            ["orangered"] = new Color(1f, 0.27058825f, 0f, 1f),
            // darkgray (r, g, b, a) = (169, 169, 169, 255)
            ["darkgray"] = new Color(0.6627451f, 0.6627451f, 0.6627451f, 1f),
            // lightslategray (r, g, b, a) = (119, 136, 153, 255)
            ["lightslategray"] = new Color(0.46666667f, 0.53333336f, 0.6f, 1f),
            // darkgreen (r, g, b, a) = (0, 100, 0, 255)
            ["darkgreen"] = new Color(0f, 0.39215687f, 0f, 1f),
            // wheat (r, g, b, a) = (245, 222, 179, 255)
            ["wheat"] = new Color(0.9607843f, 0.87058824f, 0.7019608f, 1f),
            // red (r, g, b, a) = (255, 0, 0, 255)
            ["red"] = new Color(1f, 0f, 0f, 1f),
            // silver (r, g, b, a) = (192, 192, 192, 255)
            ["silver"] = new Color(0.7529412f, 0.7529412f, 0.7529412f, 1f),
            // slategray (r, g, b, a) = (112, 128, 144, 255)
            ["slategray"] = new Color(0.4392157f, 0.5019608f, 0.5647059f, 1f),
            // green (r, g, b, a) = (0, 128, 0, 255)
            ["green"] = new Color(0f, 0.5019608f, 0f, 1f),
            // burlywood (r, g, b, a) = (222, 184, 135, 255)
            ["burlywood"] = new Color(0.87058824f, 0.72156864f, 0.5294118f, 1f),
            // crimson (r, g, b, a) = (220, 20, 60, 255)
            ["crimson"] = new Color(0.8627451f, 0.078431375f, 0.23529412f, 1f),
            // lightgray (r, g, b, a) = (211, 211, 211, 255)
            ["lightgray"] = new Color(0.827451f, 0.827451f, 0.827451f, 1f),
            // steelblue (r, g, b, a) = (70, 130, 180, 255)
            ["steelblue"] = new Color(0.27450982f, 0.50980395f, 0.7058824f, 1f),
            // forestgreen (r, g, b, a) = (34, 139, 34, 255)
            ["forestgreen"] = new Color(0.13333334f, 0.54509807f, 0.13333334f, 1f),
            // tan (r, g, b, a) = (210, 180, 140, 255)
            ["tan"] = new Color(0.8235294f, 0.7058824f, 0.54901963f, 1f),
            // mediumvioletred (r, g, b, a) = (199, 21, 133, 255)
            ["mediumvioletred"] = new Color(0.78039217f, 0.08235294f, 0.52156866f, 1f),
            // gainsboro (r, g, b, a) = (220, 220, 220, 255)
            ["gainsboro"] = new Color(0.8627451f, 0.8627451f, 0.8627451f, 1f),
            // royalblue (r, g, b, a) = (65, 105, 225, 255)
            ["royalblue"] = new Color(0.25490198f, 0.4117647f, 0.88235295f, 1f),
            // seagreen (r, g, b, a) = (46, 139, 87, 255)
            ["seagreen"] = new Color(0.18039216f, 0.54509807f, 0.34117648f, 1f),
            // khaki (r, g, b, a) = (240, 230, 140, 255)
            ["khaki"] = new Color(0.9411765f, 0.9019608f, 0.54901963f, 1f),
            // deeppink (r, g, b, a) = (255, 20, 147, 255)
            ["deeppink"] = new Color(1f, 0.078431375f, 0.5764706f, 1f),
            // whitesmoke (r, g, b, a) = (245, 245, 245, 255)
            ["whitesmoke"] = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f),
            // midnightblue (r, g, b, a) = (25, 25, 112, 255)
            ["midnightblue"] = new Color(0.09803922f, 0.09803922f, 0.4392157f, 1f),
            // mediumseagreen (r, g, b, a) = (60, 179, 113, 255)
            ["mediumseagreen"] = new Color(0.23529412f, 0.7019608f, 0.44313726f, 1f),
            // yellow (r, g, b, a) = (255, 255, 0, 255)
            ["yellow"] = new Color(1f, 1f, 0f, 1f),
            // hotpink (r, g, b, a) = (255, 105, 180, 255)
            ["hotpink"] = new Color(1f, 0.4117647f, 0.7058824f, 1f),
            // white (r, g, b, a) = (255, 255, 255, 255)
            ["white"] = new Color(1f, 1f, 1f, 1f),
            // navy (r, g, b, a) = (0, 0, 128, 255)
            ["navy"] = new Color(0f, 0f, 0.5019608f, 1f),
            // mediumaquamarine (r, g, b, a) = (102, 205, 170, 255)
            ["mediumaquamarine"] = new Color(0.4f, 0.8039216f, 0.6666667f, 1f),
            // gold (r, g, b, a) = (255, 215, 0, 255)
            ["gold"] = new Color(1f, 0.84313726f, 0f, 1f),
            // palevioletred (r, g, b, a) = (219, 112, 147, 255)
            ["palevioletred"] = new Color(0.85882354f, 0.4392157f, 0.5764706f, 1f),
            // snow (r, g, b, a) = (255, 250, 250, 255)
            ["snow"] = new Color(1f, 0.98039216f, 0.98039216f, 1f),
            // darkblue (r, g, b, a) = (0, 0, 139, 255)
            ["darkblue"] = new Color(0f, 0f, 0.54509807f, 1f),
            // darkseagreen (r, g, b, a) = (143, 188, 143, 255)
            ["darkseagreen"] = new Color(0.56078434f, 0.7372549f, 0.56078434f, 1f),
            // orange (r, g, b, a) = (255, 165, 0, 255)
            ["orange"] = new Color(1f, 0.64705884f, 0f, 1f),
            // pink (r, g, b, a) = (255, 192, 203, 255)
            ["pink"] = new Color(1f, 0.7529412f, 0.79607844f, 1f),
            // ghostwhite (r, g, b, a) = (248, 248, 255, 255)
            ["ghostwhite"] = new Color(0.972549f, 0.972549f, 1f, 1f),
            // mediumblue (r, g, b, a) = (0, 0, 205, 255)
            ["mediumblue"] = new Color(0f, 0f, 0.8039216f, 1f),
            // aquamarine (r, g, b, a) = (127, 255, 212, 255)
            ["aquamarine"] = new Color(0.49803922f, 1f, 0.83137256f, 1f),
            // sandybrown (r, g, b, a) = (244, 164, 96, 255)
            ["sandybrown"] = new Color(0.95686275f, 0.6431373f, 0.3764706f, 1f),
            // lightpink (r, g, b, a) = (255, 182, 193, 255)
            ["lightpink"] = new Color(1f, 0.7137255f, 0.75686276f, 1f),
            // floralwhite (r, g, b, a) = (255, 250, 240, 255)
            ["floralwhite"] = new Color(1f, 0.98039216f, 0.9411765f, 1f),
            // blue (r, g, b, a) = (0, 0, 255, 255)
            ["blue"] = new Color(0f, 0f, 1f, 1f),
            // palegreen (r, g, b, a) = (152, 251, 152, 255)
            ["palegreen"] = new Color(0.59607846f, 0.9843137f, 0.59607846f, 1f),
            // darkorange (r, g, b, a) = (255, 140, 0, 255)
            ["darkorange"] = new Color(1f, 0.54901963f, 0f, 1f),
            // thistle (r, g, b, a) = (216, 191, 216, 255)
            ["thistle"] = new Color(0.84705883f, 0.7490196f, 0.84705883f, 1f),
            // linen (r, g, b, a) = (250, 240, 230, 255)
            ["linen"] = new Color(0.98039216f, 0.9411765f, 0.9019608f, 1f),
            // dodgerblue (r, g, b, a) = (30, 144, 255, 255)
            ["dodgerblue"] = new Color(0.11764706f, 0.5647059f, 1f, 1f),
            // lightgreen (r, g, b, a) = (144, 238, 144, 255)
            ["lightgreen"] = new Color(0.5647059f, 0.93333334f, 0.5647059f, 1f),
            // goldenrod (r, g, b, a) = (218, 165, 32, 255)
            ["goldenrod"] = new Color(0.85490197f, 0.64705884f, 0.1254902f, 1f),
            // magenta (r, g, b, a) = (255, 0, 255, 255)
            ["magenta"] = new Color(1f, 0f, 1f, 1f),
            // antiquewhite (r, g, b, a) = (250, 235, 215, 255)
            ["antiquewhite"] = new Color(0.98039216f, 0.92156863f, 0.84313726f, 1f),
            // cornflowerblue (r, g, b, a) = (100, 149, 237, 255)
            ["cornflowerblue"] = new Color(0.39215687f, 0.58431375f, 0.92941177f, 1f),
            // springgreen (r, g, b, a) = (0, 255, 127, 255)
            ["springgreen"] = new Color(0f, 1f, 0.49803922f, 1f),
            // peru (r, g, b, a) = (205, 133, 63, 255)
            ["peru"] = new Color(0.8039216f, 0.52156866f, 0.24705882f, 1f),
            // fuchsia (r, g, b, a) = (255, 0, 255, 255)
            ["fuchsia"] = new Color(1f, 0f, 1f, 1f),
            // papayawhip (r, g, b, a) = (255, 239, 213, 255)
            ["papayawhip"] = new Color(1f, 0.9372549f, 0.8352941f, 1f),
            // deepskyblue (r, g, b, a) = (0, 191, 255, 255)
            ["deepskyblue"] = new Color(0f, 0.7490196f, 1f, 1f),
            // mediumspringgreen (r, g, b, a) = (0, 250, 154, 255)
            ["mediumspringgreen"] = new Color(0f, 0.98039216f, 0.6039216f, 1f),
            // darkgoldenrod (r, g, b, a) = (184, 134, 11, 255)
            ["darkgoldenrod"] = new Color(0.72156864f, 0.5254902f, 0.043137256f, 1f),
            // violet (r, g, b, a) = (238, 130, 238, 255)
            ["violet"] = new Color(0.93333334f, 0.50980395f, 0.93333334f, 1f),
            // blanchedalmond (r, g, b, a) = (255, 235, 205, 255)
            ["blanchedalmond"] = new Color(1f, 0.92156863f, 0.8039216f, 1f),
            // lightskyblue (r, g, b, a) = (135, 206, 250, 255)
            ["lightskyblue"] = new Color(0.5294118f, 0.80784315f, 0.98039216f, 1f),
            // lawngreen (r, g, b, a) = (124, 252, 0, 255)
            ["lawngreen"] = new Color(0.4862745f, 0.9882353f, 0f, 1f),
            // chocolate (r, g, b, a) = (210, 105, 30, 255)
            ["chocolate"] = new Color(0.8235294f, 0.4117647f, 0.11764706f, 1f),
            // plum (r, g, b, a) = (221, 160, 221, 255)
            ["plum"] = new Color(0.8666667f, 0.627451f, 0.8666667f, 1f),
            // bisque (r, g, b, a) = (255, 228, 196, 255)
            ["bisque"] = new Color(1f, 0.89411765f, 0.76862746f, 1f),
            // skyblue (r, g, b, a) = (135, 206, 235, 255)
            ["skyblue"] = new Color(0.5294118f, 0.80784315f, 0.92156863f, 1f),
            // chartreuse (r, g, b, a) = (127, 255, 0, 255)
            ["chartreuse"] = new Color(0.49803922f, 1f, 0f, 1f),
            // sienna (r, g, b, a) = (160, 82, 45, 255)
            ["sienna"] = new Color(0.627451f, 0.32156864f, 0.1764706f, 1f),
            // orchid (r, g, b, a) = (218, 112, 214, 255)
            ["orchid"] = new Color(0.85490197f, 0.4392157f, 0.8392157f, 1f),
            // moccasin (r, g, b, a) = (255, 228, 181, 255)
            ["moccasin"] = new Color(1f, 0.89411765f, 0.70980394f, 1f),
            // lightblue (r, g, b, a) = (173, 216, 230, 255)
            ["lightblue"] = new Color(0.6784314f, 0.84705883f, 0.9019608f, 1f),
            // greenyellow (r, g, b, a) = (173, 255, 47, 255)
            ["greenyellow"] = new Color(0.6784314f, 1f, 0.18431373f, 1f),
            // saddlebrown (r, g, b, a) = (139, 69, 19, 255)
            ["saddlebrown"] = new Color(0.54509807f, 0.27058825f, 0.07450981f, 1f),
            // mediumorchid (r, g, b, a) = (186, 85, 211, 255)
            ["mediumorchid"] = new Color(0.7294118f, 0.33333334f, 0.827451f, 1f),
            // navajowhite (r, g, b, a) = (255, 222, 173, 255)
            ["navajowhite"] = new Color(1f, 0.87058824f, 0.6784314f, 1f),
            // powderblue (r, g, b, a) = (176, 224, 230, 255)
            ["powderblue"] = new Color(0.6901961f, 0.8784314f, 0.9019608f, 1f),
            // lime (r, g, b, a) = (0, 255, 0, 255)
            ["lime"] = new Color(0f, 1f, 0f, 1f),
            // maroon (r, g, b, a) = (128, 0, 0, 255)
            ["maroon"] = new Color(0.5019608f, 0f, 0f, 1f),
            // darkorchid (r, g, b, a) = (153, 50, 204, 255)
            ["darkorchid"] = new Color(0.6f, 0.19607843f, 0.8f, 1f),
            // peachpuff (r, g, b, a) = (255, 218, 185, 255)
            ["peachpuff"] = new Color(1f, 0.85490197f, 0.7254902f, 1f),
            // paleturquoise (r, g, b, a) = (175, 238, 238, 255)
            ["paleturquoise"] = new Color(0.6862745f, 0.93333334f, 0.93333334f, 1f),
            // limegreen (r, g, b, a) = (50, 205, 50, 255)
            ["limegreen"] = new Color(0.19607843f, 0.8039216f, 0.19607843f, 1f),
            // darkred (r, g, b, a) = (139, 0, 0, 255)
            ["darkred"] = new Color(0.54509807f, 0f, 0f, 1f),
            // darkviolet (r, g, b, a) = (148, 0, 211, 255)
            ["darkviolet"] = new Color(0.5803922f, 0f, 0.827451f, 1f),
            // mistyrose (r, g, b, a) = (255, 228, 225, 255)
            ["mistyrose"] = new Color(1f, 0.89411765f, 0.88235295f, 1f),
            // lightcyan (r, g, b, a) = (224, 255, 255, 255)
            ["lightcyan"] = new Color(0.8784314f, 1f, 1f, 1f),
            // yellowgreen (r, g, b, a) = (154, 205, 50, 255)
            ["yellowgreen"] = new Color(0.6039216f, 0.8039216f, 0.19607843f, 1f),
            // brown (r, g, b, a) = (165, 42, 42, 255)
            ["brown"] = new Color(0.64705884f, 0.16470589f, 0.16470589f, 1f),
            // darkmagenta (r, g, b, a) = (139, 0, 139, 255)
            ["darkmagenta"] = new Color(0.54509807f, 0f, 0.54509807f, 1f),
            // lavenderblush (r, g, b, a) = (255, 240, 245, 255)
            ["lavenderblush"] = new Color(1f, 0.9411765f, 0.9607843f, 1f),
            // cyan (r, g, b, a) = (0, 255, 255, 255)
            ["cyan"] = new Color(0f, 1f, 1f, 1f),
            // darkolivegreen (r, g, b, a) = (85, 107, 47, 255)
            ["darkolivegreen"] = new Color(0.33333334f, 0.41960785f, 0.18431373f, 1f),
            // firebrick (r, g, b, a) = (178, 34, 34, 255)
            ["firebrick"] = new Color(0.69803923f, 0.13333334f, 0.13333334f, 1f),
            // purple (r, g, b, a) = (128, 0, 128, 255)
            ["purple"] = new Color(0.5019608f, 0f, 0.5019608f, 1f),
            // seashell (r, g, b, a) = (255, 245, 238, 255)
            ["seashell"] = new Color(1f, 0.9607843f, 0.93333334f, 1f),
            // aqua (r, g, b, a) = (0, 255, 255, 255)
            ["aqua"] = new Color(0f, 1f, 1f, 1f),
            // olivedrab (r, g, b, a) = (107, 142, 35, 255)
            ["olivedrab"] = new Color(0.41960785f, 0.5568628f, 0.13725491f, 1f),
            // indianred (r, g, b, a) = (205, 92, 92, 255)
            ["indianred"] = new Color(0.8039216f, 0.36078432f, 0.36078432f, 1f),
            // indigo (r, g, b, a) = (75, 0, 130, 255)
            ["indigo"] = new Color(0.29411766f, 0f, 0.50980395f, 1f),
            // oldlace (r, g, b, a) = (253, 245, 230, 255)
            ["oldlace"] = new Color(0.99215686f, 0.9607843f, 0.9019608f, 1f),
            // turquoise (r, g, b, a) = (64, 224, 208, 255)
            ["turquoise"] = new Color(0.2509804f, 0.8784314f, 0.8156863f, 1f),
            // olive (r, g, b, a) = (128, 128, 0, 255)
            ["olive"] = new Color(0.5019608f, 0.5019608f, 0f, 1f),
            // rosybrown (r, g, b, a) = (188, 143, 143, 255)
            ["rosybrown"] = new Color(0.7372549f, 0.56078434f, 0.56078434f, 1f),
            // darkslateblue (r, g, b, a) = (72, 61, 139, 255)
            ["darkslateblue"] = new Color(0.28235295f, 0.23921569f, 0.54509807f, 1f),
            // ivory (r, g, b, a) = (255, 255, 240, 255)
            ["ivory"] = new Color(1f, 1f, 0.9411765f, 1f),
            // mediumturquoise (r, g, b, a) = (72, 209, 204, 255)
            ["mediumturquoise"] = new Color(0.28235295f, 0.81960785f, 0.8f, 1f),
            // darkkhaki (r, g, b, a) = (189, 183, 107, 255)
            ["darkkhaki"] = new Color(0.7411765f, 0.7176471f, 0.41960785f, 1f),
            // darksalmon (r, g, b, a) = (233, 150, 122, 255)
            ["darksalmon"] = new Color(0.9137255f, 0.5882353f, 0.47843137f, 1f),
            // blueviolet (r, g, b, a) = (138, 43, 226, 255)
            ["blueviolet"] = new Color(0.5411765f, 0.16862746f, 0.8862745f, 1f),
            // honeydew (r, g, b, a) = (240, 255, 240, 255)
            ["honeydew"] = new Color(0.9411765f, 1f, 0.9411765f, 1f),
            // darkturquoise (r, g, b, a) = (0, 206, 209, 255)
            ["darkturquoise"] = new Color(0f, 0.80784315f, 0.81960785f, 1f),
            // palegoldenrod (r, g, b, a) = (238, 232, 170, 255)
            ["palegoldenrod"] = new Color(0.93333334f, 0.9098039f, 0.6666667f, 1f),
            // lightcoral (r, g, b, a) = (240, 128, 128, 255)
            ["lightcoral"] = new Color(0.9411765f, 0.5019608f, 0.5019608f, 1f),
            // mediumpurple (r, g, b, a) = (147, 112, 219, 255)
            ["mediumpurple"] = new Color(0.5764706f, 0.4392157f, 0.85882354f, 1f),
            // mintcream (r, g, b, a) = (245, 255, 250, 255)
            ["mintcream"] = new Color(0.9607843f, 1f, 0.98039216f, 1f),
            // lightseagreen (r, g, b, a) = (32, 178, 170, 255)
            ["lightseagreen"] = new Color(0.1254902f, 0.69803923f, 0.6666667f, 1f),
            // cornsilk (r, g, b, a) = (255, 248, 220, 255)
            ["cornsilk"] = new Color(1f, 0.972549f, 0.8627451f, 1f),
            // salmon (r, g, b, a) = (250, 128, 114, 255)
            ["salmon"] = new Color(0.98039216f, 0.5019608f, 0.44705883f, 1f),
            // slateblue (r, g, b, a) = (106, 90, 205, 255)
            ["slateblue"] = new Color(0.41568628f, 0.3529412f, 0.8039216f, 1f),
            // azure (r, g, b, a) = (240, 255, 255, 255)
            ["azure"] = new Color(0.9411765f, 1f, 1f, 1f),
            // cadetblue (r, g, b, a) = (95, 158, 160, 255)
            ["cadetblue"] = new Color(0.37254903f, 0.61960787f, 0.627451f, 1f),
            // beige (r, g, b, a) = (245, 245, 220, 255)
            ["beige"] = new Color(0.9607843f, 0.9607843f, 0.8627451f, 1f),
            // lightsalmon (r, g, b, a) = (255, 160, 122, 255)
            ["lightsalmon"] = new Color(1f, 0.627451f, 0.47843137f, 1f),
            // mediumslateblue (r, g, b, a) = (123, 104, 238, 255)
            ["mediumslateblue"] = new Color(0.48235294f, 0.40784314f, 0.93333334f, 1f),
        };

        public static bool TryParseHtmlString(string htmlColor, out Color color)
        {
            color = default;
            if (string.IsNullOrEmpty(htmlColor))
            {
                return false;
            }

            if (htmlColor[0] == '#')
            {
                switch (htmlColor.Length)
                {
                    // #RRGGBBAA
                    case 9:
                        {
                            var r = Convert.ToInt32(htmlColor.Substring(1, 2), 16);
                            var g = Convert.ToInt32(htmlColor.Substring(3, 2), 16);
                            var b = Convert.ToInt32(htmlColor.Substring(5, 2), 16);
                            var a = Convert.ToInt32(htmlColor.Substring(7, 2), 16);
                            color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                            return true;
                        }
                    // #RRGGBB
                    case 7:
                        {
                            var r = Convert.ToInt32(htmlColor.Substring(1, 2), 16);
                            var g = Convert.ToInt32(htmlColor.Substring(3, 2), 16);
                            var b = Convert.ToInt32(htmlColor.Substring(5, 2), 16);
                            color = new Color(r / 255f, g / 255f, b / 255f);
                            return true;
                        }
                    // #RGBA
                    case 5:
                        {
                            var rStr = char.ToString(htmlColor[1]);
                            var gStr = char.ToString(htmlColor[2]);
                            var bStr = char.ToString(htmlColor[3]);
                            var aStr = char.ToString(htmlColor[4]);
                            var r = Convert.ToInt32(rStr + rStr, 16);
                            var g = Convert.ToInt32(gStr + gStr, 16);
                            var b = Convert.ToInt32(bStr + bStr, 16);
                            var a = Convert.ToInt32(aStr + aStr, 16);
                            color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                            return true;
                        }
                    // #RGB
                    case 4:
                        {
                            var rStr = char.ToString(htmlColor[1]);
                            var gStr = char.ToString(htmlColor[2]);
                            var bStr = char.ToString(htmlColor[3]);
                            var r = Convert.ToInt32(rStr + rStr, 16);
                            var g = Convert.ToInt32(gStr + gStr, 16);
                            var b = Convert.ToInt32(bStr + bStr, 16);
                            color = new Color(r / 255f, g / 255f, b / 255f);
                            return true;
                        }
                    default:
                        return false;
                }
            }
            else
            {
                return _htmlColors.TryGetValue(htmlColor.ToLower(), out color);
            }
        }
    }
}
