using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class ClusteredRenderPipeline : RenderPipeline
{
    private ClusteredRenderPipelineAsset pipelineAsset;
    private ClusteredRenderer renderer;
    private RenderGraph renderGraph;
    

    public ClusteredRenderPipeline(ClusteredRenderPipelineAsset asset)
    {
        this.pipelineAsset = asset;

        this.renderGraph = new RenderGraph("ClusteredRenderPipeline");
        this.renderer = new ClusteredRenderer();
    }

    private static readonly ShaderTagId testShaderTagID = new ShaderTagId("TestUnlit");
    private static readonly List<ShaderTagId> s_ShaderTagIds = new List<ShaderTagId>
    {
        testShaderTagID
    };

    // Definition of Data for RenderGraph Pass
    private class OpaquePassData
    {
        public TextureHandle cameraColorTarget;
    }
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach(var camera in cameras)
        {
            renderer.Render(context, camera, renderGraph);
        }
        
    }

    protected override void Dispose(bool disposing)
    {
        renderGraph.Cleanup();
        renderGraph = null;
        base.Dispose(disposing);
    }
}