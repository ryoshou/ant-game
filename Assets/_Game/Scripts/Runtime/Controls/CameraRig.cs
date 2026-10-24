using Game.Core;
using UnityEngine;

namespace Game.Controls
{
    /// <summary>Owns the orthographic camera: pan, pinch-zoom around a point, and keeping the view on the map.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        private const float CameraZ = -10f;

        [SerializeField] private GameBootstrap bootstrap;

        private Camera cam;
        private Vector2 center;
        private float tilesAcross;

        public Camera Camera => cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
        }

        private void Start()
        {
            tilesAcross = bootstrap.Config.defaultTilesAcross;
            Apply();
        }

        private void LateUpdate() => Apply();

        public void Pan(Vector2 screenDelta)
        {
            float worldPerPixel = 2f * cam.orthographicSize / Screen.height;
            center -= screenDelta * worldPerPixel;
            Apply();
        }

        public void PinchZoom(Vector2 screenCenter, float scale)
        {
            if (!(scale > 0f)) return;
            TuningConfig config = bootstrap.Config;
            Vector2 focus = ScreenToWorld(screenCenter);
            float oldOrtho = cam.orthographicSize;
            tilesAcross = CameraMath.ClampTilesAcross(tilesAcross / scale, config.minTilesAcross, config.maxTilesAcross);
            float newOrtho = CameraMath.OrthoSizeForTilesAcross(tilesAcross, cam.aspect);
            center = CameraMath.ZoomAround(center, focus, oldOrtho, newOrtho);
            Apply();
        }

        public Vector2 ScreenToWorld(Vector2 screen) => cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -CameraZ));

        private void Apply()
        {
            if (bootstrap.Grid == null) return;
            cam.orthographicSize = CameraMath.OrthoSizeForTilesAcross(tilesAcross, cam.aspect);
            center = CameraMath.ClampCenter(center, cam.orthographicSize, cam.aspect, bootstrap.Grid.Width, bootstrap.Grid.Height);
            transform.position = new Vector3(center.x, center.y, CameraZ);
        }
    }
}
