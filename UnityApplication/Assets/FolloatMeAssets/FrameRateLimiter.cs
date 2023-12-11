using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    [SerializeField] public int targetFrameRate;
    [SerializeField] public int vSyncCount;

    void Awake()
    {
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.maxQueuedFrames = 1;
    }

    void Update()
    {
        if (Application.targetFrameRate != targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}