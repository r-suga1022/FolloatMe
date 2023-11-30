using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RecordPosRot : MonoBehaviour {

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

        //Returnキーで記録開始&終了
		if (Input.GetKeyDown(KeyCode.Return))
        {
            f = !f;
            if (f)
            {
                startTime = Time.time;
                Debug.Log("start");
            }
            else
            {
                //csv書き出し
                LogSave(allLogs, fileName + n);
                //リストの初期化
                allLogs.Clear();
                Debug.Log("stop");
                n++;
            }

        }

        if (f)
        {
            //時間データ
            elapsedTime = Time.time - startTime;

            //位置データ
            pos = target.transform.position*10000;

            //回転データ
            rot = target.transform.rotation.eulerAngles; //回転をオイラー角で取得

            //各データを配列に格納
            log = new float[] { elapsedTime, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z };
            //リストに追加
            allLogs.Add(log);
        }

    }

    //csv書き出し
    public void LogSave(List<float[]> x, string fileName)
    {
        StreamWriter sw;
        FileInfo fi;

        fi = new FileInfo(Application.dataPath + "/" + fileName + ".csv");
        sw = fi.AppendText();
        for (int i = 0; i < x.Count; i++)
        {
            for (int j = 0; j < x[i].Length; j++)
            {
                if (j == x[i].Length - 1)
                {
                    sw.WriteLine(x[i][j].ToString());
                }
                else
                {
                    sw.Write(x[i][j].ToString() + ',');
                }
            }
        }
        sw.Flush();
        sw.Close();
    }
}