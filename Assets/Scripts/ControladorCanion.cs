using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorCanion : MonoBehaviour
{
    public float fuerzaDisparo = 10f;
    public Transform pivoteCanion;
    public GameObject bala;
    public Transform puntoCreacionBala;
    [Range(0.1f, 5f)]
    public float frecuencia = 1f;
    public float timer = 0f;
    public AudioSource audioDisparo;

    public Vector2 vectorMovimiento;
    public float coeficienteMovimiento = 0.1f;
    public Vector2 vectorDelta;
    public Vector2 vectorGiro;
    public float sensibilidadGiro = 0.1f;

    public InputActionReference inputDisparo;
    public InputActionReference inputMovimiento;
    public InputActionReference inputGiro;

    // Start se llama el primer frame en que se habilita este script
    void Start()
    {
        
    }

    // Update se llama por cada frame que esta ejecutando el juego
    void Update()
    {
        Debug.Log("EJE Z DEL CANION: " + pivoteCanion.forward);

        // if(timer >= frecuencia)
        // {
        //     DisparaBala();
        //     timer = 0f;
        // }
        // else
        //     timer += Time.deltaTime;

        Mover();

        vectorDelta = inputGiro.action.ReadValue<Vector2>();

        vectorGiro.x = vectorDelta.x;
        vectorGiro.y = vectorDelta.y;

        vectorGiro = vectorGiro * sensibilidadGiro;

        this.transform.Rotate(Vector3.up, vectorGiro.x, Space.World);
    }

    // OnEnable se llama cada vez que se habilita este script
    void OnEnable()
    {
        inputDisparo.action.started += Disparar;
        inputMovimiento.action.started += MoverCanion;
        inputMovimiento.action.canceled += ResetVectorMovimiento;
        
    }

    private void ResetVectorMovimiento(InputAction.CallbackContext context)
    {
        vectorMovimiento = Vector2.zero;
    }

    private void MoverCanion(InputAction.CallbackContext context)
    {
        vectorMovimiento = context.ReadValue<Vector2>();
    }

    // OnDisable se llama cada vez que se deshabilita este script
    void OnDisable()
    {
        inputDisparo.action.started -= Disparar;
        inputMovimiento.action.started -= MoverCanion;
        inputMovimiento.action.canceled -= ResetVectorMovimiento;
    }

    private void Disparar(InputAction.CallbackContext context)
    {
        Debug.Log("DISPARO!!");
        DisparaBala();
    }

    public void DisparaBala()
    {
        GameObject nuevaBala = GameObject.Instantiate(bala, puntoCreacionBala.position, puntoCreacionBala.rotation);
        // Dispara una nueva bala en el eje z que apunta el canion
        nuevaBala.GetComponent<Rigidbody>().AddForce(pivoteCanion.forward * fuerzaDisparo);
        audioDisparo.pitch = Random.Range(0.65f,2f);
        audioDisparo.Play();
    }

    public void Mover()
    {
        Vector3 desplazamiento = (new Vector3(vectorMovimiento.x, 0f, vectorMovimiento.y)) * coeficienteMovimiento;
        this.transform.Translate(desplazamiento,Space.Self);
    }

    
}
