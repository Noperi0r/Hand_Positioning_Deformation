using Oculus.Interaction.DebugTree;
using Oculus.Interaction.PoseDetection;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class ObjectDeformer : MonoBehaviour
{
    [SerializeField] bool _isDebugging;

    Mesh _mesh;
    Vector3[] _originalVertices;
    Vector3[] _deformedVertices;
    Vector3[] _verticesVelocity;

    [SerializeField] int _attenFormula_Base = 50;
    [SerializeField] float _restorationForce = 20f; // Restore vertices into original position 
    [SerializeField] float _damping = 7.5f;
    float _uniformScale = 1f;

    // Start is called before the first frame update
    void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;

        _originalVertices = _mesh.vertices; // Shallow copy 

        _deformedVertices = new Vector3[_originalVertices.Length]; // Deep copy
        for (int i = 0; i < _originalVertices.Length; ++i)
        {
            _deformedVertices[i] = _originalVertices[i];
        }

        _verticesVelocity = new Vector3[_deformedVertices.Length];
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(transform.up, .7f);

        _uniformScale = transform.localScale.x;

        for (int i = 0; i < _deformedVertices.Length; ++i)
        {
            _deformedVertices[i] = transform.TransformPoint(_deformedVertices[i]);
            UpdateVertexPos(i);
            _deformedVertices[i] = transform.InverseTransformPoint(_deformedVertices[i]);
        }
        _mesh.vertices = _deformedVertices;
        _mesh.RecalculateNormals();
    }

/*    public void InvokeUpdateCollider()
    {
        InvokeRepeating("UpdateMeshCollider", 0f, 0.01f);
        Debug.Log("Invoke");
    }

    public void CancelUpdateCollider()
    {
        GetComponent<MeshCollider>().sharedMesh.vertices = originalVertices;
        CancelInvoke("UpdateMeshCollider");

        Debug.Log("Canceled");
    }

    void UpdateMeshCollider()
    {
        DestroyImmediate(GetComponent<MeshCollider>());
        var collider = this.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.convex = true;
        collider.isTrigger = true;
    }*/

    public void AddDeformingForce(Vector3 point, float force)
    {
        for(int i=0; i<_deformedVertices.Length; ++i)
        {
            UpdateVertexVelocity(point, force, i);
        }
    }

    void UpdateVertexVelocity(Vector3 point, float force, int i)
    {
        Vector3 pointToVertex = transform.TransformPoint(_deformedVertices[i]) - point;
        pointToVertex /= _uniformScale;

        if(_isDebugging)
            Debug.DrawRay(point, pointToVertex, Color.green);

        // Improved version of inverse sqaure law 
        //float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float attenuatedForce = force / Mathf.Pow(_attenFormula_Base, pointToVertex.sqrMagnitude);

        // v1 = v0 + at, F = ma, a = f/m, m is neglectable per vertex.
        float velocity = attenuatedForce * Time.deltaTime;
        _verticesVelocity[i] = _verticesVelocity[i] + pointToVertex.normalized * velocity;
    }

    void UpdateVertexPos(int i)
    {   
        Vector3 velocity = _verticesVelocity[i];

        // Vertices restoration with spring force 
        Vector3 posDiffVector = _deformedVertices[i] - transform.TransformPoint(_originalVertices[i]);
        //posDiffVector *= _uniformScale;
        velocity = velocity - _restorationForce * posDiffVector * Time.deltaTime;

        // Velocity damping 
        velocity = velocity * (1f - _damping * Time.deltaTime);

        _verticesVelocity[i] = velocity;

        //_deformedVertices[i] = _deformedVertices[i] + velocity * (Time.deltaTime / _uniformScale);
        _deformedVertices[i] = _deformedVertices[i] + velocity * Time.deltaTime;
    }
}
