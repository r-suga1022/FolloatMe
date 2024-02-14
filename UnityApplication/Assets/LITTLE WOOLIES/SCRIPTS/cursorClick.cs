using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorClick : MonoBehaviour
{
    public Texture2D pointCursor;
    public Texture2D clickCursor;



    public void OnMouseEnter()
    {
        Cursor.SetCursor(pointCursor, Vector2.zero, CursorMode.Auto);
    }
    public void OnMouseDown()
    {
        Cursor.SetCursor(clickCursor, Vector2.zero, CursorMode.Auto);
    }
    public void OnMouseUp()
    {
        Cursor.SetCursor(pointCursor, Vector2.zero, CursorMode.Auto);
    }
    public void OnMouseExit()
    {
        Cursor.SetCursor(default, Vector2.zero, CursorMode.Auto);
    }
}
