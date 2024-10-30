using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Algorithm;
using UnityEngine;
using Utils;

public class MacCreater : MonoBehaviour
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

        //mapInfo.UpdateDijkstraMap(new Vector2Int(5, 5));
    }

    private void MapGenerate(MapInfo mapInfo)
    {
        for(int y = 0; y < mapInfo.height; y++)
        {
            for(int x = 0; x < mapInfo.width; x++)
            {
                int reversedY = mapInfo.height - 1 - y; // Y軸を逆にする
                if (mapInfo.mapData[reversedY, x] == 0)
                {
                    Instantiate(BlockPrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                }
                else if (mapInfo.mapData[reversedY, x] == 2)
                {
                    Instantiate(CostUpBlockPrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                }
                else if (mapInfo.mapData[reversedY, x] == 1)
                {
                    Instantiate(WallPrefab, new Vector3(x, 1, y), Quaternion.identity, transform);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        GizmosExtensions.DrawArrow(new Vector3(0.25f,0,0.25f), new Vector3(0.75f, 0, 0.75f));
    }
}

public class MapInfo
{
    public int[,] mapData;
    public int width;
    public int height;
    public int[,] dijkstraMap;

    public MapInfo(int[,] mapData, int width, int height)
    {
        this.mapData = mapData;
        this.width = width;
        this.height = height;
        dijkstraMap = new int[height, width];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                dijkstraMap[y, x] = int.MaxValue;
            }
        }
    }
    
    public void UpdateDijkstraMap(Vector2Int goal)
    {
        dijkstraMap = DijkstraAlgorithm.GetDijkstraMap(mapData, goal, out int maxCost);
        DijkstraAlgorithm.PrintDijkstraMap(dijkstraMap);
    }
}
