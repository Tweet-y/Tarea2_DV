using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorCargaEscena : MonoBehaviour
{
    [Header("Configuracion de UI")]
    public GameObject pantallaCarga;
    public Slider barraProgreso;
    public TMP_Text textoPorcentaje;
    
    // COMPONENTE TEXTO INCORRECTO PARA TEXT MESH PRO
    // public Text texto;

    [Header("Ajustes de transición")]
    public float velocidadLlenado = 1.5f;



    public void CargaEscena_Old(int indice)
    {
        SceneManager.LoadScene(indice);
    }

    public void CargaEscena(int indice)
    {
        StartCoroutine(_CargaEscena(indice));
    }

    IEnumerator _CargaEscena(int indice)
    {
        // Si tenemos asignada la pantalla de carga, la activamos
        if(pantallaCarga != null)
            pantallaCarga.SetActive(true);
        
        // Comenzamos la operacion de carga de la escena indicada
        AsyncOperation operacionCarga = SceneManager.LoadSceneAsync(indice);

        // Bloquea la activacion automática de la escena
        // para controlar su despliegue final de forma manual
        operacionCarga.allowSceneActivation = false;

        // Cuanto le paso al slider para mostrar el porcentaje
        float progresoVisible = 0f;

        // Mientras no se termine la carga de la nueva escena...
        while(operacionCarga.isDone == false)
        {
            // ... seguir cargando la barra y porcentaje
            
            // Normalizo de 0 a 1 el progreso real de la operacion
            float progresoObjetivo = Mathf.Clamp01(operacionCarga.progress / 0.9f);
            
            // Incrementa el valor visual de forma suava, frame a frame
            progresoVisible = Mathf.MoveTowards(progresoVisible, progresoObjetivo,
                                        velocidadLlenado * Time.deltaTime);
            
            // Si asignamos el slider de progreso, le damos el valor al slider
            if(barraProgreso != null)
                barraProgreso.value = progresoVisible;
            
            // Si asignamos el text TMP, le damos el valor al porcentaje
            if(textoPorcentaje != null)
                textoPorcentaje.text = (progresoVisible * 100f).ToString("F0") + "%";
            
            // Si termino de cargar (verificando en ambos progresos)... 
            if(progresoVisible >= 1f && operacionCarga.progress >= 0.9f)
            {
                // ... cargo la escena en pantalla
                operacionCarga.allowSceneActivation = true;
            }

            yield return null;
        }

    }
}
