using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() { Observer.Instance.Subscribe(GameConstants.ObserverKey.OnMapModified, this.OnMapModified); }

    private void OnMapModified(object param)
    {
        Vector2Int matrixWidthHeight = param is Vector2Int ? (Vector2Int)param : default;

        if (matrixWidthHeight == default) return;

        float worldWidth  = matrixWidthHeight.x; // fixed sprite 1 cell = 1 unit
        float worldHeight = matrixWidthHeight.y;

        // Calculate orthographic size to fit the map
        float sizeY = worldHeight / 2f;
        float sizeX = (worldWidth / 2f) / cam.aspect;
        cam.orthographicSize = Mathf.Max(sizeY, sizeX);
    }
}