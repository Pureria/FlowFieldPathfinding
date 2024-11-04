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
        //マップデータの読込
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

        //マップ情報の作成
        MapInfo mapInfo = new MapInfo(mapData, width, height);
        mapInfoSO.mapInfo = mapInfo;
        GenerateMap(mapInfo);

        mapInfo.UpdateFlowFieldMap(new Vector2Int(5, 5));
    }

    private void GenerateMap(MapInfo mapInfo)
    {
        for(int y = 0; y < mapInfo.height; y++)
        {
            for(int x = 0; x < mapInfo.width; x++)
            {
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
        
        //各グリッドのコストを無限大に初期化
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                dijkstraMap[y, x] = int.MaxValue;
            }
        }
    }
    
    /// <summary>
    /// 指定した座標の方向を取得する
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 GetFlowField(Vector2Int position)
    {
        return flowMap[position.y, position.x];
    }

    /// <summary>
    /// マップを更新する
    /// </summary>
    /// <param name="goal"></param>
    public void UpdateFlowFieldMap(Vector2Int goal)
    {
        UpdateDijkstraMap(goal);
        UpdateFlowFieldMap(dijkstraMap);
    }
    
    /// <summary>
    /// ダイクストラマップを更新する
    /// </summary>
    /// <param name="goal"></param>
    private void UpdateDijkstraMap(Vector2Int goal)
    {
        dijkstraMap = DijkstraAlgorithm.GetDijkstraMap(mapData, goal, out int maxCost);
        //DijkstraAlgorithm.PrintDijkstraMap(dijkstraMap);
    }
    
    /// <summary>
    /// フローフィールドマップを更新する
    /// </summary>
    /// <param name="dijkstraMap"></param>
    private void UpdateFlowFieldMap(int[,] dijkstraMap)
    {
        for(int height = 0; height < this.height; height++)
        {
            for(int width = 0; width < this.width; width++)
            {
                flowMap[height, width] = CalculateFlowFieldDirection(new Vector2Int(width, height), dijkstraMap);
            }
        }
    }
    
    private Vector2 CalculateFlowFieldDirection(Vector2Int position, int[,] dijkstraMap)
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
