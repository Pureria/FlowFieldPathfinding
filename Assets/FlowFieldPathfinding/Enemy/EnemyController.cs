using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private MapInfoSO _mapInfoSO;
    private Rigidbody _myRB;
    private float _speed;
    
    public void Initialize(MapInfoSO mapInfoSO, float speed)
    {
        this._mapInfoSO = mapInfoSO;
        _myRB = GetComponent<Rigidbody>();
        _speed = speed;
    }

    private void Update()
    {
        //自分の下にあるオブジェクト取得
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit[] hits;

        hits = Physics.RaycastAll(ray, 0.5f);

        foreach (RaycastHit hit in hits)
        {
            if(hit.transform == this.transform) continue;
            Vector2Int currentPos = new Vector2Int((int)hit.point.x, (int)-hit.point.z);
            Vector2 direction = _mapInfoSO.mapInfo.GetFlowField(currentPos);
            _myRB.velocity = new Vector3(direction.x, 0, -direction.y).normalized * _speed;
        }
    }
}
