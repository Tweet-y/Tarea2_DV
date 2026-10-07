using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>Comprueba la integración en una partida real, incluyendo aterrizaje y victoria.</summary>
[InitializeOnLoad]
public static class ValidarCiudadFinal
{
    private const string Clave = "CiudadFinal.Validando";
    private static int paso;
    private static double desde;
    private static float vidaAntes;
    private static string informe = "";
    static ValidarCiudadFinal()
    {
        if (SessionState.GetBool(Clave, false)) Registrar();
    }
    [MenuItem("Tools/Gameplay/Verificar mapa curacion caida y victoria")]
    public static void Ejecutar()
    {
        Directory.CreateDirectory("Logs");
        SessionState.SetBool(Clave, true);
        EditorSceneManager.OpenScene("Assets/Scenes/MenuInicio.unity");
        Registrar();
        EditorApplication.isPlaying = true;
    }
    private static void Registrar()
    {
        desde = EditorApplication.timeSinceStartup;
        EditorApplication.update -= Avanzar;
        EditorApplication.update += Avanzar;
    }
    private static void Exigir(bool valor, string mensaje)
    {
        if (!valor) throw new Exception(mensaje);
        informe += "OK: " + mensaje + "\n";
        File.WriteAllText("Logs/ciudad-final-validada.txt", informe);
    }
    private static void Avanzar()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - desde > 90) throw new Exception("Tiempo agotado en paso " + paso);
            if (!EditorApplication.isPlaying) return;
            Application.runInBackground = true;
            var partida = ControladorPartidaBotellas.Instancia;
            if (paso == 0 && SceneManager.GetActiveScene().name == "MenuInicio")
            {
                var menu = UnityEngine.Object.FindFirstObjectByType<ControladorMenus>();
                if (menu == null) return;
                menu.Cargar("MapaV1"); paso = 1;
            }
            else if (paso == 1 && partida != null && partida.Total > 0)
            {
                var jugador = GameObject.FindGameObjectWithTag("Player");
                var botellas = UnityEngine.Object.FindObjectsByType<ObjetoEspecialColeccionable>(FindObjectsSortMode.None);
                var azules = botellas.Where(b => b.tipo == ObjetoEspecialColeccionable.TipoBotella.Curativa).ToArray();
                Exigir(azules.Length == 5, "Existen cinco botellas curativas azules.");
                Exigir(partida.Total == botellas.Length - azules.Length, "Las azules no forman parte del objetivo.");
                var mapa = UnityEngine.Object.FindFirstObjectByType<MapaCiudadGraphic>();
                Exigir(mapa != null, "El mapa de la ciudad está integrado en el HUD.");
                Canvas.ForceUpdateCanvases();
                Exigir(mapa.canvasRenderer.GetMesh() != null && mapa.canvasRenderer.GetMesh().vertexCount > 0, "El mapa genera geometría visible.");
                partida.RecibirDanio(.15f);
                float vida = partida.Vida;
                azules[0].Recoger(jugador);
                Exigir(Mathf.Abs(partida.Vida - vida - .08f) < .001f && partida.Recogidas == 0 && partida.Puntos == 0,
                    "La botella azul cura sólo 8% sin dar puntos ni avanzar el objetivo.");
                azules[0].Recoger(jugador);
                Exigir(Mathf.Abs(partida.Vida - vida - .08f) < .001f, "La curación no se puede repetir.");
                var caida = jugador.GetComponent<DanioCaida>();
                Exigir(caida != null && caida.CalcularDanio(3f) == 0 && Mathf.Abs(caida.CalcularDanio(10f) - .27f) < .001f,
                    "Saltos bajos seguros; caída de 10m inflige 27%.");
                // Plataforma aislada para comprobar gravedad y contacto reales.
                var piso = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piso.name = "PlataformaValidacionCaida";
                piso.transform.position = new Vector3(0, 1000, 0);
                piso.transform.localScale = new Vector3(30, 1, 30);
                var movimiento = jugador.GetComponent<StarterAssets.ThirdPersonController>();
                piso.layer = Enumerable.Range(0, 32).First(l => (movimiento.GroundLayers.value & (1 << l)) != 0);
                var cc = jugador.GetComponent<CharacterController>();
                cc.enabled = false;
                jugador.transform.position = new Vector3(0, 1012, 0);
                cc.enabled = true;
                vidaAntes = partida.Vida;
                paso = 2;
            }
            else if (paso == 2)
            {
                var jugador = GameObject.FindGameObjectWithTag("Player");
                if (jugador.transform.position.y > 1001 || !jugador.GetComponent<StarterAssets.ThirdPersonController>().Grounded) return;
                Exigir(vidaAntes - partida.Vida > .25f, "Aterrizar tras una caída real aplica daño.");
                jugador.GetComponent<StarterAssets.ThirdPersonController>().enabled = false;
                // Recuperar margen para completar el objetivo sin intoxicación acumulada de la prueba.
                foreach (var b in UnityEngine.Object.FindObjectsByType<ObjetoEspecialColeccionable>(FindObjectsSortMode.None))
                    if (b.tipo == ObjetoEspecialColeccionable.TipoBotella.Curativa) b.Recoger(jugador);
                foreach (var b in partida.Botellas.ToArray()) if (b != null) b.Recoger(jugador);
                Exigir(partida.Estado == ControladorPartidaBotellas.Resultado.Victoria, "Todas las botellas objetivo activan la victoria.");
                var textos = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
                Exigir(textos.Any(t => t.text == "mission passed!") && textos.Any(t => t.text == "RESPECT + 99"),
                    "Victoria muestra mission passed! y RESPECT + 99.");
                Exigir(!UnityEngine.Object.FindFirstObjectByType<ControladorGameplayHUD>().transform.Find("MapaCiudad").gameObject.activeSelf,
                    "El mapa se oculta al finalizar.");
                Terminar(0);
            }
        }
        catch (Exception e)
        {
            File.WriteAllText("Logs/ciudad-final-validada.txt", informe + "ERROR: " + e + "\n");
            Debug.LogException(e);
            Terminar(1);
        }
    }
    private static void Terminar(int codigo)
    {
        SessionState.SetBool(Clave, false);
        EditorApplication.update -= Avanzar;
        EditorApplication.isPlaying = false;
        if (Application.isBatchMode) EditorApplication.Exit(codigo);
    }
}
