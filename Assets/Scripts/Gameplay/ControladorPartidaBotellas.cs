using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>Fuente de verdad del objetivo, la ebriedad y el resultado de la partida.</summary>
public sealed class ControladorPartidaBotellas : MonoBehaviour
{
    public enum Resultado { EnCurso, Victoria, Derrota }

    public static ControladorPartidaBotellas Instancia { get; private set; }
    public event Action EstadoActualizado;
    public event Action BotellaRecogida;
    public event Action<Resultado> PartidaTerminada;

    [Header("Balance")]
    [SerializeField, Min(0.01f)] private float ebriedadPorBotella = 0.04f;
    [SerializeField, Min(0f)] private float ebriedadPorSegundo = 0.001f;
    [SerializeField, Range(0f, 1f)] private float limiteEbriedad = 1f;

    private readonly HashSet<ObjetoEspecialColeccionable> recogidas = new HashSet<ObjetoEspecialColeccionable>();
    private ObjetoEspecialColeccionable[] botellas;
    private int total;
    private int cantidadRecogidas;
    private int puntosAcumulados;
    private float ebriedad;
    private Resultado resultado = Resultado.EnCurso;
    private PlayerInput inputJugador;
    private StarterAssets.ThirdPersonController controladorJugador;
    private float tiempoActualizacionHud;

    public int Total => total;
    public int Recogidas => cantidadRecogidas;
    public int Puntos => puntosAcumulados;
    public int Restantes => Mathf.Max(0, total - Recogidas);
    public float Ebriedad => ebriedad;
    public float EbriedadNormalizada => Mathf.Clamp01(ebriedad / Mathf.Max(.01f, limiteEbriedad));
    public float Vida => 1f - EbriedadNormalizada;
    public Resultado Estado => resultado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegistrarCargaEscena()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        Instancia = null;
    }

    private static void AlCargarEscena(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MapaV1" || Instancia != null) return;
        new GameObject("PartidaBotellas").AddComponent<ControladorPartidaBotellas>();
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        botellas = FindObjectsByType<ObjetoEspecialColeccionable>(FindObjectsSortMode.None);
        total = botellas.Length;

        var jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            inputJugador = jugador.GetComponent<PlayerInput>();
            controladorJugador = jugador.GetComponent<StarterAssets.ThirdPersonController>();
        }

        var hud = new GameObject("CanvasGameplay", typeof(RectTransform)).AddComponent<ControladorGameplayHUD>();
        hud.Inicializar(this);
        EstadoActualizado?.Invoke();
    }

    private void OnEnable()
    {
        if (Instancia != null && Instancia != this) return;
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        Instancia = this;
        ObjetoEspecialColeccionable.AlRecogerColeccionable -= AlRecoger;
        ObjetoEspecialColeccionable.AlRecogerColeccionable += AlRecoger;
    }

    private void OnDisable()
    {
        ObjetoEspecialColeccionable.AlRecogerColeccionable -= AlRecoger;
        if (Instancia == this) Instancia = null;
    }

    private void Update()
    {
        if (resultado != Resultado.EnCurso || ebriedadPorSegundo <= 0f) return;
        var anterior = ebriedad;
        ebriedad = Mathf.Min(limiteEbriedad, ebriedad + ebriedadPorSegundo * Time.deltaTime);
        if (Mathf.Approximately(anterior, ebriedad)) return;
        if (ebriedad >= limiteEbriedad)
            Finalizar(Resultado.Derrota);
        tiempoActualizacionHud += Time.deltaTime;
        if (tiempoActualizacionHud >= .1f)
        {
            tiempoActualizacionHud = 0f;
            EstadoActualizado?.Invoke();
        }
    }

    private void AlRecoger(ObjetoEspecialColeccionable botella, GameObject jugador)
    {
        if (resultado != Resultado.EnCurso || botella == null || !Array.Exists(botellas, b => b == botella)) return;
        if (!recogidas.Add(botella)) return;

        cantidadRecogidas++;
        puntosAcumulados += Mathf.Max(0, botella.puntos);

        ebriedad = Mathf.Min(limiteEbriedad, ebriedad + ebriedadPorBotella);
        // Debe completar el objetivo antes de alcanzar el límite de ebriedad.
        if (ebriedad >= limiteEbriedad)
            Finalizar(Resultado.Derrota);
        else if (Recogidas >= total)
            Finalizar(Resultado.Victoria);

        EstadoActualizado?.Invoke();
        BotellaRecogida?.Invoke();
    }

    /// <summary>Aplica daño directo al jugador (aumenta la intoxicación / reduce la vida).</summary>
    public void RecibirDanio(float cantidadDanio)
    {
        if (resultado != Resultado.EnCurso || cantidadDanio <= 0f) return;
        ebriedad = Mathf.Min(limiteEbriedad, ebriedad + cantidadDanio);

        if (ebriedad >= limiteEbriedad)
            Finalizar(Resultado.Derrota);

        EstadoActualizado?.Invoke();
    }

    private void Finalizar(Resultado nuevoResultado)
    {
        if (resultado != Resultado.EnCurso) return;
        resultado = nuevoResultado;
        if (inputJugador != null) inputJugador.enabled = false;
        if (controladorJugador != null) controladorJugador.enabled = false;
        if (controladorJugador != null)
        {
            var entradas = controladorJugador.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (entradas != null)
            {
                entradas.cursorLocked = false;
                entradas.cursorInputForLook = false;
                entradas.MoveInput(Vector2.zero);
                entradas.LookInput(Vector2.zero);
                entradas.JumpInput(false);
                entradas.SprintInput(false);
            }
        }
        Time.timeScale = 0f;
        PartidaTerminada?.Invoke(resultado);
    }
}
