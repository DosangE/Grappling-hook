using System.Collections;
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


    void Start()
    {
        StartCoroutine(SpawnObstacleRoutine());
    }

    IEnumerator SpawnObstacleRoutine()
    {
        while (true)
        {
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.y = Random.Range(minY, maxY);
            Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);     // 장애물 생성
            yield return new WaitForSeconds(spawnInterval); // 생성 주기 대기
            Debug.Log("장애물 생성됨");
        }
    }
}
