using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

public class ClusteredRenderPipeline : RenderPipeline
{
    private ClusteredRenderPipelineAsset renderPipelineAsset;

    private RenderGraph myRenderGraph;

    public ClusteredRenderPipeline(ClusteredRenderPipelineAsset asset)
    {
        renderPipelineAsset = asset;
        myRenderGraph = new RenderGraph("ClusteredRenderGraph");
    }

    protected override void Dispose(bool disposing)
    {
        myRenderGraph?.Cleanup();
        base.Dispose(disposing);
    }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var cam in cameras)
        {
            if (cam.enabled)
            {
                RenderCamera(context, cam);
            }
        }

    }

    class PassData
    {
        public TextureHandle cameraTarget;
        public Material material;
        public Color clearColor;
    }

    void RenderCamera(ScriptableRenderContext context, Camera cameraToRender)
    {
        context.SetupCameraProperties(cameraToRender);
        var cmd = CommandBufferPool.Get("Example command buffer");

        RenderGraphParameters rgParams = new RenderGraphParameters
        {
            commandBuffer = cmd,
            scriptableRenderContext = context,
            currentFrameIndex = Time.frameCount,
        };

        try
        {
            myRenderGraph.BeginRecording(rgParams);
            using (var builder = myRenderGraph.AddRasterRenderPass<PassData>("Example render pass", out var passData))
            {

                RenderTargetInfo cameraTargetProperties = new RenderTargetInfo
                {
                    width = cameraToRender.pixelWidth,
                    height = cameraToRender.pixelHeight,
                    volumeDepth = 1,
                    msaaSamples = 1,
                    format = GraphicsFormat.R8G8B8A8_UNorm
                };
                passData.cameraTarget = myRenderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget, cameraTargetProperties);

                passData.material = new Material(Shader.Find("Examples/SimpleUnlitColor"));
                passData.clearColor = cameraToRender.backgroundColor;

                builder.SetRenderAttachment(passData.cameraTarget, 0, AccessFlags.Write);

                // Make sure the render graph system keeps the render pass, even if it's not used in the final frame.
                // Don't use this in production code, because it prevents the render graph system from removing the render pass if it's not needed.
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(static (PassData passData, RasterGraphContext context) =>
                {
                    // Create a quad mesh
                    Mesh mesh = new Mesh();

                    Vector3[] vertices = new Vector3[4]
                    {
                        new Vector3(0, 0, 0),
                        new Vector3(1f, 0, 0),
                        new Vector3(0, 1f, 0),
                        new Vector3(1f, 1f, 0)
                    };
                    mesh.vertices = vertices;

                    int[] triangles = new int[6]
                    {
                        0, 2, 1,
                        2, 3, 1
                    };
                    mesh.triangles = triangles;

                    context.cmd.ClearRenderTarget(true, true, passData.clearColor);

                    // Create a transformation matrix for the quad
                    Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);

                    // Draw the quad onto the camera target, using the shader and the source texture
                    context.cmd.DrawMesh(mesh, trs, passData.material, 0);
                });
            }

            myRenderGraph.EndRecordingAndExecute();
        }
        catch (Exception e)
        {
            if (myRenderGraph.ResetGraphAndLogException(e))
                throw;
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
        context.Submit();
    }
}
