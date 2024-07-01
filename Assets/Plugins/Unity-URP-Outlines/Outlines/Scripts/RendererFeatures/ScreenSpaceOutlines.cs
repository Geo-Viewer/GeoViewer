using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{
    [System.Serializable]
    private class ScreenSpaceOutlineSettings
    {
        [Header("General Outline Settings")] public Color outlineColor = Color.black;
        [Range(0.0f, 20.0f)] public float outlineScale = 1.0f;

        [Header("Depth Settings")] [Range(0.0f, 100.0f)]
        public float depthThreshold = 1.5f;

        [Range(0.0f, 500.0f)] public float robertsCrossMultiplier = 100.0f;

        [Header("Normal Settings")] [Range(0.0f, 1.0f)]
        public float normalThreshold = 0.4f;

        [Header("Depth Normal Relation Settings")] [Range(0.0f, 2.0f)]
        public float steepAngleThreshold = 0.2f;

        [Range(0.0f, 500.0f)] public float steepAngleMultiplier = 25.0f;
    }

    [System.Serializable]
    private class ViewSpaceNormalsTextureSettings
    {
        [Header("General Scene View Space Normal Texture Settings")]
        public RenderTextureFormat colorFormat;

        public int depthBufferBits = 16;
        public FilterMode filterMode;
        public Color backgroundColor = Color.black;

        [Header("View Space Normal Texture Object Draw Settings")]
        public PerObjectData perObjectData;

        public bool enableDynamicBatching;
        public bool enableInstancing;
    }

    private class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
        private readonly ViewSpaceNormalsTextureSettings _normalsTextureSettings;
        private FilteringSettings _filteringSettings;
        private FilteringSettings _occluderFilteringSettings;

        private readonly List<ShaderTagId> _shaderTagIdList;
        private readonly Material _normalsMaterial;
        private readonly Material _occludersMaterial;

        private RenderTargetHandle _normals;

        public ViewSpaceNormalsTexturePass(RenderPassEvent renderPassEvent, LayerMask layerMask,
            LayerMask occluderLayerMask, ViewSpaceNormalsTextureSettings settings)
        {
            this.renderPassEvent = renderPassEvent;
            this._normalsTextureSettings = settings;
            _filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
            _occluderFilteringSettings = new FilteringSettings(RenderQueueRange.opaque, occluderLayerMask);

            _shaderTagIdList = new List<ShaderTagId>
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };

            _normals.Init("_SceneViewSpaceNormals");
            _normalsMaterial = new Material(Shader.Find("Hidden/ViewSpaceNormals"));

            _occludersMaterial = new Material(Shader.Find("Hidden/UnlitColor"));
            _occludersMaterial.SetColor(Color1, _normalsTextureSettings.backgroundColor);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor normalsTextureDescriptor = cameraTextureDescriptor;
            normalsTextureDescriptor.colorFormat = _normalsTextureSettings.colorFormat;
            normalsTextureDescriptor.depthBufferBits = _normalsTextureSettings.depthBufferBits;
            cmd.GetTemporaryRT(_normals.id, normalsTextureDescriptor, _normalsTextureSettings.filterMode);

            ConfigureTarget(_normals.Identifier());
            ConfigureClear(ClearFlag.All, _normalsTextureSettings.backgroundColor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!_normalsMaterial || !_occludersMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                DrawingSettings drawSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData,
                    renderingData.cameraData.defaultOpaqueSortFlags);
                drawSettings.perObjectData = _normalsTextureSettings.perObjectData;
                drawSettings.enableDynamicBatching = _normalsTextureSettings.enableDynamicBatching;
                drawSettings.enableInstancing = _normalsTextureSettings.enableInstancing;
                drawSettings.overrideMaterial = _normalsMaterial;

                DrawingSettings occluderSettings = drawSettings;
                occluderSettings.overrideMaterial = _occludersMaterial;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref _filteringSettings);
                context.DrawRenderers(renderingData.cullResults, ref occluderSettings, ref _occluderFilteringSettings);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_normals.id);
        }
    }

    private class ScreenSpaceOutlinePass : ScriptableRenderPass
    {
        private readonly Material _screenSpaceOutlineMaterial;

        RenderTargetIdentifier _cameraColorTarget;

        RenderTargetIdentifier _temporaryBuffer;
        readonly int _temporaryBufferID = Shader.PropertyToID("_TemporaryBuffer");

        public ScreenSpaceOutlinePass(RenderPassEvent renderPassEvent, ScreenSpaceOutlineSettings settings)
        {
            this.renderPassEvent = renderPassEvent;

            _screenSpaceOutlineMaterial = new Material(Shader.Find("Hidden/Outlines"));
            _screenSpaceOutlineMaterial.SetColor(OutlineColor, settings.outlineColor);
            _screenSpaceOutlineMaterial.SetFloat(OutlineScale, settings.outlineScale);

            _screenSpaceOutlineMaterial.SetFloat(DepthThreshold, settings.depthThreshold);
            _screenSpaceOutlineMaterial.SetFloat(RobertsCrossMultiplier, settings.robertsCrossMultiplier);

            _screenSpaceOutlineMaterial.SetFloat(NormalThreshold, settings.normalThreshold);

            _screenSpaceOutlineMaterial.SetFloat(SteepAngleThreshold, settings.steepAngleThreshold);
            _screenSpaceOutlineMaterial.SetFloat(SteepAngleMultiplier, settings.steepAngleMultiplier);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor temporaryTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            temporaryTargetDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(_temporaryBufferID, temporaryTargetDescriptor, FilterMode.Bilinear);
            _temporaryBuffer = new RenderTargetIdentifier(_temporaryBufferID);

            _cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!_screenSpaceOutlineMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines")))
            {
                Blit(cmd, _cameraColorTarget, _temporaryBuffer);
                Blit(cmd, _temporaryBuffer, _cameraColorTarget, _screenSpaceOutlineMaterial);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_temporaryBufferID);
        }
    }

    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    [SerializeField] private LayerMask outlinesLayerMask;
    [SerializeField] private LayerMask outlinesOccluderLayerMask;

    [SerializeField] private ScreenSpaceOutlineSettings outlineSettings = new ScreenSpaceOutlineSettings();

    [SerializeField]
    private ViewSpaceNormalsTextureSettings viewSpaceNormalsTextureSettings = new ViewSpaceNormalsTextureSettings();

    private ViewSpaceNormalsTexturePass _viewSpaceNormalsTexturePass;
    private ScreenSpaceOutlinePass _screenSpaceOutlinePass;
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineScale = Shader.PropertyToID("_OutlineScale");
    private static readonly int DepthThreshold = Shader.PropertyToID("_DepthThreshold");
    private static readonly int RobertsCrossMultiplier = Shader.PropertyToID("_RobertsCrossMultiplier");
    private static readonly int NormalThreshold = Shader.PropertyToID("_NormalThreshold");
    private static readonly int SteepAngleThreshold = Shader.PropertyToID("_SteepAngleThreshold");
    private static readonly int SteepAngleMultiplier = Shader.PropertyToID("_SteepAngleMultiplier");

    public override void Create()
    {
        if (renderPassEvent < RenderPassEvent.BeforeRenderingPrePasses)
            renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;

        _viewSpaceNormalsTexturePass = new ViewSpaceNormalsTexturePass(renderPassEvent, outlinesLayerMask,
            outlinesOccluderLayerMask, viewSpaceNormalsTextureSettings);
        _screenSpaceOutlinePass = new ScreenSpaceOutlinePass(renderPassEvent, outlineSettings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_viewSpaceNormalsTexturePass);
        renderer.EnqueuePass(_screenSpaceOutlinePass);
    }
}