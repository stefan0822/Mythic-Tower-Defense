using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public Transform[] waypoints;
    public Color pathColor = Color.cyan;
    public float pointSize = 2.5f;

    private void Awake()
    {
        RefreshWaypoints();
    }

    public void RefreshWaypoints()
    {
        waypoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints[i] = transform.GetChild(i);
        }
    }

    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        if (count == 0) return;

        Gizmos.color = pathColor;
        for (int i = 0; i < count; i++)
        {
            Transform current = transform.GetChild(i);
            if (current == null) continue;

            // Primul punct (poarta) e verde, ultimul (centrul) e rosu, restul cyan
            if (i == 0) Gizmos.color = Color.green;
            else if (i == count - 1) Gizmos.color = Color.red;
            else Gizmos.color = pathColor;

            Gizmos.DrawSphere(current.position, pointSize);

            if (i < count - 1)
            {
                Transform next = transform.GetChild(i + 1);
                if (next != null)
                {
                    Gizmos.color = pathColor;
                    Gizmos.DrawLine(current.position, next.position);
                }
            }
        }
    }
}