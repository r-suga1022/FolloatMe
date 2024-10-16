using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Text;

public class Record : MonoBehaviour
{
    public List<float> tracking_interval_list;
    public List<float> tracked_position_list;
    public List<float> fps_list;
    public List<float> pulsewidth_list;
    public List<int> ReceivedOnMiconWidthList;

    bool Pulserecording = false;

    // Start is called before the first frame update
    void Start()
    {
        tracking_interval_list = new List<float>();
        tracked_position_list = new List<float>();
        fps_list = new List<float>();
        pulsewidth_list = new List<float>();
        ReceivedOnMiconWidthList = new List<int>();
    }

    bool DifShow = false;
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Pulserecording)
            {
                LogSave(pulsewidth_list, "pulsewidth", false);
            }
            Pulserecording = !Pulserecording;
        }

    }

    public void AddData(List<float> list, float data)
    {
        list.Add(data);
    }

    //csv書き出し
    public void LogSave(List<float> x, string fileName, bool AppendToFile)
    {
        FileInfo fi;
        StreamWriter sw;

        string filepath = Application.dataPath + "/" + fileName + ".csv";
        //string filepath = "../" + fileName + ".csv";
        
        // fi = new FileInfo(Application.dataPath + "/" + fileName + ".csv");
        // sw = fi.AppendText();
        using (sw = new StreamWriter(filepath, AppendToFile))
        {
            for (int i = 0; i < x.Count; ++i)
            {
                sw.Write(x[i].ToString()+"\n");
            }

            sw.Flush();
            sw.Close();
        }

        x.Clear();
    }
}
