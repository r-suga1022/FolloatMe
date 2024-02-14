using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorDrag : MonoBehaviour
{
    public Texture2D handCursor;
    public Texture2D holdCursor;



    public void OnMouseEnter()
    {
        Cursor.SetCursor(handCursor, Vector2.zero, CursorMode.Auto);
    }
    public void OnMouseDrag()
    {
        Cursor.SetCursor(holdCursor, Vector2.zero, CursorMode.Auto);
    }

    public void OnMouseExit()
    {
        Cursor.SetCursor(default, Vector2.zero, CursorMode.Auto);
    }
}
