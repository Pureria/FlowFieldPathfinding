using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private int _enemyCount = 100;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private MapInfoSO _mapInfoSO;
    [SerializeField] private float _speed = 5f;
    
    private void Start()
    {
        for (int i = 0; i < _enemyCount; i++)
        {
            GameObject enemy = Instantiate(_enemyPrefab, new Vector3( this.transform.position.x + Random.Range(0, 10), 0.8f,  this.transform.position.z + Random.Range(0, 10)), Quaternion.identity);
            enemy.GetComponent<EnemyController>().Initialize(_mapInfoSO, _speed);
        }
    }
}
