using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct MaskableRenderPassSettings
{
    public LayerMask layerMask;
    public RenderPassEvent renderPassEvent;
    public bool drawSkybox;
}
