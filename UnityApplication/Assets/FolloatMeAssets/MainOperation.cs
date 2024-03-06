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
        _SerialSend.MousePrototyping = this.MousePrototyping;
    }

    // Update is called once per frame
    void Update()
    {
        _CharacterOperation.TrackingStop = _SerialSend.SendStop;

        if (Input.GetKeyDown(KeyCode.A)) {
            bool TrackingStop = _CharacterOperation.TrackingStop;
            bool SendStop = _SerialSend.SendStop;
            bool AKeyOn = _SerialSend.AKeyOn;

            _CharacterOperation.TrackingStop = !TrackingStop;
            _SerialSend.SendStop = !SendStop;
            _SerialSend.AKeyOn = !AKeyOn;
            Debug.Log("AkeyPressed"+", sendstop = "+_SerialSend.SendStop);
        }

        //
        if (Input.GetKeyDown(KeyCode.O))
        {
            bool DebugOn = _SerialSend.DebugOn;
            _SerialSend.DebugOn = DebugOn;
        }
        //

        if (Input.GetKeyDown(KeyCode.N))
        {
            ++_SerialSend.CurrentTargetNumber;
            if (_SerialSend.CurrentTargetNumber == _SerialSend._targetlist_in_serialsend.Count) _SerialSend.CurrentTargetNumber = 0;
        }
    }
}
