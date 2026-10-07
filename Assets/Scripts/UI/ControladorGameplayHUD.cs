using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>HUD compacto de cerveza, ebriedad y resistencia al desmayo.</summary>
public sealed class ControladorGameplayHUD : MonoBehaviour
{
    private ControladorPartidaBotellas partida;
    private RectTransform ebriedadFill, vidaFill, icono;
    private TMP_Text contador, restantes, aviso, feedback, puntos;
    private float objetivoEbriedad, objetivoVida = 1f, visualEbriedad, visualVida = 1f;
    private float tiempoPickup;
    private RectTransform objetivoInicial;
    private float tiempoObjetivo = 8f;
    private readonly List<Material> materialesTexto = new List<Material>();

    public void Inicializar(ControladorPartidaBotellas fuente)
    {
        partida = fuente;
        Construir();
        ConectarEventos();
        Actualizar();
    }

    private void OnEnable()
    {
        if (partida == null) return;
        ConectarEventos();
        Actualizar();
    }

    private void ConectarEventos()
    {
        DesconectarEventos();
        partida.EstadoActualizado += Actualizar;
        partida.BotellaRecogida += Recogida;
        partida.PartidaTerminada += MostrarResultado;
    }

    private void OnDisable() => DesconectarEventos();

    private void DesconectarEventos()
    {
        if (partida == null) return;
        partida.EstadoActualizado -= Actualizar;
        partida.BotellaRecogida -= Recogida;
        partida.PartidaTerminada -= MostrarResultado;
    }

    private void OnDestroy()
    {
        if (partida != null)
        {
            partida.EstadoActualizado -= Actualizar;
            partida.BotellaRecogida -= Recogida;
            partida.PartidaTerminada -= MostrarResultado;
        }
        foreach (var material in materialesTexto) Destroy(material);
    }

    private void Construir()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = .5f;

        var root = Nodo(transform, "GameplayHUD", new Vector2(1, 1), new Vector2(-30, -26), new Vector2(280, 188));
        root.pivot = new Vector2(1, 1);
        icono = Nodo(root, "BottleIcon", new Vector2(0, 1), Vector2.zero, new Vector2(88, 90));
        Caja(icono, "BlackFrame", Vector2.zero, new Vector2(88, 90), Color.black);
        Caja(icono, "WhiteInset", new Vector2(4, -4), new Vector2(80, 82), Color.white);
        var botella = Nodo(icono, "BeerSilhouette", new Vector2(0, 1), new Vector2(13, -8), new Vector2(62, 74));
        botella.gameObject.AddComponent<BottleHudGraphic>().raycastTarget = false;

        contador = Texto(root, "BottleCounter", $"00 / {partida.Total:00}", new Vector2(99, 0), new Vector2(181, 36), 34, Color.white);
        Texto(root, "DrunkLabel", "EBRIEDAD", new Vector2(101, -40), new Vector2(179, 20), 17, Color.white);
        ebriedadFill = Barra(root, "DrunkMeter", new Vector2(99, -63), new Vector2(181, 17), new Color32(205, 205, 205, 255));
        Texto(root, "HealthLabel", "VIDA", new Vector2(0, -98), new Vector2(280, 20), 17, Color.white);
        vidaFill = Barra(root, "PlayerHealth", new Vector2(0, -120), new Vector2(280, 20), new Color32(188, 37, 45, 255));
        restantes = Texto(root, "RemainingText", $"QUEDAN {partida.Total}", new Vector2(0, -146), new Vector2(280, 31), 29, new Color32(82, 142, 72, 255));
        aviso = Texto(root, "DangerStatus", "SOBRIO", new Vector2(0, -180), new Vector2(280, 22), 17, Color.white);
        puntos = Texto(root, "Score", "PUNTOS 0000", new Vector2(0, -207), new Vector2(280, 22), 17, Color.white);
        feedback = Texto(root, "BottlePickupFeedback", "+1", new Vector2(48, -66), new Vector2(35, 26), 24, Color.white);
        feedback.gameObject.SetActive(false);

        objetivoInicial = Nodo(transform, "ObjetivoInicial", new Vector2(.5f, 0f),
            new Vector2(0, 42), new Vector2(900, 106));
        objetivoInicial.pivot = new Vector2(.5f, 0f);
        var fondoObjetivo = objetivoInicial.gameObject.AddComponent<Image>();
        fondoObjetivo.color = new Color32(17, 21, 20, 225);
        fondoObjetivo.raycastTarget = false;
        var objetivo = Texto(objetivoInicial, "TextoObjetivo",
            "OBJETIVO\nRecoge todas las botellas para quedar lo más borracho que puedas.",
            new Vector2(24, -12), new Vector2(852, 82), 26, Color.white);
        objetivo.alignment = TextAlignmentOptions.Center;
        objetivo.textWrappingMode = TextWrappingModes.Normal;

    }

    private void Actualizar()
    {
        objetivoEbriedad = partida.EbriedadNormalizada;
        objetivoVida = partida.Vida;
        contador.text = $"{partida.Recogidas:00} / {partida.Total:00}";
        restantes.text = $"QUEDAN {partida.Restantes}";
        if (puntos != null) puntos.text = $"PUNTOS {partida.Puntos:0000}";
        aviso.text = objetivoEbriedad >= .85f ? "! PELIGRO DE DESMAYO !" :
            objetivoEbriedad >= .6f ? "MUY EBRIO" : objetivoEbriedad >= .3f ? "MAREADO" : "SOBRIO";
    }

    private void Update()
    {
        if (partida == null) return;
        if (objetivoInicial != null && objetivoInicial.gameObject.activeSelf && !partida.EstaPausada)
        {
            tiempoObjetivo = Mathf.Max(0f, tiempoObjetivo - Time.deltaTime);
            if (tiempoObjetivo <= 0f) objetivoInicial.gameObject.SetActive(false);
        }
        visualEbriedad = Mathf.MoveTowards(visualEbriedad, objetivoEbriedad, Time.unscaledDeltaTime * 2f);
        visualVida = Mathf.MoveTowards(visualVida, objetivoVida, Time.unscaledDeltaTime * 2f);
        ebriedadFill.localScale = new Vector3(visualEbriedad, 1, 1);
        vidaFill.localScale = new Vector3(visualVida, 1, 1);
        aviso.color = objetivoEbriedad >= .85f
            ? Color.Lerp(Color.white, new Color32(255, 90, 75, 255), .5f + .5f * Mathf.Sin(Time.unscaledTime * 7))
            : Color.white;
        if (tiempoPickup > 0)
        {
            tiempoPickup = Mathf.Max(0, tiempoPickup - Time.unscaledDeltaTime);
            icono.localScale = Vector3.one * (1 + .07f * Mathf.Sin(tiempoPickup / .45f * Mathf.PI));
            feedback.gameObject.SetActive(tiempoPickup > 0);
        }
        else icono.localScale = Vector3.one;
    }

    private void Recogida() => tiempoPickup = .45f;

    private void MostrarResultado(ControladorPartidaBotellas.Resultado estado)
    {
        Actualizar();
        transform.Find("GameplayHUD").gameObject.SetActive(false);
        if (objetivoInicial != null) objetivoInicial.gameObject.SetActive(false);
        bool victoria = estado == ControladorPartidaBotellas.Resultado.Victoria;
        var menu = new GameObject("MenuResultado", typeof(RectTransform)).AddComponent<ControladorMenus>();
        menu.ConstruirResultado(victoria, $"{partida.Recogidas} / {partida.Total} BOTELLAS RECOGIDAS");
    }

    private RectTransform Barra(Transform parent, string nombre, Vector2 pos, Vector2 size, Color color)
    {
        var frame = Caja(parent, nombre, pos, size, Color.black);
        var inset = Caja(frame, "Empty", new Vector2(3, -3), size - new Vector2(6, 6), new Color32(46, 46, 46, 255));
        var fill = Caja(inset, "Fill", Vector2.zero, size - new Vector2(6, 6), color);
        return fill;
    }

    private static RectTransform Nodo(Transform parent, string nombre, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var obj = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer));
        obj.transform.SetParent(parent, false);
        var rect = (RectTransform)obj.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return rect;
    }

    private static RectTransform Caja(Transform parent, string nombre, Vector2 pos, Vector2 size, Color color)
    {
        var rect = Nodo(parent, nombre, new Vector2(0, 1), pos, size);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private TMP_Text Texto(Transform parent, string nombre, string valor, Vector2 pos, Vector2 size, float fontSize, Color color)
    {
        var rect = Nodo(parent, nombre, new Vector2(0, 1), pos, size);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        var material = new Material(text.fontSharedMaterial);
        materialesTexto.Add(material);
        text.fontMaterial = material;
        text.text = valor;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = -1;
        text.color = color;
        text.outlineColor = Color.black;
        text.outlineWidth = .32f;
        text.alignment = TextAlignmentOptions.Right;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }
}

/// <summary>Icono vectorial original; no depende de caracteres o imágenes externas.</summary>
public sealed class BottleHudGraphic : Graphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var shape = new[] {
            new Vector2(.39f,.94f), new Vector2(.61f,.94f), new Vector2(.61f,.65f),
            new Vector2(.76f,.53f), new Vector2(.76f,.06f), new Vector2(.24f,.06f),
            new Vector2(.24f,.53f), new Vector2(.39f,.65f)
        };
        Polygon(vh, shape, Color.black);
        var inner = new Vector2[shape.Length];
        for (int i = 0; i < shape.Length; i++) inner[i] = new Vector2(.5f,.5f) + (shape[i] - new Vector2(.5f,.5f)) * .83f;
        Polygon(vh, inner, new Color32(55, 111, 53, 255));
        Polygon(vh, new[] {new Vector2(.35f,.23f), new Vector2(.65f,.23f), new Vector2(.65f,.45f), new Vector2(.35f,.45f)}, new Color32(249, 225, 153, 255));
        Polygon(vh, new[] {new Vector2(.36f,.91f), new Vector2(.64f,.91f), new Vector2(.64f,.99f), new Vector2(.36f,.99f)}, Color.black);
        Polygon(vh, new[] {new Vector2(.40f,.94f), new Vector2(.60f,.94f), new Vector2(.60f,.97f), new Vector2(.40f,.97f)}, new Color32(211, 164, 62, 255));
    }

    private void Polygon(VertexHelper vh, Vector2[] points, Color color)
    {
        int first = vh.currentVertCount;
        var rect = rectTransform.rect;
        // Concave bottle shoulder is triangulated around an interior point.
        var center = UIVertex.simpleVert;
        Vector2 average = Vector2.zero;
        foreach (var point in points) average += point;
        average /= points.Length;
        center.position = new Vector2(rect.xMin + rect.width * average.x, rect.yMin + rect.height * average.y);
        center.color = color;
        vh.AddVert(center);
        foreach (var point in points)
        {
            var vertex = UIVertex.simpleVert;
            vertex.position = new Vector2(rect.xMin + point.x * rect.width, rect.yMin + point.y * rect.height);
            vertex.color = color;
            vh.AddVert(vertex);
        }
        for (int i = 0; i < points.Length; i++)
            vh.AddTriangle(first, first + i + 1, first + (i + 1) % points.Length + 1);
    }
}
