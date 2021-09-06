using UnityEngine;

public class TerrainAlign : MonoBehaviour
{

    public float groundDistance = 5f;
    public Vector3 offset;

    RaycastHit hit;
    Vector3 ray;

    void Update()
    {
        Align();
    }

    private void Align()
    {
        ray = -transform.up;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, ray, out hit, groundDistance, CONSTANTS.GROUND_MASK))
        {
            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotation * transform.parent.rotation, 10 * Time.deltaTime);
        }
    }
}
