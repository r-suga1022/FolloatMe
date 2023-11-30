using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainOperation : MonoBehaviour
{
    public CharacterOperation charaope;
    public SerialSendNew serisend;
    public SerialReceive serirece;

    public bool mouse_prototyping;

    public Text deltaTimeText;

    // Start is called before the first frame update
    void Start()
    {
        // マウスカーソルを表示にする
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // マウスプロトタイピングか、Optiによるトラッキングか
        charaope.mouse_prototyping = this.mouse_prototyping;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            bool charaflag = charaope.IsTrackingStop;
            bool serialflag = serisend.IsSendStop;

            charaope.IsTrackingStop = !charaflag;
            serisend.IsSendStop = !serialflag;
        }

        //Debug.Log("deltaTime = "+Time.deltaTime);
        deltaTimeText.text = Time.deltaTime.ToString();
    }
}
