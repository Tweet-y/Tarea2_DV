using UnityEngine;

public class ControladorBolaPeso : MonoBehaviour
{
    [SerializeField]
    public float danio = 10f;
    public float salud = 90f;
    public float saludMax = 145f;

    public ControladorBarraSalud barraSalud;
    public ControladorBarraSalud iconoSalud;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(salud == 100f)
        {
            Debug.Log("ESTOY A TOPE DE SALUD");
        }

        barraSalud.porcentajeSalud = salud / saludMax;
        iconoSalud.porcentajeSalud = salud / saludMax;
    }
}
