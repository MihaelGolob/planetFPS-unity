using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentSpawner : MonoBehaviour {
    [SerializeField] private List<GameObject> trees;
    [SerializeField] private int treeCount;
    [SerializeField] private List<GameObject> rocks;
    [SerializeField] private int rockCount;
    [SerializeField] private List<GameObject> grass;
    [SerializeField] private int grassCount;

    private float _radius;
    
    public void Spawn() {
        _radius = GetComponent<SphereCollider>().radius * transform.localScale.x;
        // destroy all children
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        // spawn new objects
        SpawnObjects(trees, treeCount);
        SpawnObjects(rocks, rockCount);
        SpawnObjects(grass, grassCount);
    }
    
    public void Clear() {
        // destroy all children
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
    
    private void SpawnObjects(List<GameObject> objectList, int count) {
        for (var i = 0; i < count; i++) {
            var randomPrefab = objectList[Random.Range(0, objectList.Count)];
            CreateObjectOnSphere(randomPrefab, Random.Range(0, 2*Mathf.PI), Random.Range(0, Mathf.PI), Random.Range(0, 360));
        }
    }

    private void CreateObjectOnSphere(GameObject o, float theta, float phi, float randomAngle) {
        var sinPhi = Mathf.Sin(phi);
        var cosPhi = Mathf.Cos(phi);
        var sinTheta = Mathf.Sin(theta);
        var cosTheta = Mathf.Cos(theta);

        var offset = new Vector3(_radius * sinTheta * cosPhi, _radius * sinTheta * sinPhi, _radius * cosTheta);
        var position = gameObject.transform.position + offset;
        
        var gravityDir = (gameObject.transform.position - position).normalized;
        var rotation = Quaternion.FromToRotation(Vector3.down, gravityDir);
        
        var randomScale = Random.Range(0.8f, 1.2f);
        
        var obj = Instantiate(o, position, rotation);
        obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        obj.transform.Rotate(new Vector3(0, randomAngle, 0));
        obj.transform.parent = gameObject.transform;
    }
}
