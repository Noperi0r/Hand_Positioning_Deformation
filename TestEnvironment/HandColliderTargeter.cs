using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandColliderTargeter : MonoBehaviour
{
    //[SerializeField] PositionToHand positionToHand;

    [SerializeField] OVRSkeleton handSkeleton;
    [SerializeField] OVRSkeleton.BoneId targetBoneId;
    SphereCollider handCollider;

    // Start is called before the first frame update
    void Start()
    {
        handCollider = gameObject.AddComponent<SphereCollider>();
        handCollider.radius = 0.5f;
    }

    void FixedUpdate()
    {
        AttachColliderToBone();
    }

    Transform GetTargetedBoneTransform(OVRSkeleton.BoneId targetBone)
    {
        foreach (var bone in handSkeleton.Bones)
        {
            if (bone.Id == targetBone)
            {
                //targetTransform = bone.Transform;
                return bone.Transform;
            }
        }
        return null;
    }

    void AttachColliderToBone()
    {
        //handCollider.center = transform.InverseTransformPoint(positionToHand.GetBoneTransform(targetBoneId).position);
        handCollider.center = transform.InverseTransformPoint(GetTargetedBoneTransform(targetBoneId).position);
        //handCollider.center = transform.InverseTransformPoint(transform.position);
    }
}
