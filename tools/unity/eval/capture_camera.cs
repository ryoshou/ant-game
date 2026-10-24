// Renders Camera.main to Temp/captures/camera.png (capture_game_view is stale while the Game view is in Device Simulator mode).
var cam = UnityEngine.Camera.main;
var rt = new UnityEngine.RenderTexture(540, 1170, 24);
var previous = cam.targetTexture;
cam.targetTexture = rt;
cam.Render();
cam.targetTexture = previous;
UnityEngine.RenderTexture.active = rt;
var tex = new UnityEngine.Texture2D(rt.width, rt.height, UnityEngine.TextureFormat.RGB24, false);
tex.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
tex.Apply();
UnityEngine.RenderTexture.active = null;
System.IO.Directory.CreateDirectory("Temp/captures");
System.IO.File.WriteAllBytes("Temp/captures/camera.png", UnityEngine.ImageConversion.EncodeToPNG(tex));
UnityEngine.Object.DestroyImmediate(tex);
rt.Release();
return "saved";
