using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private bool snapToTargetOnEnable = true;

    [Header("Smoothing")]
    [Min(0f)]
    [SerializeField] private float smoothTime = 0.16f;
    [Min(0f)]
    [SerializeField] private float maxFollowSpeed = 40f;

    [Header("Deadzone")]
    [SerializeField] private bool useDeadzone = true;
    [Min(0f)]
    [SerializeField] private float deadzoneWidth = 0.75f;
    [Min(0f)]
    [SerializeField] private float deadzoneHeight = 0.45f;

    [Header("Map Limits")]
    [SerializeField] private bool useMapLimits = true;
    [SerializeField] private Vector2 mapMin = new Vector2(-20f, -12f);
    [SerializeField] private Vector2 mapMax = new Vector2(20f, 12f);

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private Camera _camera;
    private Vector2 _velocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        _velocity = Vector2.zero;

        if (snapToTargetOnEnable)
            SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 currentPosition = transform.position;
        Vector3 desiredPosition = GetDesiredPosition(currentPosition);

        if (smoothTime <= 0f)
        {
            transform.position = desiredPosition;
            return;
        }

        Vector2 smoothedPosition = Vector2.SmoothDamp(
            currentPosition,
            desiredPosition,
            ref _velocity,
            smoothTime,
            maxFollowSpeed,
            Time.deltaTime);

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, desiredPosition.z);
    }

    public void SetTarget(Transform newTarget, bool snapImmediately = true)
    {
        target = newTarget;
        _velocity = Vector2.zero;

        if (snapImmediately)
            SnapToTarget();
    }

    public void SnapToTarget()
    {
        if (target == null) return;

        transform.position = GetClampedPosition(target.position + targetOffset);
        _velocity = Vector2.zero;
    }

    private Vector3 GetDesiredPosition(Vector3 currentPosition)
    {
        Vector3 targetPosition = target.position + targetOffset;

        if (!useDeadzone)
            return GetClampedPosition(targetPosition);

        float halfWidth = deadzoneWidth * 0.5f;
        float halfHeight = deadzoneHeight * 0.5f;

        float desiredX = currentPosition.x;
        float desiredY = currentPosition.y;
        float deltaX = targetPosition.x - currentPosition.x;
        float deltaY = targetPosition.y - currentPosition.y;

        if (Mathf.Abs(deltaX) > halfWidth)
            desiredX = targetPosition.x - Mathf.Sign(deltaX) * halfWidth;

        if (Mathf.Abs(deltaY) > halfHeight)
            desiredY = targetPosition.y - Mathf.Sign(deltaY) * halfHeight;

        return GetClampedPosition(new Vector3(desiredX, desiredY, targetPosition.z));
    }

    private Vector3 GetClampedPosition(Vector3 position)
    {
        if (!useMapLimits)
            return position;

        float minX = mapMin.x;
        float maxX = mapMax.x;
        float minY = mapMin.y;
        float maxY = mapMax.y;

        if (_camera != null && _camera.orthographic)
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            minX += halfWidth;
            maxX -= halfWidth;
            minY += halfHeight;
            maxY -= halfHeight;
        }

        if (minX > maxX)
            position.x = (mapMin.x + mapMax.x) * 0.5f;
        else
            position.x = Mathf.Clamp(position.x, minX, maxX);

        if (minY > maxY)
            position.y = (mapMin.y + mapMax.y) * 0.5f;
        else
            position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private void OnValidate()
    {
        deadzoneWidth = Mathf.Max(0f, deadzoneWidth);
        deadzoneHeight = Mathf.Max(0f, deadzoneHeight);
        smoothTime = Mathf.Max(0f, smoothTime);
        maxFollowSpeed = Mathf.Max(0f, maxFollowSpeed);

        if (mapMin.x > mapMax.x)
        {
            float minX = mapMax.x;
            float maxX = mapMin.x;
            mapMin.x = minX;
            mapMax.x = maxX;
        }

        if (mapMin.y > mapMax.y)
        {
            float minY = mapMax.y;
            float maxY = mapMin.y;
            mapMin.y = minY;
            mapMax.y = maxY;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadzoneWidth, deadzoneHeight, 0f));

        if (!useMapLimits) return;

        Gizmos.color = Color.cyan;
        Vector2 center = (mapMin + mapMax) * 0.5f;
        Vector2 size = mapMax - mapMin;
        Gizmos.DrawWireCube(center, size);
    }
}
