using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SNS : MonoBehaviour
{

    Rigidbody rb;

    public float bias;
    public float strength;

    Vector3 startScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startScale = transform.localScale;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.y >= 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            return;
        }
        var velocity = rb.velocity.magnitude;

        if (Mathf.Approximately(velocity, 0f))
        {
            Debug.Log("Zero Vel");
            return;
        }

        var amount = velocity * (strength / 2) + bias;
        var inverseAmount = (1f / amount) * startScale.magnitude;

        transform.localScale = new Vector3(1f, amount, 1f);
    }
}
