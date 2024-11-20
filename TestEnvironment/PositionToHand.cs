using Oculus.Interaction.PoseDetection;
using OVR.OpenVR;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Experimental.GraphView.Port;


public class PositionToHand : MonoBehaviour
{
    // TODO: Get normalized directional vectors of hand(right, forward, and up using cross product)

    enum ETargetingMode
    {
        Default,
        Average
    }

    enum EObjAxisVector
    {
        X_Red,
        Y_Green,
        Z_Blue
    }

    [Header("Hand Objects In Scene")]
    [SerializeField] OVRSkeleton _handSkeleton;
    [SerializeField] SkinnedMeshRenderer _handMesh;

    [Space]
    [SerializeField] bool _mode_usingActiveState;

    [Header("Hand Targeting Property")]
    [SerializeField] ETargetingMode _softMode;
    [SerializeField] EObjAxisVector _objectUpVector;
    [SerializeField] EObjAxisVector _objectFrontVector;

    //[SerializeField] OVRSkeleton.BoneId _targetBoneId;
    //[SerializeField] List<OVRSkeleton.BoneId> _targetBonesList;

    // IMPORTANT: Need to be set in clockwise order 
    [Header("Clockwise Order Target Bones")]
    [SerializeField] OVRSkeleton.BoneId[] _targetBonesArray = new OVRSkeleton.BoneId[3];

    [Header("Position Adjusting")]
    [SerializeField] Vector3 _adjustedPos;

    Transform _initialTransform;
    float _initialColorAlpha;

    bool _isActiveStateOn;

    void Start()
    {
        if(_mode_usingActiveState) // For left hand 
        {
            _initialTransform = new GameObject().transform;

            _initialTransform.position = transform.position;
            _initialTransform.rotation = transform.rotation;
            
            _initialColorAlpha = _handMesh.material.GetFloat("_Opacity");
        }
    }

    void Update()
    {
        if (_mode_usingActiveState) // For left hand 
        {
            if (_isActiveStateOn)
            {
                SetObjTransform(_softMode);

                //SetTargetTransform(_targetBoneId);
                //TransformPositionToTarget();
            }
        }
        else // For right hand 
        {
            SetObjTransform(_softMode);

            //SetTargetTransform(_targetBoneId);
            //TransformPositionToTarget();
        }
    }

    public void ActiveStateOn()
    {
        _isActiveStateOn = true;

        ChangeHandAlpha(0f);
    }

    public void ActiveStateOff()
    {
        _isActiveStateOn = false;

        ChangeHandAlpha(_initialColorAlpha);

        transform.position = _initialTransform.position;
        transform.rotation = _initialTransform.rotation;
    }

    void SetObjTransform(ETargetingMode softMode)
    {
        switch (softMode)
        {
            case ETargetingMode.Average:
                // thumb 0,1, index 1
                List<Transform> targetBoneTransforms = new List<Transform>();
                List<OVRSkeleton.BoneId> targetBoneIds = new List<OVRSkeleton.BoneId>();

                // Get each target bone transform 
                foreach(var bone in _handSkeleton.Bones)
                {
                    foreach(OVRSkeleton.BoneId targetBone in _targetBonesArray)
                    {
                        if(bone.Id == targetBone)
                        {
                            targetBoneTransforms.Add(bone.Transform);
                            targetBoneIds.Add(bone.Id);
                        }
                    }

                    if (targetBoneTransforms.Count == _targetBonesArray.Length)
                    {
                        break;
                    } 
                }

                // Get average transform and Set targettransform
                if(targetBoneTransforms.Count == 3)
                {
                    Vector3 averagePos = new Vector3();
                    foreach(Transform targetTransform in targetBoneTransforms)
                    {
                        averagePos += targetTransform.position;

                    }
                    averagePos /= targetBoneTransforms.Count;

                    Vector3 dirVec1 = targetBoneTransforms[targetBoneIds.IndexOf(_targetBonesArray[1])].position
                                    - targetBoneTransforms[targetBoneIds.IndexOf(_targetBonesArray[0])].position;

                    Vector3 dirVec2 = targetBoneTransforms[targetBoneIds.IndexOf(_targetBonesArray[2])].position
                                    - targetBoneTransforms[targetBoneIds.IndexOf(_targetBonesArray[0])].position;

                    Vector3 handUpVec = Vector3.Cross(dirVec1, dirVec2).normalized; // LHS
                    Vector3 handFrontVec = (dirVec1 + dirVec2).normalized;
                    Vector3 handRightVec = Vector3.Cross(handUpVec, handFrontVec).normalized;

                    // Set result target transform
                    transform.position = averagePos;

                    if(_objectUpVector == _objectFrontVector)
                    {
                        Debug.Log("WRONG OBJECT AXIS SETUP");
                    }

                    Vector3 resultUpVec = Vector3.zero, resultForwardVec = Vector3.zero, resultRightVec = Vector3.zero;

                    switch (_objectUpVector)
                    {
                        case EObjAxisVector.X_Red:
                            resultRightVec = handUpVec;
                            break;
                        case EObjAxisVector.Y_Green:
                            resultUpVec = handUpVec;
                            break;
                        case EObjAxisVector.Z_Blue:
                            resultForwardVec = handUpVec;
                            break;
                    }

                    switch (_objectFrontVector)
                    {
                        case EObjAxisVector.X_Red:
                            resultRightVec = handFrontVec;
                            break;
                        case EObjAxisVector.Y_Green:
                            resultUpVec = handFrontVec;
                            break;
                        case EObjAxisVector.Z_Blue:
                            resultForwardVec = handFrontVec;
                            break;
                    }

                    if (_objectUpVector == EObjAxisVector.X_Red && _objectFrontVector == EObjAxisVector.Y_Green)
                        resultForwardVec = handRightVec;
                    else if (_objectUpVector == EObjAxisVector.X_Red && _objectFrontVector == EObjAxisVector.Z_Blue)
                        resultUpVec = handRightVec;
                    else
                        resultRightVec = handRightVec;

                    if (resultForwardVec == Vector3.zero)
                        resultForwardVec = Vector3.Cross(resultRightVec, resultUpVec).normalized;
                    else if (resultUpVec == Vector3.zero)
                        resultUpVec = Vector3.Cross(resultForwardVec, resultRightVec).normalized;

                    transform.rotation = Quaternion.LookRotation(resultForwardVec, resultUpVec);
                }
                else
                {
                    Debug.Log("ERROR: No 3 bones found");
                }

                break;

            case ETargetingMode.Default: // Retarget object to the target bone based on its unique id
                if (_mode_usingActiveState)
                {
                    foreach (var bone in _handSkeleton.Bones)
                    {
                        if (bone.Id == _targetBonesArray[0])
                        {
                            transform.position = bone.Transform.position;
                            transform.rotation = bone.Transform.rotation;
                            return;
                        }
                    }
                }
                else
                {
                    foreach (var bone in _handSkeleton.Bones)
                    {
                        if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                        {
                            transform.position = bone.Transform.position;
                            transform.rotation = bone.Transform.rotation;
                            return;
                        }
                    }
                }

                break;
        }

        transform.position += _adjustedPos;
    }

    void ChangeHandAlpha(float alpha)
    {
        // Normal material
        /*
        Color color = handMesh.material.color;
        color.a = alpha;
        handMesh.material.color = color;
        */

        // Oculus shader material (OculusHand.shader)
        _handMesh.material.SetFloat("_Opacity", alpha); // Changes the property in OculusHand shader    
    }

}
