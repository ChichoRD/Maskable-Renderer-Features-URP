using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public abstract class MaskableRenderPass : ScriptableRenderPass
{
    private const string BLIT_TO_DEPTH_SHADER_PATH = "Hidden/BlitToDepth";
    private const string MASK_OVERRIDE_SHADER_PATH = "Shader Graphs/White Override";
    private RenderStateBlock _renderStateBlock;
    private FilteringSettings _filteringSettings;
    private readonly ProfilingSampler _profilingSampler = new ProfilingSampler(nameof(MaskableRenderPass));

    private Material _maskOverrideMaterial;
    private Material _blitCopyDepthMaterial;

    protected RenderTargetIdentifier CameraColorBuffer { get; private set; }

    private RenderTargetIdentifier _filteringBuffer;
    private RenderTargetIdentifier _cameraDepthBuffer;

    private static readonly int s_filteringBufferID = Shader.PropertyToID($"_{nameof(_filteringBuffer)}");
    private static readonly int s_cameraDepthBufferID = Shader.PropertyToID("_CameraDepthTexture");

    public MaskableRenderPass(MaskableRenderPassSettings settings)
    {
        renderPassEvent = settings.renderPassEvent;
        RenderQueueRange renderQueueRange = RenderQueueRange.all;
        _filteringSettings = new FilteringSettings(renderQueueRange, settings.layerMask);

        _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (_blitCopyDepthMaterial == null) _blitCopyDepthMaterial = CoreUtils.CreateEngineMaterial(BLIT_TO_DEPTH_SHADER_PATH);
        if (_maskOverrideMaterial == null) _maskOverrideMaterial = CoreUtils.CreateEngineMaterial(MASK_OVERRIDE_SHADER_PATH);

        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.colorFormat = RenderTextureFormat.ARGB32;

        var renderer = renderingData.cameraData.renderer;

        cmd.GetTemporaryRT(s_filteringBufferID, descriptor, FilterMode.Bilinear);
        _filteringBuffer = new RenderTargetIdentifier(s_filteringBufferID);

        CameraColorBuffer = renderer.cameraColorTarget;
        _cameraDepthBuffer = new RenderTargetIdentifier(s_cameraDepthBufferID);

        ConfigureTarget(_filteringBuffer);
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
        DrawingSettings drawingSettings = CreateDrawingSettings(RenderPassUtils.DefaultUrpShaderTags, ref renderingData, sortingCriteria);
        drawingSettings.overrideMaterial = _maskOverrideMaterial;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            //Clear small RT
            cmd.ClearRenderTarget(true, true, Color.clear);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            //Blit Camera Depth Texture
            Blit(cmd, _cameraDepthBuffer, _filteringBuffer, _blitCopyDepthMaterial);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            //Draw to RT
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings, ref _renderStateBlock);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            ExecuteWithMask(context, ref renderingData, cmd, drawingSettings, _filteringBuffer);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    protected abstract void ExecuteWithMask(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd, DrawingSettings drawingSettings, RenderTargetIdentifier filteredBuffer);

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (cmd == null) throw new ArgumentNullException(nameof(cmd));

        cmd.ReleaseTemporaryRT(s_filteringBufferID);
    }
}
