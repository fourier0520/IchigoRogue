using UnityEngine;
using UnityEngine.Rendering;

public class LowResolutionCamera : MonoBehaviour
{
    RenderTexture renderTexture;
    Material material;

    [SerializeField]
    [Range(1, 640)]
    int width = 166;

    [SerializeField]
    [Range(1, 480)]
    int height = 144;

    void Start()
    {
        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.useMipMap = false;
        renderTexture.filterMode = FilterMode.Point;
        var camera = GetComponent<Camera>();
        camera.targetTexture = renderTexture;

        material = new Material(Shader.Find("Unlit/Texture"));

        var cameraObject = new GameObject("Camera");
        var camera2 = cameraObject.AddComponent<Camera>();
        camera2.cullingMask = 0;
        camera2.transform.parent = transform;

        var commandBuffer = new CommandBuffer();
        commandBuffer.Blit((RenderTargetIdentifier)renderTexture, BuiltinRenderTextureType.CameraTarget);
        camera2.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }
}