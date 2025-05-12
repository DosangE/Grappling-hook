using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappler : MonoBehaviour
{
    public Camera mainCamera;                // 카메라 참조
    public LineRenderer _lineRenderer;       // 와이어 시각화
    public DistanceJoint2D _distanceJoint;   // 물리 연결
    public Rigidbody2D rb;                   // 캐릭터 물리

    public float boostForce = 5f;            // 우클릭 부스트 힘
    public LayerMask grappleLayer;           // 감지할 레이어(circle)

    public float maxGrappleDistance = 100f;  // Raycast 최대 거리
    public float allowedMissOffset = 1.5f;   // 마우스와 충돌지점 거리 허용

    void Start()
    {
        _distanceJoint.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mouseWorldPos - (Vector2)transform.position;

            // Ray를 충분히 먼 거리로 발사
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                direction.normalized,
                maxGrappleDistance,
                grappleLayer);

            // Debug 용: Ray 방향 확인
            Debug.DrawRay(transform.position, direction.normalized * maxGrappleDistance, Color.red, 1f);

            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                float hitDistance = Vector2.Distance(transform.position, hit.point);
                if (hitDistance > 2f)
                {
                    _distanceJoint.connectedAnchor = hit.point;
                    _distanceJoint.distance = hitDistance;
                    _distanceJoint.enabled = true;

                    _lineRenderer.SetPosition(0, transform.position);
                    _lineRenderer.SetPosition(1, hit.point);
                    _lineRenderer.enabled = true;
                }
            }

        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _distanceJoint.enabled = false;
            _lineRenderer.enabled = false;
        }

        if (_distanceJoint.enabled)
        {
            _lineRenderer.SetPosition(0, transform.position);

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Vector2 velocityDir = rb.velocity.normalized;
                rb.AddForce(velocityDir * boostForce, ForceMode2D.Impulse);
            }
        }
    }
}
