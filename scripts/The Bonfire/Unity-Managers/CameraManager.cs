// Filename: SmoothDampCamera.cs
// Author: 0xFirekeeper
// Description: Uses Transform References and Smooth Damps the position and rotation
//              Quaternion using Vector3.SmoothDamp on its values feels really nice.

using System.Collections;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using System;

public enum CameraTransforms
{
    View1,
    View2,
    View3
}

[System.Serializable]
public class CameraViews : SerializableDictionaryBase<CameraTransforms, Transform> { }

public class CameraManager : MonoBehaviour
{
    public float smoothDampFactor = 20f;
    public CameraViews cameraViews;

    public static CameraManager Instance;

    private CameraTransforms currentTransform;
    private Coroutine smoothDampRoutine;
    private Vector3 velocity;
    private Quaternion quaternion;

    private bool explorationMode;

    private void Awake()
    {
        Instance = this;

        transform.position = cameraViews[CameraTransforms.View3].position;
        transform.rotation = cameraViews[CameraTransforms.View3].rotation;
    }

    public void InitializeSmoothDampCamera()
    {
        currentTransform = CameraTransforms.View1;
    }

    public void MoveToView(CameraTransforms camTransform)
    {
        if (smoothDampRoutine != null)
            StopCoroutine(smoothDampRoutine);

        smoothDampRoutine = StartCoroutine(MoveToViewRoutine(camTransform));

        currentTransform = camTransform;
    }

    IEnumerator MoveToViewRoutine(CameraTransforms camTransform)
    {
        Transform targetTransform = cameraViews[camTransform];

        while (!Transform.Equals(this.transform, targetTransform))
        {
            transform.position = Vector3.SmoothDamp(transform.position,
                        targetTransform.position, ref velocity, smoothDampFactor * Time.deltaTime);

            transform.rotation = Extensions.QuaternionSmoothDamp(transform.rotation,
                targetTransform.rotation, ref quaternion, smoothDampFactor * Time.deltaTime);

            yield return null;
        }

    }

    internal void DisableCameraManager()
    {
        if (smoothDampRoutine != null)
            StopCoroutine(smoothDampRoutine);

        this.enabled = false;
    }
}
