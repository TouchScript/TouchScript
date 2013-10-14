using UnityEngine;

public class CombinedExample_BigCube : MonoBehaviour
{
    private Vector3 startScale;

    private void Start()
    {
        startScale = transform.localScale;
    }

    private void Update()
    {
        if (transform.localScale.sqrMagnitude > startScale.sqrMagnitude)
        {
            transform.localScale = startScale;
        }
    }
}