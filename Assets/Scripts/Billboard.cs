using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        Vector3 CameraForward = GlobalsController.Instance.mainCamera.gameObject.transform.forward;
        transform.forward = new Vector3(CameraForward.x,
                                        transform.forward.y,
                                        CameraForward.z);
    }
}
