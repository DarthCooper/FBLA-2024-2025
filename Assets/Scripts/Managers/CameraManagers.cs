using UnityEngine;
using Unity.Cinemachine;

public class CameraManagers : MonoBehaviour
{
    public static CameraManagers Instance { get; private set; }

    public Camera[] cameras;

    [SerializeField] public CinemachineImpulseSource[] impulseSources;

    public void Awake()
    {
        Instance = this;
    }

    public void Impulse(int i)
    {
        CinemachineImpulseSource source = impulseSources[i];
        source.GenerateImpulse();
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
