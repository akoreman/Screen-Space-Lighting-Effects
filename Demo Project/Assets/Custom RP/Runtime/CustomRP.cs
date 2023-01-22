using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;


public class CustomRP : RenderPipeline
{
    CamRenderer renderer;
    public static Shader deferredShader;

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera);
            GraphicsSettings.lightsUseLinearIntensity = true;
        }
    }

    public CustomRP(Shader inputShader)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        deferredShader = inputShader;

        renderer = new CamRenderer();
    }
}

public class CamRenderer
{
    ScriptableRenderContext context;
    Camera camera;
    CullingResults cullingResults;

    GeometryBuffer geometryBuffer = new GeometryBuffer();
    SSAOBuffer ssaoBuffer = new SSAOBuffer();

    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer {name = bufferName};

    public Material material = new Material(CustomRP.deferredShader);

    Lighting lighting = new Lighting();

    public void Render (ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        // If there are no vertices within the culling volume, stop the rendering process.
        if (!Cull())
            return;

        lighting.Setup(context, cullingResults);

        Setup();
        DrawGeometry();  
        Submit();
    }

    void Setup()
    {
        context.SetupCameraProperties(camera);

        ExecuteBuffer();

        geometryBuffer.Setup(context, cullingResults, camera);
        ssaoBuffer.Setup(context, camera, material, cullingResults);

        buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, true, Color.clear);

        ExecuteBuffer();
    }

    void DrawGeometry()
    {
        // Render a screen-space triangle using the lit pass of the deferred shader.
        buffer.DrawProcedural(
			Matrix4x4.identity, material, 2,
			MeshTopology.Triangles, 3
		);
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

    bool Cull()
    {
        ScriptableCullingParameters param;

        if (camera.TryGetCullingParameters(out param))
        {
            this.cullingResults = context.Cull(ref param);
            return true;
        }

        return false;
    }
}