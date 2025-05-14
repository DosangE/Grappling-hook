using UnityEngine;

public class LoopingBackground : MonoBehaviour
{
    public float scrollSpeed = 5f;
    public float backgroundWidth = 18f;  // Sprite 한 장의 너비(무조건 맞춰야 함)

    void Update()
    {
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // 화면 왼쪽으로 빠졌으면 오른쪽으로 이동
        if (transform.position.x <= -backgroundWidth)
        {
            transform.position += new Vector3(backgroundWidth * 2f, 0f, 0f);
        }
    }
}
