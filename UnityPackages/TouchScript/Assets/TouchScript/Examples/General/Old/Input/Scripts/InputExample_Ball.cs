using UnityEngine;

public class InputExample_Ball : MonoBehaviour
{
    public float Speed = 1f;

    private void Update()
    {
        transform.localPosition += Vector3.forward*Speed*Time.deltaTime;
        if (transform.localPosition.z > 100) Destroy(gameObject);
    }
}