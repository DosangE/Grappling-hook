using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("컴포넌트 설정")]
    [SerializeField] private LineRenderer line;    // 연결 줄의 시각적 표현
    [SerializeField] private Transform hook;    // 줄이 발사되고 도달할 위치
    [SerializeField] private DistanceJoint2D joint2D;    //  물리적으로 연결을 담당하는 Joint
    [SerializeField] private Rigidbody2D rb;

    [Header("감지 대상 레이어")]
    [SerializeField] private LayerMask grappleLayer;    //  Raycast로 감지할 수 있는 레이어
    private Vector2 mousedir;   // 마우스 방향 벡터
    private bool isHookActive;      // 줄이 발사 중인지 여부
    private bool isLineMax;   // 줄이 최대 길이에 도달했는지 여부
    private bool isAttach;      // 줄이 연결된 상태인지 여부

    [Header("줄 설정")]
    // ✅ 줄 길이 설정
    [SerializeField] private float maxGrappleDistance = 6f;
    [SerializeField] private float minGrappleDistance = 1f;
    [SerializeField] private float hookSpeed = 35f; // 줄이 발사되는 속도
    [SerializeField] private float visualOffset = 0.05f;  // 줄 길이 보정 값


    private bool isPulling = false;

    // ✅ 당기는 힘, 회전 속도
    [SerializeField] private float pullForce = 10f;
    [SerializeField] private float rotateSpeed = 8f;

    void Start()
    {
        line.positionCount = 2;
        line.startWidth = 0.15f;
        line.endWidth = 0.05f;
        line.useWorldSpace = true;

        isHookActive = false;
        isAttach = false;
        isLineMax = false;

        line.enabled = false;
        hook.gameObject.SetActive(false);
        joint2D.enabled = false;
    }

    void Update()
    {
        CheckLineBlocked();   // ✅ 항상 실행

        line.SetPosition(0, transform.position);

        if (isAttach && joint2D.enabled && joint2D.connectedBody != null)
        {
            HandleAttachedState();  // 연결된 상태에서의 처리
        }
        else
        {
            HandleDetachedState();  // 연결되지 않은 상태에서의 처리
        }
    }
    private void CheckLineBlocked()
    {
        Vector2 targetPos;

        if (isAttach && joint2D.enabled && joint2D.connectedBody != null)
        {
            // 연결된 상태면 Anchor로 검사
            targetPos = joint2D.connectedBody.transform.TransformPoint(joint2D.connectedAnchor);
        }
        else if (isHookActive)
        {
            // 발사 또는 되돌림 중이면 Hook 위치로 검사
            targetPos = hook.position;
        }
        else
        {
            // 아무것도 없으면 검사 안함
            return;
        }

        Vector2 dir = targetPos - (Vector2)transform.position;
        float distance = dir.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, distance, grappleLayer);

        if (hit.collider != null &&
            hit.collider.attachedRigidbody != joint2D.connectedBody &&  // 연결 대상은 무시
            hit.collider.gameObject != gameObject)                      // Player 자신은 무시
        {
            Debug.Log($"[LineBlocked]  {hit.collider.name} 끼어듦 → 즉시 해제");
            ReleaseGrapple();
        }
    }

    private void HandleAttachedState()
    {

        Vector2 anchorPos = joint2D.connectedBody.transform.TransformPoint(joint2D.connectedAnchor);
        Vector2 dirToAnchor = anchorPos - (Vector2)transform.position;
        // 줄 위치
        line.SetPosition(1, GetVisualAnchor());

        // 좌클릭 해제 시 끊기
        if (!Input.GetKey(KeyCode.Mouse0))
        {
            ReleaseGrapple();
            return;
        }

        CheckTargetOut();   // 연결된 대상이 화면 밖으로 나갔는지 확인

        // 회전
        if (Input.GetMouseButton(1))
        {
            RotateAroundAnchor();
        }

        // 당김
        if (Input.GetKeyDown(KeyCode.Space)) isPulling = true;
        if (Input.GetKeyUp(KeyCode.Space)) isPulling = false;

        if (isPulling)
        {
            PullToAnchor();
        }
        else
        {
            float towardAnchorVelocity = Vector2.Dot(rb.velocity, dirToAnchor.normalized);
            if (towardAnchorVelocity > 0f)
            {
                rb.AddForce(-dirToAnchor.normalized * towardAnchorVelocity * 20f, ForceMode2D.Force);
            }
        }
    }
    private void HandleDetachedState()
    {
        line.SetPosition(1, hook.position);

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isHookActive)
        {
            StartHookShot();    // 줄 발사 시작
        }

        if (isHookActive && !isLineMax && !isAttach)
        {
            ShootHook();    // 줄이 발사 중일 때
        }
        else if (isHookActive && isLineMax && !isAttach)
        {
            ReturnHook();   // 줄이 최대 길이에 도달했을 때
        }
    }

    private void StartHookShot()
    {
        hook.SetParent(null);
        hook.position = transform.position;
        mousedir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

        isHookActive = true;
        isLineMax = false;
        isAttach = false;

        hook.gameObject.SetActive(true);
        line.enabled = true;
    }

    private void ShootHook()
    {
        Vector3 nextPos = hook.position + (Vector3)(mousedir.normalized * Time.deltaTime * hookSpeed);

        RaycastHit2D hit = Physics2D.Raycast(hook.position, mousedir.normalized, Vector2.Distance(hook.position, nextPos), grappleLayer);
        if (hit.collider != null && hit.collider.attachedRigidbody != null)
        {   // 줄이 연결된 상태로 변경
            float hitDistance = Vector2.Distance(transform.position, hit.point);

            if (hitDistance < minGrappleDistance)
            {
                isLineMax = true;   // 줄이 너무 짧으면 연결하지 않음
                return;
            }
            // 줄이 연결된 상태로 변경
            joint2D.connectedBody = hit.collider.attachedRigidbody;
            joint2D.autoConfigureConnectedAnchor = false;
            joint2D.connectedAnchor = hit.collider.attachedRigidbody.transform.InverseTransformPoint(hit.point);
            joint2D.autoConfigureDistance = false;
            joint2D.maxDistanceOnly = false;
            joint2D.enableCollision = true;
            joint2D.distance = hitDistance;
            joint2D.enabled = true;

            isAttach = true;
            hook.position = hit.point;
            return;
        }

        hook.position = nextPos;

        if (Vector2.Distance(transform.position, hook.position) > maxGrappleDistance)
        {
            isLineMax = true;
        }
    }

    private void ReturnHook()
    {
        hook.position = Vector2.MoveTowards(hook.position, transform.position, Time.deltaTime * hookSpeed);
        // 줄이 플레이어에게 돌아오면 연결 해제
        if (Vector2.Distance(transform.position, hook.position) < 0.1f)
        {
            isHookActive = false;
            isLineMax = false;
            hook.gameObject.SetActive(false);
            line.enabled = false;
        }
    }

    // 연결 해제 처리
    private void ReleaseGrapple()
    {
        isAttach = false;
        isHookActive = false;
        isLineMax = false;
        isPulling = false;
        joint2D.connectedBody = null;
        joint2D.enabled = false;
        hook.gameObject.SetActive(false);
        line.enabled = false;
    }

    // 연결된 대상이 화면 왼쪽 밖으로 나가면 연결 해제
    private void CheckTargetOut()
    {
        if (!line.enabled || joint2D.connectedBody == null) return;

        Renderer targetRenderer = joint2D.connectedBody.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            float objectLeftX = targetRenderer.bounds.min.x;
            float cameraLeftX = Camera.main.ViewportToWorldPoint(Vector3.zero).x;

            if (objectLeftX < cameraLeftX)
            {
                ReleaseGrapple();
            }
        }
    }

    // 우클릭: 연결된 점을 중심으로 반시계 회전
    private void RotateAroundAnchor()
    {
        Vector2 visualAnchor = GetVisualAnchor(); // ✅ 보정 Anchor 사용
    
        Vector2 dir = (Vector2)transform.position - visualAnchor;
        Vector2 tangent = new Vector2(-dir.y, dir.x).normalized;

        rb.velocity = tangent * rotateSpeed;
    }

    // 스페이스바 누르면 당겨지기
    private void PullToAnchor()
    {
        if (!joint2D.enabled || joint2D.connectedBody == null) return;

        Vector2 anchorPos = joint2D.connectedBody.transform.TransformPoint(joint2D.connectedAnchor);
        float distance = Vector2.Distance(transform.position, anchorPos);

        joint2D.distance = Mathf.MoveTowards(joint2D.distance, 0.5f, pullForce * Time.deltaTime);

        if (distance < 1f)  // 연결된 대상에 너무 가까워지면
        {
            rb.velocity = Vector2.zero;
            isPulling = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        // 현재 연결된 대상이 Obstacle인지 여부
        bool isConnectedToObstacle = false;

        if (joint2D.enabled && joint2D.connectedBody != null)
        {
            GameObject connectedObj = joint2D.connectedBody.gameObject;
            isConnectedToObstacle = connectedObj.CompareTag("Obstacle");
        }

        // 부딪힌 대상이 Obstacle인지 여부
        bool isHitObstacle = other.gameObject.CompareTag("Obstacle");

        // 조건:둘 다 Obstacle이거나 둘 다 Obstacle이 아닐 땐 유지
        if (isConnectedToObstacle != isHitObstacle)
        {
            Debug.Log($"[Collision] {other.gameObject.name}와 충돌 → 줄 해제");
            ReleaseGrapple();
        }
    }
    private Vector2 GetVisualAnchor()
    {
        Vector2 anchorPos = joint2D.connectedBody.transform.TransformPoint(joint2D.connectedAnchor);
        Vector2 dirToAnchor = (anchorPos - (Vector2)transform.position).normalized;

        Vector2 visualPos = anchorPos + dirToAnchor * visualOffset;

        return visualPos;
    }
}