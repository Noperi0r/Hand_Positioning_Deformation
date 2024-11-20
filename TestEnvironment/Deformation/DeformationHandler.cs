using Meta.WitAi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformationHandler : MonoBehaviour
{
    ContactPoint[] collidedPoints;
    ObjectDeformer deformer;

    [SerializeField] float pushForce = 10f;

/*    void OnTriggerEnter(Collider other)
    {
        other.GetComponent<ObjectDeformer>()?.InvokeUpdateCollider();
    }*/

    void OnTriggerStay(Collider other)
    {
        // If it's not null it calls the method or return value. (?. operator)
        other.GetComponent<ObjectDeformer>()?.AddDeformingForce(transform.position, pushForce);
        // TODO: point vector should be bone transform of OculusHand
    }

    /*    void OnTriggerExit(Collider other)
        {
            other.GetComponent<ObjectDeformer>()?.CancelUpdateCollider();
        }*/


}
