using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class ClusteredRenderer
{
    private static ShaderTagId shaderTagId = new ShaderTagId("TestUnlit");
    private CommandBuffer cmd;

    private class OpaquePassData
    {
        public TextureHandle colorTarget;
    }

    public ClusteredRenderer()
    {
        cmd = new CommandBuffer {name = "Clustered Renderer"};
    }
}