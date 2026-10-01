using TMPro;
using UnityEngine;

public class ControladorCanvas : MonoBehaviour
{
    public TMP_Text textoSalud;
    public ControladorBolaPeso controladorBola;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textoSalud.text = controladorBola.salud.ToString();
    }
}
