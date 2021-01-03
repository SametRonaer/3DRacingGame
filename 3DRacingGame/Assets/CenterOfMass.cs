using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMass : MonoBehaviour
{

    [SerializeField] GameObject COM;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = COM.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
