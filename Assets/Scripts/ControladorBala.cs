using UnityEngine;

public class ControladorBala : MonoBehaviour
{
    public float tiempoVida = 1f;
    public float timer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= tiempoVida)
            GameObject.Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject.Destroy(this.gameObject);
    }
}
