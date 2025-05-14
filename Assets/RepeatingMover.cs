using UnityEngine;

public class RepeatingMover : MonoBehaviour
{
    public float speed = 5f;
    public float resetX = -15f;   // 왼쪽 끝
    public float startX = 15f;    // 오른쪽 초기 위치

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // 왼쪽 바깥으로 벗어나면 다시 오른쪽으로 이동
        if (transform.position.x < resetX)
        {
            Vector3 pos = transform.position;
            pos.x = startX;
            transform.position = pos;
        }
    }
}
