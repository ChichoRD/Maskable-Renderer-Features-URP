using System.Collections.Generic;
using UnityEngine.Rendering;

public static class RenderPassUtils
{
    public static readonly List<ShaderTagId> DefaultUrpShaderTags = new List<ShaderTagId>()
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward"),
        };

    public static readonly string[] DefaultUrpShaderTagNames = new string[]
    {
            "SRPDefaultUnlit",
            "UniversalForward",
            "UniversalForwardOnly",
            "LightweightForward",
    };
}
