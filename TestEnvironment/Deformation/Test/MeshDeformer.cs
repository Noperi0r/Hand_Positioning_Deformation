using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices;
    Vector3[] vertexVelocities;

    [SerializeField] float springForce = 20f;
    [SerializeField] float damping = 5f;
    float uniformScale = 1f;

    // Start is called before the first frame update
    void Start()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;

        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        for(int i = 0; i < originalVertices.Length; ++i)
        {
            displacedVertices[i] = originalVertices[i];
        }

        vertexVelocities = new Vector3[originalVertices.Length];
    }

    // Update is called once per frame
    void Update()
    {
        uniformScale = transform.localScale.x;

        for(int i = 0; i<displacedVertices.Length; ++i)
        {
            UpdateVertex(i);
        }
        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        Debug.DrawLine(Camera.main.transform.position, point);
        
        for(int i=0; i<displacedVertices.Length; ++i)
        {
            AddForceToVertex(i, point, force);
        }
    }

    void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = transform.TransformPoint(displacedVertices[i]) - point;
        pointToVertex *= uniformScale;
        Debug.DrawRay(point, pointToVertex, Color.green);

        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude); // Modified inverse square law 

        float velocity = attenuatedForce * Time.deltaTime; // magnitute is ignored as if it were one for each vertex.
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }
    void UpdateVertex(int i)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 PosDiffVector = displacedVertices[i] - originalVertices[i];
        PosDiffVector *= uniformScale;
        
        velocity -= springForce * PosDiffVector * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime; // v - v(damp * deltaTime)

        vertexVelocities[i] = velocity;
        //displacedVertices[i] += velocity * Time.deltaTime; // pos = vel * t 
        displacedVertices[i] += velocity * (Time.deltaTime / uniformScale); // pos = vel * t 
    }
}
