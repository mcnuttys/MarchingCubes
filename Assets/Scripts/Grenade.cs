using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float explosionRange = 15f;
    public float explosiveForce = 500f;
    public float weight = 0;
    public float speed = 5f;
    public float maxAge = 5f;
    public LayerMask explosionLayers;

    private float timer;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > maxAge)
            Detonate();
    }

    void Detonate()
    {
        // Instantiate effect and modify the terrain
        Collider[] explodedObjects = Physics.OverlapSphere(transform.position, explosionRange, explosionLayers);
        for (int i = 0; i < explodedObjects.Length; i++)
        {
            if(explodedObjects[i].GetComponent<Rigidbody>())
            {
                explodedObjects[i].GetComponent<Rigidbody>().AddExplosionForce(explosiveForce, transform.position, explosionRange);
            }
        }

        //GameObject.FindObjectOfType<WorldGenerator>().ModifyTerrain(transform.position, explosionRange, weight);
        GameObject.Destroy(gameObject);
    }
}
