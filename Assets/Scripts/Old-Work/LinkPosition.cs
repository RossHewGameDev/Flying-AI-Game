using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkPosition : MonoBehaviour
{

    [SerializeField] Transform parent;
    [SerializeField] Rigidbody rb;
    private Vector3 whereToBe;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        


    }
    // Update is called once per frame
    void FixedUpdate()
    {
        rb.MovePosition(parent.position);



        /*
        whereToBe.x = Mathf.Lerp(transform.position.x, parent.position.x, 10f);
        whereToBe.y = Mathf.Lerp(transform.position.y, parent.position.y, 10f);
        whereToBe.z = Mathf.Lerp(transform.position.z, parent.position.z, 10f);

        transform.position = whereToBe;
        */
    }
}
