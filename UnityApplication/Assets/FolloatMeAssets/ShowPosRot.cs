using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShowPosRot : MonoBehaviour {

    public GameObject target;

    private List<float[]> allLogs = new List<float[]>();
    private float[] log;
    private float startTime;
    private float elapsedTime;
    private Vector3 pos;
    private Vector3 rot;
    private bool f = false;
    private string fileName = "log";
    private int n = 1;

	// Update is called once per frame
	void Update () {
        //時間データ
        elapsedTime = Time.time - startTime;

        //位置データ
        pos = target.transform.position*1000;

        //回転データ
        rot = target.transform.rotation.eulerAngles; //回転をオイラー角で取得

        //各データを配列に格納
        log = new float[] { elapsedTime, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z };
        //リストに追加
        // allLogs.Add(log);
        Debug.Log("position = "+pos);

    }
}