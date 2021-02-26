using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//[System.Serializable] //EventVector3未继承至MonoBehaviour,不能在Unity中显示出来，需要序列化
//public class EventVector3 : UnityEvent<Vector3> { }

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance;
    public Texture2D point, doorway, attack, target, arrow;

    private RaycastHit hitInfo;

    //public EventVector3 OnMouseClicked;
    public event Action<Vector3> OnMouseClicked;

    public event Action<GameObject> OnEnemyClicked;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    private void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo))
        {
            //切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;

                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;

                default:
                    break;
            }
        }
    }

    private void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
                OnMouseClicked?.Invoke(hitInfo.point);
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
        }
    }
}