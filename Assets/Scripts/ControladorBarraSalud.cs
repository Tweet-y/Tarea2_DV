using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ControladorBarraSalud : MonoBehaviour
{
    // ATRIBUTOS
    public Animator animatorBarraSalud;
    public string nombreParametro = "salud";
    [Range(0f,1f)]
    public float porcentajeSalud = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(animatorBarraSalud == null)
            animatorBarraSalud = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animatorBarraSalud.SetFloat(nombreParametro, porcentajeSalud);
    }

    void Reset()
    {
        if(animatorBarraSalud == null)
            animatorBarraSalud = this.GetComponent<Animator>();
    }
}
