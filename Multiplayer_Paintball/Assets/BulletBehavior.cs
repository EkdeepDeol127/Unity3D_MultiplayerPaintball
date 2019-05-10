using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour {

    private float timer = 10f;
    private Rigidbody m_rb = null;
    public Color m_HoldColor;

    void Start ()
    {
        m_rb = GetComponent<Rigidbody>();
	}
	
	void Update ()
    {
        m_rb.AddForce(Vector3.forward * 50);
        if(timer <= 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(this.gameObject);
    }
}
