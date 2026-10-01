using UnityEngine;

public class Moneda : MonoBehaviour
{
    public int puntos = 10;
    [SerializeField]
    private float velocidadGiro = 1f;

    public Animator animatorMoneda;
    public string tagBuscado;
    public AudioSource sonido;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("Hola mundo. ESTOY EN EL START");
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Hola mundo. ESTOY EN EL UPDATE");

        // Debug.Log("Este objeto se llama: " + this.gameObject.name);
        // Debug.Log("Este objeto se encuentra en: " + this.gameObject.transform.position);
        // Debug.Log("Este objeto mide: " + this.gameObject.transform.localScale);
        // Debug.Log("Este objeto esta orientado hacia: " + this.gameObject.transform.localEulerAngles);

        // ROTACION POR SCRIPT
        //this.gameObject.transform.localEulerAngles += (velocidadGiro * Time.deltaTime * (new Vector3(0f, 1f, 0f)));
        //this.gameObject.transform.localPosition += new Vector3(0f, 0.001f, 0f);
        //this.gameObject.transform.Translate(new Vector3(0f, 0.001f, 0f));
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("UN OBJETO COLISIONO CONMIGO: " + collision.gameObject.name);

        // if (collision.gameObject.name == "BolaPeso")
        // {
        //     Debug.Log("Es el objeto que buscaba! ✅");
        // }
        // else
        // {
        //     Debug.Log("NO es el objeto que buscaba! ❌");
        // }

        if(collision.gameObject.GetComponent<ControladorBolaPeso>() != null)
        {
            Debug.Log("Es el objeto que buscaba! ✅");
            Debug.Log("Me hace una cantidad de daño: -" + collision.gameObject.GetComponent<ControladorBolaPeso>().danio);
        }
        else
            Debug.Log("NO es el objeto que buscaba! ❌");
    }

    void OnCollisionStay(Collision collision)
    {
        Debug.Log("OnCollisionStay -> UN OBJETO COLISIONO CONMIGO: " + collision.gameObject.name);
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("OnCollisionEXIT -> UN OBJETO DEJO DE COLISIONAR CONMIGO: " + collision.gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == tagBuscado)
        {
            Debug.Log("ME OBTUVO EL OBJETO " + other.gameObject.name);
            animatorMoneda.SetTrigger("obtencion");
        }
    }

    public void ReproduceSonido(AudioClip clipSonido)
    {
        sonido.clip = clipSonido;
        sonido.Play();
    }
}
