using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UHFPS.Rendering
{
    public class ScreenshotPass : CustomPass
    {
        public Vector2Int OutputImageSize = new(640, 360);
        private RTHandle temp;

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            temp = RTHandles.Alloc(OutputImageSize.x, OutputImageSize.y, TextureXR.slices, dimension: TextureXR.dimension,
                useDynamicScale: false, name: "ScreenshotRT");
        }

        protected override void Execute(CustomPassContext ctx)
        {
            CoreUtils.SetRenderTarget(ctx.cmd, temp);
            Blitter.BlitCameraTexture(ctx.cmd, ctx.cameraColorBuffer, temp);
        }

        protected override void Cleanup()
        {
            RTHandles.Release(temp);
        }

        public IEnumerator CaptureScreenToFile(string outputPath)
        {
            yield return new WaitForEndOfFrame();

            // request an asynchronous readback of the render texture
            AsyncGPUReadback.Request(temp.rt, 0, TextureFormat.RGBA32, (AsyncGPUReadbackRequest request) =>
            {
                if (request.hasError)
                {
                    Debug.LogError("GPU readback error detected.");
                    return;
                }

                var rawData = request.GetData<byte>();
                Texture2D texture = new(OutputImageSize.x, OutputImageSize.y, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(rawData);
                texture.Apply();

                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(outputPath, bytes);
            });
        }
    }
}