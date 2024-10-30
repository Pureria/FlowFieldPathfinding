using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private MapInfoSO mapInfoSO;

    private void Update()
    {
        //WASDでカメラ移動
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * moveSpeed * Time.deltaTime;
        }
        
        //マウスホイールでZ軸移動
        transform.position += transform.up * -Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
        
        //クリックしたオブジェクト取得
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log(hit.collider.gameObject.name);
                Vector2Int clickPos = new Vector2Int((int)hit.point.x, (int)-hit.point.z);
                mapInfoSO.mapInfo.UpdateMap(clickPos);
            }
        }
    }
}
