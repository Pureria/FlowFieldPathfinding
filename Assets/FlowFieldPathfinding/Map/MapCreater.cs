using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Algorithm;
using UnityEngine;
using Utils;

public class MapCreater : MonoBehaviour
{
    [SerializeField] private TextAsset csvFile;
    [SerializeField] private MapInfoSO mapInfoSO;
    [SerializeField] private GameObject BlockPrefab;
    [SerializeField] private GameObject CostUpBlockPrefab;
    [SerializeField] private GameObject WallPrefab;
    List<string[]> csvDatas = new List<string[]>();

    private void Start()
    {
        string line = "";
        StringReader reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }

        int width = int.Parse(csvDatas[0][0]);
        int height = int.Parse(csvDatas[1][0]);
        int[,] mapData = new int[height, width];
        
        for (int y = 0; y < height; y++)
        {
            string[] str = csvDatas[y + 2];
            for (int x = 0; x < width; x++)
            {
                mapData[y, x] = int.Parse(str[x]);
            }
        }

        MapInfo mapInfo = new MapInfo(mapData, width, height);
        mapInfoSO.mapInfo = mapInfo;
        MapGenerate(mapInfo);

        mapInfo.UpdateMap(new Vector2Int(5, 5));
    }

    private void MapGenerate(MapInfo mapInfo)
    {
        for(int y = 0; y < mapInfo.height; y++)
        {
            for(int x = 0; x < mapInfo.width; x++)
            {
                int reversedY = mapInfo.height - 1 - y; // Y軸を逆にする
                if (mapInfo.mapData[y, x] == 0)
                {
                    Instantiate(BlockPrefab, new Vector3(x, 0, -y), Quaternion.identity, transform);
                }
                else if (mapInfo.mapData[y, x] == 2)
                {
                    Instantiate(CostUpBlockPrefab, new Vector3(x, 0, -y), Quaternion.identity, transform);
                }
                else if (mapInfo.mapData[y, x] == 1)
                {
                    Instantiate(WallPrefab, new Vector3(x, 1, -y), Quaternion.identity, transform);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        //ゲーム中のみ表示
        if (!Application.isPlaying) return;
        //マップ情報がない場合は表示しない
        if (mapInfoSO.mapInfo == null) return;
        
        //フローフィールドに基づいて矢印を表示させる
        for (int y = 0; y < mapInfoSO.mapInfo.height; y++)
        {
            for (int x = 0; x < mapInfoSO.mapInfo.width; x++)
            {
                //壁がある場合は表示しない
                if (mapInfoSO.mapInfo.mapData[y, x] == 1) continue;
                
                Vector2 flow = mapInfoSO.mapInfo.flowMap[y, x];
                Vector3 position = new Vector3(x, 1, -y);
                GizmosExtensions.DrawArrow(position, position + new Vector3(flow.x * 0.5f, 0, -flow.y * 0.5f));
            }
        }
    }
}

public class MapInfo
{
    public int[,] mapData;
    public int width;
    public int height;
    public int[,] dijkstraMap;
    public Vector2[,] flowMap;

    public MapInfo(int[,] mapData, int width, int height)
    {
        this.mapData = mapData;
        this.width = width;
        this.height = height;
        dijkstraMap = new int[height, width];
        flowMap = new Vector2[height, width];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                dijkstraMap[y, x] = int.MaxValue;
            }
        }
    }
    
    public Vector2 GetFlowField(Vector2Int position)
    {
        return flowMap[position.y, position.x];
    }

    public void UpdateMap(Vector2Int goal)
    {
        UpdateDijkstraMap(goal);
        UpdateFlowFieldMap(dijkstraMap);
    }
    
    private void UpdateDijkstraMap(Vector2Int goal)
    {
        dijkstraMap = DijkstraAlgorithm.GetDijkstraMap(mapData, goal, out int maxCost);
        //DijkstraAlgorithm.PrintDijkstraMap(dijkstraMap);
    }

    private void UpdateFlowFieldMap(int[,] dijkstraMap)
    {
        for(int height = 0; height < this.height; height++)
        {
            for(int width = 0; width < this.width; width++)
            {
                flowMap[height, width] = GetFlowField(new Vector2Int(width, height), dijkstraMap);
            }
        }
    }
    
    private Vector2 GetFlowField(Vector2Int position, int[,] dijkstraMap)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        };

        int minCost = dijkstraMap[position.y, position.x];
        Vector2Int minDirection = new Vector2Int(0, 0);
        foreach (Vector2Int direction in directions)
        {
            Vector2Int nextPosition = position + direction;
            if (nextPosition.x < 0 || nextPosition.x >= width || nextPosition.y < 0 || nextPosition.y >= height)
            {
                continue;
            }

            if (dijkstraMap[nextPosition.y, nextPosition.x] < minCost)
            {
                minCost = dijkstraMap[nextPosition.y, nextPosition.x];
                minDirection = direction;
            }
        }

        return new Vector2(minDirection.x, minDirection.y);
    }
}
