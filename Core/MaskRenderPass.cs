using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MaskRenderPass : ScriptableRenderPass
{
    private const int DEPTH_BUFFER_BITS = 32;
    private readonly ProfilingSampler _profilingSampler;

    private RenderTargetIdentifier _colorRenderTarget;
    private static readonly int s_ColorRenderTargetID = Shader.PropertyToID(nameof(_colorRenderTarget));

    private RenderTargetIdentifier _depthRenderTarget;
    private static readonly int s_DepthRenderTargetID = Shader.PropertyToID("_CameraDepthTexture");

    private Material _blitToDepthMaterial;
    private readonly MaskRenderPassSettings _settings;

    public MaskRenderPass(MaskRenderPassSettings settings)
    {
        _settings = settings;
        _profilingSampler = new ProfilingSampler($"{nameof(MaskRenderPass)}: {_settings.textureName}");
        renderPassEvent = settings.renderPassEvent;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        base.OnCameraSetup(cmd, ref renderingData);
        _blitToDepthMaterial = _blitToDepthMaterial == null
                               ? CoreUtils.CreateEngineMaterial("Hidden/BlitToDepth")
                               : _blitToDepthMaterial;

        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.colorFormat = _settings.colorFormat;
        descriptor.depthBufferBits = DEPTH_BUFFER_BITS;
        descriptor.msaaSamples = 1;

        cmd.GetTemporaryRT(s_ColorRenderTargetID, descriptor, FilterMode.Point);
        _colorRenderTarget = new RenderTargetIdentifier(s_ColorRenderTargetID);
        _depthRenderTarget = new RenderTargetIdentifier(s_DepthRenderTargetID);

        ConfigureInput(_settings.renderPassInput);
        ConfigureTarget(_colorRenderTarget);
        ConfigureClear(ClearFlag.All, Color.clear);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        using (new ProfilingScope(cmd, _profilingSampler))
        {
            cmd.ClearRenderTarget(true, true, Color.clear);
            Blit(cmd, _depthRenderTarget, _colorRenderTarget, _blitToDepthMaterial);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            FilteringSettings filteringSettings = new FilteringSettings(_settings.RenderQueueRange,
                                                                        _settings.layerMask,
                                                                        _settings.renderingLayerMask);

            DrawingSettings drawingSettings = CreateDrawingSettings(_settings.LightModeShaderTags,
                                                                    ref renderingData,
                                                                    _settings.sortingCriteria);

            if (_settings.drawSkybox)
                context.DrawSkybox(renderingData.cameraData.camera);
            if (_settings.drawRenderers)
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            if (_settings.drawUIOverlay)
                context.DrawUIOverlay(renderingData.cameraData.camera);

            context.Submit();
            cmd.SetGlobalTexture(_settings.textureName, _colorRenderTarget);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        base.OnCameraCleanup(cmd);
        cmd.ReleaseTemporaryRT(s_ColorRenderTargetID);
    }
}