using UnityEngine;

public class FieldOfView : MonoBehaviour {

    [SerializeField] private LayerMask layerMask;
    private Mesh _mesh;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private float fov = 90f;
    [SerializeField] private float viewDistance = 50f;
    private const int RayCount = 50;
    private Vector3 _origin = Vector3.zero;
    private float _startingAngle;

    private void Start() {
        _mesh = new Mesh();
        meshFilter.mesh = _mesh;
    }

    private void LateUpdate() {
        var angle = _startingAngle;
        var angleIncrease = fov / RayCount;

        Vector3[] vertices = new Vector3[RayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[RayCount * 3];

        vertices[0] = _origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (var i = 0; i <= RayCount; i++) {
            Vector3 vertex;
            var direction = GetVectorFromAngle(angle);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_origin, direction, viewDistance, layerMask);
            if (raycastHit2D.collider == null) {
                // No hit
                vertex = _origin + direction * viewDistance;
            } else {
                // Hit object
                vertex = raycastHit2D.point;
                //if (raycastHit2D.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
                //Debug.Log("Enemy start combat!");
            }
            vertices[vertexIndex] = vertex;

            if (i > 0) {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }


        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        _mesh.bounds = new Bounds(_origin, Vector3.one * 1000f);
    }

    public void SetOrigin(Vector3 origin) {
        _origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection) {
        _startingAngle = GetAngleFromVectorFloat(aimDirection) + fov / 2f;
    }

    public void SetFoV(float fieldOfView) {
        fov = fieldOfView;
    }

    public void SetViewDistance(float distance) {
        viewDistance = distance;
    }

    private static Vector3 GetVectorFromAngle(float angle) {
        var angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    private static float GetAngleFromVectorFloat(Vector3 dir) {
        dir = dir.normalized;
        var n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }
}
