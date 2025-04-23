using UnityEngine;
using System.IO;

[RequireComponent(typeof(Camera))]
public class SavePngFromCamera : MonoBehaviour
{
    public string sceneName = "Scene"; // Optional: set in inspector

    void Start()
    {
        SaveToPNG();
    }

    void SaveToPNG()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("No Camera component found.");
            return;
        }

        if (cam.targetTexture == null)
        {
            Debug.LogError("Camera must have a RenderTexture assigned.");
            return;
        }

        try
        {
            RenderTexture currentRT = cam.targetTexture;

            cam.Render();

            Texture2D screenshot = new Texture2D(currentRT.width, currentRT.height, TextureFormat.RGBA32, false);
            RenderTexture.active = currentRT;
            screenshot.ReadPixels(new Rect(0, 0, currentRT.width, currentRT.height), 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;

            byte[] bytes = screenshot.EncodeToPNG();
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string desktopPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), $"screenshot_{timestamp}_{sceneName}.png");

            File.WriteAllBytes(desktopPath, bytes);
            Destroy(screenshot);

            Debug.Log($"Screenshot saved to desktop: {desktopPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save screenshot: {e.Message}");
        }
    }
}