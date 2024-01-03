using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class Zipline : MonoBehaviour {
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;
    [SerializeField] private Transform start_planet;
    [SerializeField] private Transform end_planet;

    [SerializeField] private float speed = 1f;
    [Header("Cable")]
    [SerializeField] private float cableRadius = 0.1f;
    [SerializeField] private Material cableMaterial;
    [Header("Colliders")] 
    [SerializeField] private Collider startCollider;
    [SerializeField] private Collider endCollider;
    
    private GameObject _cableObject;

    public void CreateCableMesh() {
        Clear();
        // create a round cable mesh that starts in start and ends in end
        _cableObject = new GameObject("Cable");
        _cableObject.transform.parent = gameObject.transform;
        _cableObject.transform.localPosition = Vector3.zero;
        var meshFilter = _cableObject.AddComponent<MeshFilter>();
        var meshRenderer =_cableObject.AddComponent<MeshRenderer>();
        
        var startPos = start.position;
        var endPos = end.position;

        var vertices = new Vector3[8];
        var triangles = new int[24];

        vertices[0] = startPos + new Vector3(-cableRadius, -cableRadius, 0);
        vertices[1] = startPos + new Vector3(-cableRadius, cableRadius, 0);
        vertices[2] = startPos + new Vector3(cableRadius, cableRadius, 0);
        vertices[3] = startPos + new Vector3(cableRadius, -cableRadius, 0);
        
        vertices[4] = endPos + new Vector3(-cableRadius, -cableRadius, 0);
        vertices[5] = endPos + new Vector3(-cableRadius, cableRadius, 0);
        vertices[6] = endPos + new Vector3(cableRadius, cableRadius, 0);
        vertices[7] = endPos + new Vector3(cableRadius, -cableRadius, 0);

        // bottom
        triangles[0] = 0;
        triangles[1] = 7;
        triangles[2] = 4;
        triangles[3] = 3;
        triangles[4] = 7;
        triangles[5] = 0;
        
        // right
        triangles[6] = 3;
        triangles[7] = 6;
        triangles[8] = 7;
        triangles[9] = 2;
        triangles[10] = 6;
        triangles[11] = 3;
        
        triangles[12] = 2;
        triangles[13] = 5;
        triangles[14] = 6;
        triangles[15] = 1;
        triangles[16] = 5;
        triangles[17] = 2;
        
        triangles[18] = 1;
        triangles[19] = 4;
        triangles[20] = 5;
        triangles[21] = 0;
        triangles[22] = 4;
        triangles[23] = 1;
        
        Mesh cableMesh = new Mesh();
        meshFilter.sharedMesh = cableMesh;

        cableMesh.vertices = vertices;
        cableMesh.triangles = triangles;
        cableMesh.RecalculateNormals();

        meshRenderer.sharedMaterial = cableMaterial;

        _cableObject.transform.position = Vector3.zero;
    }

    public void Clear() {
        if (_cableObject != null) {
            DestroyImmediate(_cableObject);
        }
    }

    public (Vector3 start, Vector3 end, float speed, Transform new_planet) GetZiplineInfo(Collider enterCollider) {
        if (enterCollider == startCollider) {
            return (start.position, end.position, speed, end_planet);
        } else if (enterCollider == endCollider) {
            return (end.position, start.position, speed, start_planet);
        } else {
            return (Vector3.zero, Vector3.zero, 0, null);
        }
    }
}
