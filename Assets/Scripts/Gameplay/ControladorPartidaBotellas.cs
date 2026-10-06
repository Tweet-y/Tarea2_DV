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
    private ControladorMenus menuPausa;
    private bool pausada, inputEstabaActivo, controladorEstabaActivo;
    private bool cursorEstabaBloqueado, miradaEstabaActiva;
    private float escalaAntesDePausa = 1f;

    public int Total => total;
    public int Recogidas => cantidadRecogidas;
    public int Puntos => puntosAcumulados;
    public int Restantes => Mathf.Max(0, total - Recogidas);
    public float Ebriedad => ebriedad;
    public float EbriedadNormalizada => Mathf.Clamp01(ebriedad / Mathf.Max(.01f, limiteEbriedad));
    public float Vida => 1f - EbriedadNormalizada;
    public Resultado Estado => resultado;
    public bool EstaPausada => pausada;

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
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Contar todos los coleccionables de este mapa, incluso en objetos
        // inactivos, sin incluir objetos de otras escenas cargadas.
        var botellasDelMapa = new List<ObjetoEspecialColeccionable>();
        foreach (var raiz in gameObject.scene.GetRootGameObjects())
            botellasDelMapa.AddRange(raiz.GetComponentsInChildren<ObjetoEspecialColeccionable>(true));
        botellas = botellasDelMapa.ToArray();
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
        if (resultado != Resultado.EnCurso) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) AlternarPausa();
        if (pausada || ebriedadPorSegundo <= 0f) return;
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

    public void AlternarPausa()
    {
        if (resultado != Resultado.EnCurso) return;
        if (pausada) { Continuar(); return; }
        pausada = true;
        escalaAntesDePausa = Time.timeScale;
        inputEstabaActivo = inputJugador != null && inputJugador.enabled;
        controladorEstabaActivo = controladorJugador != null && controladorJugador.enabled;
        var entradas = controladorJugador != null ? controladorJugador.GetComponent<StarterAssets.StarterAssetsInputs>() : null;
        if (entradas != null)
        {
            cursorEstabaBloqueado = entradas.cursorLocked;
            miradaEstabaActiva = entradas.cursorInputForLook;
            entradas.cursorLocked = false;
            entradas.cursorInputForLook = false;
            entradas.MoveInput(Vector2.zero);
            entradas.LookInput(Vector2.zero);
            entradas.JumpInput(false);
            entradas.SprintInput(false);
        }
        if (inputJugador != null) inputJugador.enabled = false;
        if (controladorJugador != null) controladorJugador.enabled = false;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        menuPausa = new GameObject("MenuPausa", typeof(RectTransform)).AddComponent<ControladorMenus>();
        menuPausa.ConstruirPausa();
    }

    public void Continuar()
    {
        if (!pausada || resultado != Resultado.EnCurso) return;
        pausada = false;
        if (menuPausa != null)
        {
            menuPausa.gameObject.SetActive(false);
            Destroy(menuPausa.gameObject);
            menuPausa = null;
        }
        var entradas = controladorJugador != null ? controladorJugador.GetComponent<StarterAssets.StarterAssetsInputs>() : null;
        if (entradas != null)
        {
            entradas.cursorLocked = cursorEstabaBloqueado;
            entradas.cursorInputForLook = miradaEstabaActiva;
        }
        if (inputJugador != null) inputJugador.enabled = inputEstabaActivo;
        if (controladorJugador != null) controladorJugador.enabled = controladorEstabaActivo;
        Time.timeScale = escalaAntesDePausa;
        AudioListener.pause = false;
        Cursor.lockState = cursorEstabaBloqueado ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorEstabaBloqueado;
    }

    private void AlRecoger(ObjetoEspecialColeccionable botella, GameObject jugador)
    {
        if (pausada || resultado != Resultado.EnCurso || botella == null || !Array.Exists(botellas, b => b == botella)) return;
        if (!recogidas.Add(botella)) return;

        cantidadRecogidas++;
        puntosAcumulados += Mathf.Max(0, botella.puntos);

        // Reservar margen para el tiempo y las trampas aunque aumente el
        // numero de botellas: recogerlas todas no debe causar derrota por si solo.
        float incremento = Mathf.Min(ebriedadPorBotella, limiteEbriedad * .8f / Mathf.Max(1, total));
        ebriedad = Mathf.Min(limiteEbriedad, ebriedad + incremento);
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
        if (pausada || resultado != Resultado.EnCurso || cantidadDanio <= 0f) return;
        ebriedad = Mathf.Min(limiteEbriedad, ebriedad + cantidadDanio);

        if (ebriedad >= limiteEbriedad)
            Finalizar(Resultado.Derrota);

        EstadoActualizado?.Invoke();
    }

    public void Morir()
    {
        if (pausada || resultado != Resultado.EnCurso) return;
        ebriedad = limiteEbriedad;
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
