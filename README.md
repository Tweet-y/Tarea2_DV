# Plataformas en tercera persona - Evaluación 2 (UCSC 2026-2)

Proyecto de videojuego desarrollado en **Unity**, correspondiente a la segunda evaluación de la asignatura de Desarrollo de Videojuegos. Es un juego de **plataformas en tercera persona**, de temática libre: menú inicial, un nivel jugable y una pantalla de resumen al terminar.

Al cerrar el resumen, el juego vuelve al menú con carga asíncrona. El nivel incluye obstáculos que quitan vidas. No se requieren enemigos.

---

## Integración de requerimientos

### Scripting: manejo de corrutinas
- La carga de escenas corre en una corrutina (`ControladorCargaEscena`): inicia `LoadSceneAsync`, actualiza la barra y el porcentaje frame a frame y activa la escena cuando la carga termina.

### UI: HUD y botón de inicio
- **Menú (`MenuInicio`):** botón de inicio que carga el nivel.
- **HUD:** textos en pantalla para vidas o salud (`ControladorCanvas`, `ControladorBarraSalud`) y conteo de coleccionables.
- **Cierre:** pantalla de resumen con el resultado de la partida, y desde ahí el regreso al menú.

### Sonido: efectos y música de fondo
- **Música de fondo** en loop durante el nivel.
- **Efectos del juego:** recolección, daño por obstáculos, victoria y derrota.
- **Efectos de interfaz:** botones del menú y de la pantalla de resumen.

### Input: teclado, ratón y gamepad
- El jugador se controla con el **Input System** y la base de `ThirdPersonController` (Starter Assets): movimiento, cámara y salto con teclado, ratón y gamepad.

### Personaje animado
- Personaje en tercera persona a partir de `ThirdPersonController` y los prefabs en `Assets/Prefabs/Personaje`.
- Animaciones de locomoción e idle en `Assets/Animaciones/Personajes` (`ControladorRandomIdle`).

### Gestión de escenas
- Una escena de menú con botones que cargan el nivel de forma **asíncrona**, con pantalla de progreso.
- Al terminar el nivel y la pantalla de resumen, la misma carga asíncrona devuelve al menú inicial.

### Mecánicas
- **Coleccionables:** conteo en el HUD (`Moneda`).
- **Vidas:** los obstáculos restan vidas o salud (`ControladorBolaPeso`, `ControladorCuracion`).
- **Victoria y derrota:** el nivel detecta ambas condiciones y abre la pantalla de resumen antes de volver al menú.

---

## Estructura del proyecto

```
Assets/
├── Animaciones/           # Controladores y clips (personaje, moneda, barra de salud)
├── Inputs/                # Acciones de input
├── Materiales/            # Materiales del nivel y del personaje
├── Prefabs/               # Personaje, coleccionables y obstáculos
├── Scenes/                # MenuInicio y escenas de nivel
├── Scripts/               # Lógica de juego en C#
│   ├── ControladorBala.cs
│   ├── ControladorBarraSalud.cs
│   ├── ControladorBolaPeso.cs
│   ├── ControladorCanion.cs
│   ├── ControladorCanvas.cs
│   ├── ControladorCargaEscena.cs
│   ├── ControladorCuracion.cs
│   ├── ControladorRandomIdle.cs
│   ├── Moneda.cs
│   └── TestColisiones.cs
├── Starter Assets/        # ThirdPersonController e input de ejemplo
├── Synty/                 # Modelos y animaciones de apoyo
└── TextMesh Pro/          # Fuentes y recursos de UI
```

---

## Autores

| Nombre | GitHub |
| :--- | :--- |
| Vicente Alarcón | [@vicente-ai](https://github.com/vicente-ai) |
| Benjamín Bizama | [@Tweet-y](https://github.com/Tweet-y) |
| Nicolás Valdebenito | [@NicoValdebenito](https://github.com/NicoValdebenito) |
