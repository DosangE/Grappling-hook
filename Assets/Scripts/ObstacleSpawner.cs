using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject obstaclePrefab;

    [SerializeField]
    private Transform spawnPoint;

    [Header("장애물 생성 주기")]
    [SerializeField]
    private float spawnInterval = 2f;

    [Header("최소 높이")]
    [SerializeField]
    private float minY = -3f;

    [Header("최대 높이")]
    [SerializeField]
    private float maxY = 3f;
    
    List<GameObject> obstacles = new List<GameObject>();
    
    [Header("Pool 개수")]
    [SerializeField]
    private int maxObstacleCount = 10;
    
    private WaitForSeconds _waitForSpawn;


    private void Start()
    {
        for (int i = 0; i < maxObstacleCount; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);
            obj.SetActive(false); // 비활성화
            obstacles.Add(obj); // 리스트에 추가
        }
        
        _waitForSpawn = new WaitForSeconds(spawnInterval);
        StartCoroutine(SpawnObstacleRoutine());
    }

    private IEnumerator SpawnObstacleRoutine()
    {
        while (true)
        {
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.y = Random.Range(minY, maxY);
            GameObject obj = GetObstacle(spawnPos);
            yield return _waitForSpawn;
            Debug.Log("장애물 생성됨");
        }
    }

    private GameObject GetObstacle(Vector3 position)
    {
        foreach (var obstacle in obstacles)
        {
            if (!obstacle.activeSelf)
            {
                obstacle.transform.position = position; // 위치 설정
                obstacle.SetActive(true); // 활성화
                return obstacle; // 장애물 반환
            }
        }

        return null;
    }
}
