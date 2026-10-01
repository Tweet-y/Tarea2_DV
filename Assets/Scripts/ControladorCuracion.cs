using System;
using Unity.VisualScripting;
using UnityEngine;

public class ControladorCuracion : MonoBehaviour
{
    public float cantidadCuracion = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("ALGO ENTRO EN MI ESPACIO DE CURA");
        Debug.Log("Se llama: " + other.gameObject.name);

        // if(other.gameObject.GetComponent<ControladorBolaPeso>() != null)
        // {
        //     other.gameObject.GetComponent<ControladorBolaPeso>().salud += this.cantidadCuracion;
        // }
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("ALGO ESTA ADENTRO DE MI ESPACIO DE CURA");
        other.gameObject.GetComponent<ControladorBolaPeso>().salud++;
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("ALGO SALIO DE MI ESPACIO DE CURA"); 
    }
}
