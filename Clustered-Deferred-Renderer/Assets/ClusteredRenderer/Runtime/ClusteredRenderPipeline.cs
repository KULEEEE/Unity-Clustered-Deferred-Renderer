using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

public class ClusteredRenderPipeline : RenderPipeline
{
    private ClusteredRenderPipelineAsset renderPipelineAsset;

    private static readonly ShaderTagId unlitShaderTagId = new("ExampleLightModeTag");
    private static readonly ShaderTagId forwardShaderTagId = new("UniversalForward");

    public ClusteredRenderPipeline(ClusteredRenderPipelineAsset asset)
    {
        renderPipelineAsset = asset;
    }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var camera in cameras)
        {
            if (camera.enabled)
            {
                RenderCamera(context, camera);
            }
        }
    }

    void RenderCamera(ScriptableRenderContext context, Camera camera)
    {
        // 컬링 파라미터 설정
        if (!camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParams))
            return;

        // 컬링 수행
        CullingResults cullingResults = context.Cull(ref cullingParams);

        // 카메라 프로퍼티 설정 (VP 매트릭스 등)
        context.SetupCameraProperties(camera);

        // 커맨드 버퍼 생성
        CommandBuffer cmd = CommandBufferPool.Get("Render Camera");

        // 렌더 타겟 클리어
        CameraClearFlags clearFlags = camera.clearFlags;
        cmd.ClearRenderTarget(
            clearFlags <= CameraClearFlags.Depth,
            clearFlags <= CameraClearFlags.Color,
            camera.backgroundColor
        );

        // 불투명 오브젝트 렌더링
        var opaqueRendererListDesc = new RendererListDesc(unlitShaderTagId, cullingResults, camera)
        {
            sortingCriteria = SortingCriteria.CommonOpaque,
            renderQueueRange = RenderQueueRange.opaque
        };
        RendererList opaqueRendererList = context.CreateRendererList(opaqueRendererListDesc);
        cmd.DrawRendererList(opaqueRendererList);

        // 스카이박스 렌더링
        if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
        {
            RendererList skyboxRendererList = context.CreateSkyboxRendererList(camera);
            cmd.DrawRendererList(skyboxRendererList);
        }

        // 투명 오브젝트 렌더링
        var transparentRendererListDesc = new RendererListDesc(unlitShaderTagId, cullingResults, camera)
        {
            sortingCriteria = SortingCriteria.CommonTransparent,
            renderQueueRange = RenderQueueRange.transparent
        };
        RendererList transparentRendererList = context.CreateRendererList(transparentRendererListDesc);
        cmd.DrawRendererList(transparentRendererList);

        // 커맨드 버퍼 실행 및 정리
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);

        // 프레임 제출
        context.Submit();
    }
}
