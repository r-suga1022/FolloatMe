using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainOperation : MonoBehaviour
{
    public CharacterOperation _CharacterOperation;
    public SerialSend _SerialSend;
    public SerialReceive _SerialReceive;

    public bool MousePrototyping;

    public bool Active;

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
    }
}
