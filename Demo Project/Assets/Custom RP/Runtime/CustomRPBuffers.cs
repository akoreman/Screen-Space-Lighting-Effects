using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

// Class to setup the MRT and setup and render to the appropriate targets.
public class GeometryBuffer
{
    const string bufferName = "GeometryBuffer";

    ScriptableRenderContext context;
    Camera camera;
    CullingResults cullingResults;

    CommandBuffer buffer = new CommandBuffer { name = bufferName };

    // Setup IDs for the appropriate render targets.
    static int normalBufferId = Shader.PropertyToID("_NormalBuffer");
    static int albedoBufferId = Shader.PropertyToID("_AlbedoBuffer");
    static int viewPositionBufferId = Shader.PropertyToID("_ViewPositionBuffer");

    static ShaderTagId geometryShaderTagId = new ShaderTagId("Geometry");

    // Create the array of RT IDs to send to the GPU as the MRT.
    static RenderTargetIdentifier[] mrt = new RenderTargetIdentifier[3];

    static RenderTexture depthBuffer;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, Camera camera)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.camera = camera;

        ExecuteBuffer();

        Render();
        Cleanup();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void Render()
    {
        context.SetupCameraProperties(camera);
        ExecuteBuffer();

        // Create the render targets, create the IDs and throw them together in an array.
        buffer.GetTemporaryRT(normalBufferId, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        RenderTargetIdentifier normalBufferID = new RenderTargetIdentifier(normalBufferId);

        buffer.GetTemporaryRT(albedoBufferId, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        RenderTargetIdentifier albedoBufferID = new RenderTargetIdentifier(albedoBufferId);

        buffer.GetTemporaryRT(viewPositionBufferId, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        RenderTargetIdentifier viewPositionBufferID = new RenderTargetIdentifier(viewPositionBufferId);

        mrt[0] = normalBufferID;
        mrt[1] = albedoBufferID;
        mrt[2] = viewPositionBufferID;

        ExecuteBuffer();

        // Create a render texture to use as the depth buffer.
        depthBuffer = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 24);

        buffer.SetRenderTarget(mrt, depthBuffer);
        buffer.ClearRenderTarget(true, true, Color.clear);

        ExecuteBuffer();

        SortingSettings sortingSettings = new SortingSettings(camera);
        DrawingSettings drawingSettings = new DrawingSettings(geometryShaderTagId, sortingSettings);
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        context.Submit();

        ExecuteBuffer();
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(normalBufferId);
        buffer.ReleaseTemporaryRT(albedoBufferId);
        buffer.ReleaseTemporaryRT(viewPositionBufferId);

        ExecuteBuffer();

        RenderTexture.ReleaseTemporary(depthBuffer);
    }
}

// Class to setup the buffer with the information needed for SSAO.
public class SSAOBuffer
{
    ScriptableRenderContext context;
    Camera camera;
    Material material;
    CullingResults cullingResults;

    const string bufferName = "SSAOBuffer";
    CommandBuffer buffer = new CommandBuffer {name = bufferName};


    static ShaderTagId ssaoShaderTagId = new ShaderTagId("SSAO");

    // Setup ID for the render target.
    static int ssaoBufferId = Shader.PropertyToID("_SSAOBuffer");

    public void Setup(ScriptableRenderContext context, Camera camera, Material material, CullingResults cullingResults)
    {
        this.context = context;
        this.camera = camera;
        this.material = material;

        this.cullingResults = cullingResults;

        Render();  
        Submit();
        Cleanup();
    }

    void Render()
    {
        // Create the render target.
        buffer.GetTemporaryRT(ssaoBufferId, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        RenderTargetIdentifier ssaoBufferID = new RenderTargetIdentifier(ssaoBufferId);

        ExecuteBuffer();

        buffer.SetRenderTarget(ssaoBufferID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        buffer.ClearRenderTarget(true, true, Color.clear);

        ExecuteBuffer();

        // TO DO: fix this such that we don't need to render all meshes again to compose the 
        // RT with SSAO data.
        SortingSettings sortingSettings = new SortingSettings(camera);
        DrawingSettings drawingSettings = new DrawingSettings(ssaoShaderTagId, sortingSettings);
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        context.Submit();

        /*
        // Render a screen-space triangle using the SSAO pass of the deferred shader.
        buffer.DrawProcedural(
			Matrix4x4.identity, material, 1,
			MeshTopology.Triangles, 3
		);
        */
    }

    void Submit()
    {
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Cleanup()
    {
        buffer.ReleaseTemporaryRT(ssaoBufferId);

        ExecuteBuffer();
    }
}
