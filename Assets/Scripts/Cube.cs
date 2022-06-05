using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] Renderer renderer;

    [SerializeField] float upForce = 1;
    [SerializeField] float sideForce = 0.1f;

    void OnEnable()
    {
        float xForce = Random.Range(-sideForce, sideForce);
        float yForce = Random.Range(upForce* 0.5f , upForce );
        float zForce = Random.Range(-sideForce,sideForce);

        Vector3 force = new Vector3(xForce,yForce,zForce);
        rigidbody.velocity = force;

        Invoke(nameof(DeactiveDelay),5);
    }   

    public void SetUp(Color color)
    {
        renderer.material.color = color;
    }  

    void DeactiveDelay() => gameObject.SetActive(false);

    void OnDisable()
    {
        ObjectPool.ReturnToPool(gameObject);
        CancelInvoke();
    }
}
