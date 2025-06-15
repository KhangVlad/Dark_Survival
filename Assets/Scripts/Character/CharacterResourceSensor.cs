
using UnityEngine;

public class CharacterResourceSensor : MonoBehaviour
{
    [Header("Sensor Settings")]
    [SerializeField] private float sensorRadius = 5f;
    [SerializeField] private LayerMask resourceLayerMask = -1;
    [SerializeField] private float updateInterval = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool showDebugLogs = false;

    // Events
    public System.Action<ResourceBehavior> OnTargetChanged;

    public ResourceBehavior currentTarget;
    private float lastUpdateTime;

    // Properties
    public ResourceBehavior CurrentTarget => currentTarget;
    public bool HasTarget => currentTarget != null;

    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateTarget();
            lastUpdateTime = Time.time;
        }
    }

    private void UpdateTarget()
    {
        // Check if current target is still valid and in range
        if (currentTarget != null)
        {
            // Check if the target still exists and is active
            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                if (showDebugLogs) Debug.Log("Current target became invalid, clearing");
                SetTarget(null);
            }
            else if (Vector3.Distance(transform.position, currentTarget.transform.position) > sensorRadius)
            {
                if (showDebugLogs) Debug.Log("Current target moved out of range, clearing");
                SetTarget(null);
            }
            else
            {
                // Current target is still valid, keep it
                return;
            }
        }

        // Find new target if we don't have one
        ResourceBehavior newTarget = FindClosestResource();
        SetTarget(newTarget);
    }

    private bool IsValidTarget(ResourceBehavior resource)
    {
        return resource != null &&
               resource.gameObject != null &&
               resource.gameObject.activeInHierarchy &&
               resource.ResourceNode != null;
    }

    private ResourceBehavior FindClosestResource()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sensorRadius, resourceLayerMask);

        ResourceBehavior closestResource = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            ResourceBehavior resource = collider.GetComponent<ResourceBehavior>();
            if (IsValidTarget(resource))
            {
                float distance = Vector3.Distance(transform.position, resource.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestResource = resource;
                }
            }
        }

        return closestResource;
    }

    private void SetTarget(ResourceBehavior newTarget)
    {
        // More detailed comparison to prevent unnecessary events
        if (IsSameTarget(currentTarget, newTarget))
        {
            if (showDebugLogs) Debug.Log("Target is the same, skipping event");
            return;
        }

        ResourceBehavior previousTarget = currentTarget;
        currentTarget = newTarget;
        OnTargetChanged?.Invoke(currentTarget);
    }

    private bool IsSameTarget(ResourceBehavior target1, ResourceBehavior target2)
    {
        //check id
        if (target1 == null && target2 == null) return true;
        if (target1 == null || target2 == null) return false;
        return target1.ResourceNode != null && target2.ResourceNode != null &&
               target1.ResourceNode.entityID == target2.ResourceNode.entityID &&
               target1.gameObject == target2.gameObject;
    }

    // Public methods
    public void SetSensorRadius(float radius)
    {
        sensorRadius = radius;
    }

    public void ForceUpdateTarget()
    {
        UpdateTarget();
    }

    public float GetDistanceToTarget()
    {
        if (currentTarget == null) return float.MaxValue;
        return Vector3.Distance(transform.position, currentTarget.transform.position);
    }

    public Vector3 GetDirectionToTarget()
    {
        if (currentTarget == null) return Vector3.zero;
        return (currentTarget.transform.position - transform.position).normalized;
    }

    // Debug visualization
    // private void OnDrawGizmos()
    // {
    //     if (!showDebugGizmos) return;

    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, sensorRadius);

    //     if (currentTarget != null)
    //     {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawLine(transform.position, currentTarget.transform.position);
    //         Gizmos.DrawWireCube(currentTarget.transform.position, Vector3.one * 0.5f);

    //         // Draw resource info
    //         if (currentTarget.ResourceNode != null)
    //         {
    //             Vector3 labelPos = currentTarget.transform.position + Vector3.up * 2f;
    //             UnityEditor.Handles.Label(labelPos, currentTarget.ResourceNode.entityID.ToString());
    //         }
    //     }
    // }
}