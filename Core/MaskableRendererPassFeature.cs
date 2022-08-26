using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public abstract class MaskableRendererPassFeature : ScriptableRendererFeature
{
    public class RenderObjectsDepthPass : ScriptableRenderPass
    {
        private const string WRITE_DEPTH_SHADER_PATH = "Shader Graphs/WriteToDepth";
        private RenderTargetIdentifier _objectsDepthBuffer;
        public static readonly int s_objectsDepthBufferId = Shader.PropertyToID(nameof(_objectsDepthBuffer));

        private Material _writeDepthMaterial;

        private RenderStateBlock _renderStateBlock;
        private FilteringSettings _filteringSettings;
        private readonly ProfilingSampler _profilingSampler = new ProfilingSampler(nameof(RenderObjectsDepthPass));

        public RenderObjectsDepthPass(MaskableRenderPassSettings settings)
        {
            renderPassEvent = settings.renderPassEvent;
            RenderQueueRange renderQueueRange = RenderQueueRange.all;
            _filteringSettings = new FilteringSettings(renderQueueRange, settings.layerMask);

            _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (_writeDepthMaterial == null) _writeDepthMaterial = CoreUtils.CreateEngineMaterial(WRITE_DEPTH_SHADER_PATH);

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            cmd.GetTemporaryRT(s_objectsDepthBufferId, descriptor);
            _objectsDepthBuffer = new RenderTargetIdentifier(s_objectsDepthBufferId);

            ConfigureTarget(_objectsDepthBuffer);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(RenderPassUtils.DefaultUrpShaderTags, ref renderingData, sortingCriteria);
            //Write Depth
            drawingSettings.overrideMaterial = _writeDepthMaterial;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings, ref _renderStateBlock);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [SerializeField] protected MaskableRenderPassSettings renderPassSettings = new MaskableRenderPassSettings()
    {
        renderPassEvent = RenderPassEvent.AfterRenderingOpaques
    };
}
