using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainOperation : MonoBehaviour
{
    public CharacterOperation _CharacterOperation;
    public SerialSendNew _SerialSend;
    public SerialReceive _SerialReceive;

    public bool MousePrototyping;

    // Start is called before the first frame update
    void Start()
    {
        // マウスカーソルを表示にする
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // マウスプロトタイピングか、Optiによるトラッキングか
        _CharacterOperation.MousePrototyping = this.MousePrototyping;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            bool TrackingStop = _CharacterOperation.TrackingStop;
            bool SendStop = _SerialSend.SendStop;

            _CharacterOperation.TrackingStop = !TrackingStop;
            _SerialSend.SendStop = !SendStop;
        }
    }
}
