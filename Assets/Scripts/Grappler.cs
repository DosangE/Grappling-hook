using System.Collections;
using UnityEngine;

public class Grappler : MonoBehaviour
{
    [Header("컴포넌트 설정")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private DistanceJoint2D distanceJoint;
    [SerializeField] private Rigidbody2D rb;

    [Header("가속 수치")]
    [SerializeField] private float boostForce;

    [Header("지속 당김 수치")]
    [SerializeField] private float pullForce;

    [Header("감지 대상 레이어")]
    [SerializeField] private LayerMask grappleLayer;

    [Header("Grapple 거리")]
    [SerializeField] private float maxGrappleDistance;
    [SerializeField] private float minGrappleDistance;
    [SerializeField] private float minDistance;

    private float fixedGrappleDistance;
    private bool isPulling = false;

    private Coroutine distanceLoggerCoroutine;

    private void Awake()
    {
        if (lineRenderer == null) Debug.LogError("LineRenderer가 설정되지 않았습니다.");
        if (distanceJoint == null) Debug.LogError("DistanceJoint2D가 설정되지 않았습니다.");
        else distanceJoint.enabled = false;

        if (rb == null) Debug.LogError("Rigidbody2D가 설정되지 않았습니다.");
    }

    private void Start()
    {
        distanceLoggerCoroutine = StartCoroutine(LogGrappleDistanceRoutine());
    }

    private IEnumerator LogGrappleDistanceRoutine()
    {
        while (true)
        {
            if (distanceJoint.enabled && distanceJoint.connectedBody != null)
            {
                float currentDistance = distanceJoint.distance;
                float actualDistance = Vector2.Distance(transform.position, distanceJoint.connectedBody.position);
                Debug.Log($"[1초 간격] 설정된 줄 길이: {currentDistance:F3}, 실제 거리: {actualDistance:F3}");
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void Update()
    {
        if (GameManager.instance.CurrentGameState != GameManager.GameState.Playing)
            return;

        InputGrapple();
        ReleaseGrapple();
        UpdateGrappleLine();
        InputBoost();
        if (isPulling)
            AutoPullToAnchor();
        CheckTargetOut();
    }

    private void InputGrapple()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mouseWorldPos - (Vector2)transform.position;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, maxGrappleDistance, grappleLayer);

            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                float hitDistance = Vector2.Distance(transform.position, hit.point);
                Rigidbody2D targetRb = hit.collider.attachedRigidbody;

                if (targetRb != null && hitDistance > minGrappleDistance && hitDistance <= maxGrappleDistance)
                {
                    distanceJoint.connectedBody = targetRb;
                    distanceJoint.autoConfigureConnectedAnchor = false;
                    distanceJoint.connectedAnchor = targetRb.transform.InverseTransformPoint(hit.point); // ✅ 클릭한 위치에 연결

                    distanceJoint.autoConfigureDistance = false;
                    distanceJoint.maxDistanceOnly = false;
                    distanceJoint.distance = hitDistance;
                    distanceJoint.enabled = true;

                    fixedGrappleDistance = hitDistance;

                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, hit.point); // ✅ 클릭 지점에 줄 끝 시각화
                    lineRenderer.enabled = true;
                }
            }
        }
    }


    private void ReleaseGrapple()
    {
        if (isPulling) return;

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            distanceJoint.enabled = false;
            lineRenderer.enabled = false;
        }
    }

    private void UpdateGrappleLine()
    {
        if (!distanceJoint.enabled || distanceJoint.connectedBody == null) return;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, distanceJoint.connectedBody.transform.TransformPoint(distanceJoint.connectedAnchor)); // ✅ 정확한 위치 추적
    }


    private void InputBoost()
    {
        if (!distanceJoint.enabled || distanceJoint.connectedBody == null) return;

        lineRenderer.SetPosition(0, transform.position);

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Vector2 toAnchor = ((Vector2)distanceJoint.connectedBody.position - (Vector2)transform.position);
            Vector2 tangentDir = new Vector2(toAnchor.y, -toAnchor.x).normalized;
            rb.velocity = tangentDir * boostForce;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            isPulling = true;
        }
    }

    private void AutoPullToAnchor()
    {
        if (!distanceJoint.enabled || distanceJoint.connectedBody == null)
            return;

        Vector2 toTarget = ((Vector2)distanceJoint.connectedBody.position - (Vector2)transform.position);
        float distance = toTarget.magnitude;
        Vector2 dir = toTarget.normalized;

        if (distance > minDistance)
        {
            float force = pullForce * (distance - minDistance);

            // 한 번만 강하게 당김
            distanceJoint.distance = Mathf.Min(distanceJoint.distance, minDistance);
            fixedGrappleDistance = distanceJoint.distance;

            rb.AddForce(dir * force, ForceMode2D.Impulse);

            isPulling = false;
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            distanceJoint.distance = minDistance;
            fixedGrappleDistance = minDistance;
            isPulling = false;
        }
    }

    private void CheckTargetOut()
    {
        if (!lineRenderer.enabled || distanceJoint.connectedBody == null) return;

        Renderer targetRenderer = distanceJoint.connectedBody.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            float objectLeftX = targetRenderer.bounds.min.x;
            float cameraLeftX = Camera.main.ViewportToWorldPoint(Vector3.zero).x;

            if (objectLeftX < cameraLeftX)
            {
                distanceJoint.enabled = false;
                lineRenderer.enabled = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isPulling)
        {
            isPulling = false;
            distanceJoint.enabled = false;
            lineRenderer.enabled = false;
            rb.velocity = Vector2.zero;
        }
    }
}
