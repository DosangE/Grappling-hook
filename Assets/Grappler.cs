using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappler : MonoBehaviour
{
    public Camera mainCamera;
    public LineRenderer _lineRenderer;       // 와이어(줄)를 시각적으로 그려주는 컴포넌트
    public DistanceJoint2D _distanceJoint;   // 실제 물리적으로 플레이어와 대상 사이를 연결해주는 조인트
    public Rigidbody2D rb;                   // 플레이어의 Rigidbody2D (circle)

    public float boostForce = 5f;            // 우클릭 가속
    public LayerMask grappleLayer;           // Raycast가 감지할 대상의 레이어 (square)

    public float maxGrappleDistance = 8f;  // Raycast가 최대 탐색 거리
    public float allowedMissOffset = 1.5f;   // (임시) 마우스 클릭 위치와 실제 연결 지점 사이 허용 거리

    void Start()
    {
        _distanceJoint.enabled = false;      // 시작 시 연결 비활성화
    }

    void Update()
    {
        // 좌클릭 시: 그래플링 시도
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // 마우스 클릭 위치를 월드 좌표로 변환
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            // 현재 위치로부터 클릭 방향 벡터 계산
            Vector2 direction = mouseWorldPos - (Vector2)transform.position;

            // Raycast 발사: direction 방향으로 max 거리까지, 지정된 레이어에만 충돌
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                direction.normalized,
                maxGrappleDistance,
                grappleLayer.value);

            // 충돌한 오브젝트가 있고 자기 자신이 아니라면
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                float hitDistance = Vector2.Distance(transform.position, hit.point);

                // 너무 가깝거나 먼 지점은 연결X
                if (hitDistance > 2f && hitDistance <= maxGrappleDistance)
                {
                    // 조인트 연결 설정
                    _distanceJoint.connectedAnchor = hit.point;   // 충돌 지점을 조인트 연결점으로 설정
                    _distanceJoint.distance = hitDistance;        // 초기 거리 설정
                    _distanceJoint.enabled = true;                // 연결 활성화

                    // 줄 시각화
                    _lineRenderer.SetPosition(0, transform.position); // 플레이어 위치
                    _lineRenderer.SetPosition(1, hit.point);          // 충돌 지점
                    _lineRenderer.enabled = true;                     // 줄 표시
                }
            }
        }
        // 마우스 좌클릭 해제 시: 그래플링 해제
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _distanceJoint.enabled = false;      // 조인트 해제
            _lineRenderer.enabled = false;       // 줄 숨김
        }

        // 그래플링 중일 때 (줄 연결 상태)
        if (_distanceJoint.enabled)
        {
            // 줄의 시작점은 항상 플레이어 위치로 갱신
            _lineRenderer.SetPosition(0, transform.position);

            // 우클릭 시: 현재 속도 방향으로 가속
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                // 현재 속도 크기 계산
                float currentSpeed = rb.velocity.magnitude;

                // 기준 이하일 경우: 고정된 방향으로 당김
                if (currentSpeed < 0.1f)
                {
                    Vector2 pullDirection = (_distanceJoint.connectedAnchor - (Vector2)transform.position).normalized;
                    rb.AddForce(pullDirection * boostForce, ForceMode2D.Impulse);
                }
                else
                {
                    // 기존처럼 속도 방향으로 부스트
                    Vector2 velocityDir = rb.velocity.normalized;
                    rb.AddForce(velocityDir * boostForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}
