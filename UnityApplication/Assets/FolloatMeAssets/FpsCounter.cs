using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Profiling;
/// <summary>
/// For debugging: FPS Counter
/// デバッグ用: FPS カウンタ
/// </summary>
public class FpsCounter : MonoBehaviour
{
  /// <summary>
  /// Reflect measurement results every 'EveryCalcurationTime' seconds.
  /// EveryCalcurationTime 秒ごとに計測結果を反映する
  /// </summary>
  [SerializeField, Range(0.1f, 1.0f)]
  float EveryCalcurationTime = 0.5f;

  bool IsFpsBeCounting1 = false;
  bool IsFpsBeCounting2 = false;


  public Record _record;


  int RecordingCount1 = 0;
  int RecordingCount2 = 0;
  
  /// <summary>
  /// FPS value
  /// </summary>
  public float Fps
  {
    get; private set;
  }
  
  int frameCount;
  float prevTime;
  
  void Start()
  {
    frameCount = 0;
    prevTime = 0.0f;
    Fps = 0.0f;
  }

  void Update()
  //void FixedUpdate()
  {
    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
        IsFpsBeCounting1 = !IsFpsBeCounting1;
        if (!IsFpsBeCounting1) 
        {
          _record.LogSave(_record.fps_list, "fps-FukuokaPC-DontSync"+RecordingCount1, true);
          ++RecordingCount1;
        }
        return;
    }
    if (Input.GetKeyDown(KeyCode.Alpha2))
    {
        IsFpsBeCounting2 = !IsFpsBeCounting2;
        if (!IsFpsBeCounting2)
        {
          _record.LogSave(_record.fps_list, "fps2-"+RecordingCount2, true);
          ++RecordingCount2;
        }
        return;
    }

    if (IsFpsBeCounting1 || IsFpsBeCounting2)
    {
        frameCount++;
        float time = Time.realtimeSinceStartup - prevTime;
        
        // n秒ごとに計測
        if (time >= EveryCalcurationTime)
        {
          Fps = frameCount / time;
          
          frameCount = 0;
          prevTime = Time.realtimeSinceStartup;
          // Debug.Log("Fps = "+Fps);
          _record.fps_list.Add(Fps);
        }
    }
    
  }
}