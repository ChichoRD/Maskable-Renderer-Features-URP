using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct MaskRenderPassSettings
{
    public static MaskRenderPassSettings DefaultOpaque = new MaskRenderPassSettings()
    {
        drawSkybox = false,
        drawRenderers = true,
        drawUIOverlay = false,

        renderPassEvent = RenderPassEvent.AfterRenderingOpaques,
        renderPassInput = ScriptableRenderPassInput.None,

        renderQueueLowerBound = 0,
        renderQueueUpperBound = 2499,

        colorFormat = RenderTextureFormat.ARGB32,
        sortingCriteria = SortingCriteria.CommonOpaque,
        layerMask = -1,
        renderingLayerMask = uint.MaxValue,
        textureName = "_MyTexture",

        lightMode = LightModeTags.Standard,
    };

    public static MaskRenderPassSettings DefaultTransparent = new MaskRenderPassSettings()
    {
        drawSkybox = true,
        drawRenderers = true,
        drawUIOverlay = false,

        renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
        renderPassInput = ScriptableRenderPassInput.None,

        renderQueueLowerBound = 0,
        renderQueueUpperBound = 5000,

        colorFormat = RenderTextureFormat.ARGB32,
        sortingCriteria = SortingCriteria.CommonTransparent,
        layerMask = -1,
        renderingLayerMask = uint.MaxValue,
        textureName = "_MyTexture",

        lightMode = LightModeTags.Standard,
    };

    public static MaskRenderPassSettings DefaultPostProcessing = new MaskRenderPassSettings()
    {
        drawSkybox = true,
        drawRenderers = true,
        drawUIOverlay = true,

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing,
        renderPassInput = ScriptableRenderPassInput.None,

        renderQueueLowerBound = 0,
        renderQueueUpperBound = 5000,

        colorFormat = RenderTextureFormat.ARGB32,
        sortingCriteria = SortingCriteria.CommonOpaque,
        layerMask = -1,
        renderingLayerMask = uint.MaxValue,
        textureName = "_MyTexture",

        lightMode = LightModeTags.Standard,
    };

    [Flags]
    public enum LightModeTags
    {
        None = 0,
        SRPDefaultUnlit = 1 << 0,
        UniversalForward = 1 << 1,
        UniversalForwardOnly = 1 << 2,
        LightweightForward = 1 << 3,
        DepthNormals = 1 << 4,
        DepthOnly = 1 << 5,
        Standard = SRPDefaultUnlit | UniversalForward | UniversalForwardOnly | LightweightForward,
    }

    public bool drawSkybox;
    public bool drawRenderers;
    public bool drawUIOverlay;
    public RenderPassEvent renderPassEvent;
    public ScriptableRenderPassInput renderPassInput;

    [Range(0, 5000)]
    public int renderQueueLowerBound;

    [Range(0, 5000)]
    public int renderQueueUpperBound;

    public RenderTextureFormat colorFormat;
    public SortingCriteria sortingCriteria;
    public LayerMask layerMask;
    public uint renderingLayerMask;
    public string textureName;

    public LightModeTags lightMode;
    public readonly RenderQueueRange RenderQueueRange => new(renderQueueLowerBound, renderQueueUpperBound);

    public readonly List<ShaderTagId> LightModeShaderTags
    {
        get
        {
            var tags = new List<ShaderTagId>();
            if (lightMode.HasFlag(LightModeTags.SRPDefaultUnlit))
                tags.Add(new ShaderTagId("SRPDefaultUnlit"));
            if (lightMode.HasFlag(LightModeTags.UniversalForward))
                tags.Add(new ShaderTagId("UniversalForward"));
            if (lightMode.HasFlag(LightModeTags.UniversalForwardOnly))
                tags.Add(new ShaderTagId("UniversalForwardOnly"));
            if (lightMode.HasFlag(LightModeTags.LightweightForward))
                tags.Add(new ShaderTagId("LightweightForward"));
            if (lightMode.HasFlag(LightModeTags.DepthNormals))
                tags.Add(new ShaderTagId("DepthNormals"));
            if (lightMode.HasFlag(LightModeTags.DepthOnly))
                tags.Add(new ShaderTagId("DepthOnly"));
            return tags;
        }
    }
}