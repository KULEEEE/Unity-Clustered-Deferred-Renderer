using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/ClusteredRenderPipelineAsset")]
public class ClusteredRenderPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new ClusteredRenderPipeline(this);
    }
}
