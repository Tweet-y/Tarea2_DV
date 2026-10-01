using System.Collections;
using UnityEngine;

public class ControladorRandomIdle : MonoBehaviour
{
    public Animator animatorPersonaje;
    public int maxRandom = 4;
    public float timer = 0f;
    public bool timerFuncionando = false;
    public float tiempoEsperaRandom = 5f;
    public float velocidadPlayer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Tomamos el parametro de la velocidad del personaje
        velocidadPlayer = animatorPersonaje.GetFloat("Speed");

        // Si esta detenido y el timer no funciona, lo iniciamos
        if(velocidadPlayer == 0f && timerFuncionando == false)
        {
            EmpezarTimer();
        }
        // Si el player se esta moviendo
        else if(velocidadPlayer > 0f)
        {
            timerFuncionando = false;
            timer = 0f;
            animatorPersonaje.SetFloat("tiempoIdle", 0f);
            animatorPersonaje.SetTrigger("interrumpeIdle");
            Debug.Log("INTERRUMPO EL IDLE RANDOM");
        }        
        // Si el timer esta corriendo y todavia queda tiempo antes del limite,
        // asignamos el valor al timer del animator
        else if(timerFuncionando == true && timer < tiempoEsperaRandom)
        {
            timer += Time.deltaTime;
            animatorPersonaje.SetFloat("tiempoIdle", timer);
        }
    }

    public void EmpezarTimer()
    {
        timerFuncionando = true;

        Debug.Log("EMPECE EL TIMER");

        StartCoroutine(_TimerParaRandom(tiempoEsperaRandom));
    }

    public void RandomizarIdle()
    {
        int nuevoRandom = Random.Range(0,maxRandom + 1);
        
        animatorPersonaje.SetInteger("randomIdle", nuevoRandom);

    }

    IEnumerator _TimerParaRandom(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        animatorPersonaje.SetBool("estaIdle", true);
        RandomizarIdle();

        // Espera a que termine el clip random anterior
        yield return new WaitForSeconds(1f);
        // Reestablecer los parametros
        animatorPersonaje.SetBool("estaIdle", false);
        animatorPersonaje.SetFloat("tiempoIdle", 0f);
        timer = 0f;
        timerFuncionando = false;
    }
}
