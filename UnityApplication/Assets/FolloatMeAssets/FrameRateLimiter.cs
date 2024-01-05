using UnityEngine;
using UnityEngine.UI;

public class FrameRateLimiter : MonoBehaviour
{
    [SerializeField] public int targetFrameRate;
    [SerializeField] public int vSyncCount;

    public Text _IntervalText;

    void Awake()
    {
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.maxQueuedFrames = 2;
    }

    void Update()
    {
        if (Application.targetFrameRate != targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
        }
        //_IntervalText.text = Time.deltaTime.ToString();
    }
}