using UnityEngine;

public class CameraManagers : MonoBehaviour
{
    public static CameraManagers Instance { get; private set; }

    public Camera[] cameras;

    void Awake()
    {
        Instance = this;
    }

    public Camera GetCamera(int i)
    {
        return cameras[i];
    }

    public Camera GetCamera(string name)
    {
        foreach (Camera camera in cameras)
        {
            if(camera.name == name)
            {
                return camera;
            }
        }
        return null;
    }
}
