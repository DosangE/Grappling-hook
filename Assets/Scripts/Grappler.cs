using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Grappler : MonoBehaviour
{
    [Header("컴포넌트 설정")] [SerializeField] private LineRenderer lineRenderer; // 와이어(줄)를 시각적으로 그려주는 컴포넌트
    [SerializeField] private DistanceJoint2D distanceJoint; // 실제 물리적으로 플레이어와 대상 사이를 연결해주는 조인트
    [SerializeField] private Rigidbody2D rb; // 플레이어의 Rigidbody2D (circle)

    [Header("가속 수치")] [SerializeField] private float boostForce = 5f; // 우클릭 가속

    [Header("정지 시 당김 수치")] [SerializeField]
    private float pullForce = 8f; // <정지했을시> 우클릭 당김

    [Header("감지할 대상")] [SerializeField] private LayerMask grappleLayer; // Raycast가 감지할 대상의 레이어 (square)

    [Header("Grapple 최대 거리")] [SerializeField]
    private float maxGrappleDistance = 8f; // Raycast가 최대 탐색 거리

    [Header("Grapple 최소 거리")] [SerializeField]
    private float minGrappleDistance = 2f; // Raycast가 최소 탐색 거리
    
    private void Awake()
    {
        if (lineRenderer == null)
        {
            // Debug.Log
            // Debug.LogWarning
            Debug.LogError("LineRenderer 컴포넌트가 설정되지 않았습니다.");
        }

        if (distanceJoint == null)
        {
            Debug.LogError("DistanceJoint2D 컴포넌트가 설정되지 않았습니다.");
        }
        else
        {
            distanceJoint.enabled = false; // 시작 시 연결 비활성화
        }

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D 컴포넌트가 설정되지 않았습니다.");
        }
    }

    private void Update()
    {
        InputGrapple(); /// 그래플링 입력 처리
        ReleaseGrapple(); // 그래플링 해제 처리
        UpdateGrappleLine(); // 줄 업데이트
        InputBoost(); // 우클릭 가속 or 당기기기
        CheckTargetOut(); // 연결된 오프젝트 화면 이탈 체크
    }

    private void InputGrapple()
    {
        // 좌클릭 시: 그래플링 시도
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // 마우스 클릭 위치를 월드 좌표로 변환
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // 현재 위치로부터 클릭 방향 벡터 계산
            Vector2 direction = mouseWorldPos - (Vector2)transform.position;

            // Raycast 발사: direction 방향으로 max 거리까지, 지정된 레이어에만 충돌
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                direction.normalized,
                maxGrappleDistance,
                grappleLayer);

            // 충돌한 오브젝트가 있고 자기 자신이 아니라면
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                float hitDistance = Vector2.Distance(transform.position, hit.point);
                Debug.Log("Raycast 충돌: " + hit.collider.name);
                Debug.Log("Rigidbody2D: " + hit.collider.attachedRigidbody);

                if (hitDistance > minGrappleDistance && hitDistance <= maxGrappleDistance)
                {
                    // Rigidbody2D 가져오기
                    Rigidbody2D targetRb = hit.collider.attachedRigidbody;

                    if (targetRb != null)
                    {
                        // 움직이는 물체인 경우
                        distanceJoint.connectedBody = targetRb;
                        distanceJoint.autoConfigureDistance = false;
                        distanceJoint.distance = hitDistance;
                    }
                    else
                    {
                        // 움직이지 않는 물체인 경우
                        distanceJoint.connectedAnchor = hit.point;
                        distanceJoint.connectedBody = null;
                        distanceJoint.distance = hitDistance;
                    }

                    // 조인트 활성화
                    distanceJoint.enabled = true;
                    // 줄의 시작점은 항상 플레이어 위치로 갱신
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, hit.point);
                    lineRenderer.enabled = true;
                }
            }
        }
    }

    private void ReleaseGrapple()
    {
        // 마우스 좌클릭 해제 시: 그래플링 해제
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            distanceJoint.enabled = false; // 조인트 해제
            lineRenderer.enabled = false; // 줄 숨김
        }
    }

    private void UpdateGrappleLine()
    {
        if (!distanceJoint.enabled) return; // 줄 비활성화 시: 업데이트 중지

        // 시작점 플레이어 위치로 갱신
        lineRenderer.SetPosition(0, transform.position);

        if (distanceJoint.connectedBody != null) // 움직이는 물체
        {
            // 줄의 끝점은 연결된 물체 위치로 갱신
            lineRenderer.SetPosition(1, distanceJoint.connectedBody.position);
        }
        else // 고정된 물체
        {
            // 줄의 끝점은 연결된 앵커 위치
            lineRenderer.SetPosition(1, distanceJoint.connectedAnchor);
        }
    }

    private void InputBoost()
    {
        if (!distanceJoint.enabled) return; // 줄 비활성화 시: 업데이트 중지

        // 줄의 시작점은 항상 플레이어 위치로 갱신
        lineRenderer.SetPosition(0, transform.position);

        // 우클릭 시: 현재 속도 방향으로 가속
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            // 현재 속도 크기 계산
            float currentSpeed = rb.velocity.magnitude;

            // 기준 이하일 경우: 고정된 방향으로 당김
            if (currentSpeed < 0.1f)
            {
                Vector2 pullDirection = (distanceJoint.connectedAnchor - (Vector2)transform.position).normalized;
                rb.AddForce(pullDirection * pullForce, ForceMode2D.Impulse);
            }
            else
            {
                // 기존처럼 속도 방향으로 부스트
                Vector2 velocityDir = rb.velocity.normalized;
                rb.AddForce(velocityDir * boostForce, ForceMode2D.Impulse);
            }
        }
    }

    private void CheckTargetOut()
    {
        if (!lineRenderer.enabled) return;

        if (distanceJoint.enabled && distanceJoint.connectedBody != null)
        {
            Renderer targetRenderer = distanceJoint.connectedBody.GetComponent<Renderer>();
            if (targetRenderer != null && !targetRenderer.isVisible)
            {
                Debug.Log("오브젝트 화면 이탈, 줄 해제");
                distanceJoint.enabled = false;
                lineRenderer.enabled = false;
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Wall"))
        {
            // TODO: 충돌 시 그래플링 해제
        }
    }
}