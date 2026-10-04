using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using StarterAssets;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public static class ConfigurarPersonajeCJ
{
    const string FbxPath = "Assets/Prefabs/Personaje/cj/CJ.fbx";
    const string PrefabPath = "Assets/Prefabs/Personaje/cj/CJ.prefab";
    const string SourcePlayer = "Assets/Prefabs/Personaje 2.0/PlayerArmature.prefab";
    const string ControllerPath = "Assets/Animaciones/Personajes/PlayerLocomotion.controller";
    const string ScenePath = "Assets/Scenes/MapaV1.unity";
    const string LogPath = "Logs/cj-setup.txt";
    const float AlturaObjetivo = 1.75f;

    static readonly (string Humano, string[] Claves)[] MapaHuesos =
    {
        ("Hips", new[] { "pelvis", "hips", "hip" }),
        ("Spine", new[] { "spine", "spine1", "abdomen" }),
        ("Chest", new[] { "spine1", "spine2", "chest" }),
        ("UpperChest", new[] { "spine2", "spine3", "upperchest" }),
        ("Neck", new[] { "neck" }),
        ("Head", new[] { "head" }),
        ("LeftShoulder", new[] { "lclavicle", "leftclavicle", "lshoulder", "leftshoulder" }),
        ("RightShoulder", new[] { "rclavicle", "rightclavicle", "rshoulder", "rightshoulder" }),
        ("LeftUpperArm", new[] { "lupperarm", "leftupperarm", "larm", "leftarm" }),
        ("RightUpperArm", new[] { "rupperarm", "rightupperarm", "rarm", "rightarm" }),
        ("LeftLowerArm", new[] { "lforearm", "leftforearm", "llowerarm", "leftlowerarm" }),
        ("RightLowerArm", new[] { "rforearm", "rightforearm", "rlowerarm", "rightlowerarm" }),
        ("LeftHand", new[] { "lhand", "lefthand" }),
        ("RightHand", new[] { "rhand", "righthand" }),
        ("LeftUpperLeg", new[] { "lthigh", "leftthigh", "lupperleg", "leftupperleg", "lupleg" }),
        ("RightUpperLeg", new[] { "rthigh", "rightthigh", "rupperleg", "rightupperleg", "rupleg" }),
        ("LeftLowerLeg", new[] { "lcalf", "leftcalf", "llowerleg", "leftlowerleg", "lleg" }),
        ("RightLowerLeg", new[] { "rcalf", "rightcalf", "rlowerleg", "rightlowerleg", "rleg" }),
        ("LeftFoot", new[] { "lfoot", "leftfoot" }),
        ("RightFoot", new[] { "rfoot", "rightfoot" }),
        ("LeftToes", new[] { "ltoe", "lefttoe", "ltoes", "lefttoes" }),
        ("RightToes", new[] { "rtoe", "righttoe", "rtoes", "righttoes" }),
    };

    public static void Ejecutar()
    {
        Directory.CreateDirectory("Logs");
        var log = new StringBuilder();
        try
        {
            if (!File.Exists(FbxPath))
            {
                Fallar(log, "No esta el FBX en " + FbxPath);
                return;
            }

            ConfigurarImportador();
            AjustarEscala(log);
            AsignarTexturas(log);
            var armado = ArmarAvatar(log);
            if (armado.avatar == null || !armado.avatar.isValid || !armado.avatar.isHuman)
            {
                Fallar(log, "El avatar de CJ sigue invalido.");
                return;
            }

            var prefab = CrearPrefab(armado, log);
            ReemplazarEnEscena(prefab, log);
            AssetDatabase.SaveAssets();
            File.WriteAllText(LogPath, log.ToString());
            Debug.Log(log.ToString());
            EditorApplication.Exit(0);
        }
        catch (System.Exception ex)
        {
            log.AppendLine(ex.ToString());
            File.WriteAllText(LogPath, log.ToString());
            Debug.LogError(ex);
            EditorApplication.Exit(1);
        }
    }

    static void Fallar(StringBuilder log, string mensaje)
    {
        log.AppendLine(mensaje);
        File.WriteAllText(LogPath, log.ToString());
        Debug.LogError(mensaje);
        EditorApplication.Exit(1);
    }

    static void ConfigurarImportador()
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
        importer.importAnimation = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.SaveAndReimport();
    }

    struct AvatarArmado
    {
        public Avatar avatar;
        public GameObject instancia;
    }

    static AvatarArmado ArmarAvatar(StringBuilder log)
    {
        var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        var instancia = Object.Instantiate(modelo);
        instancia.name = "CJModel";
        instancia.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        instancia.transform.localScale = Vector3.one;

        Transform Buscar(string nombre)
        {
            return instancia.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == nombre);
        }

        var torso = Buscar("TORSO");
        var cabeza = Buscar("CABEZA_");
        var brazoIzq = Buscar("BRAZO_IZQ");
        var brazoDer = Buscar("BRAZO_DER");
        var piernaIzq = Buscar("PIERNA_IZQ");
        var piernaDer = Buscar("PIERNA_DER");
        var hombroBrazoIzq = Buscar("Junta_1_5");
        var hombroBrazoDer = Buscar("Junta_1_4");
        var antebrazoIzq = Buscar("Junta_2_3");
        var antebrazoDer = Buscar("Junta_2_2");
        var manoIzq = Buscar("Junta_3_4");
        var manoDer = Buscar("Junta_3_3");
        var rodillaIzq = Buscar("Junta_1_3");
        var rodillaDer = Buscar("Junta_1_2");
        var pieIzq = Buscar("PIE_IZQ");
        var pieDer = Buscar("PIE_DER");
        var cuello = Buscar("Junta_1");

        var requeridos = new[] { torso, cabeza, brazoIzq, brazoDer, piernaIzq, piernaDer, hombroBrazoIzq, hombroBrazoDer, antebrazoIzq, antebrazoDer, manoIzq, manoDer, rodillaIzq, rodillaDer, pieIzq, pieDer, cuello };
        if (requeridos.Any(t => t == null))
        {
            log.AppendLine("Faltan transformaciones del rig de CJ");
            Object.DestroyImmediate(instancia);
            return default;
        }

        var cadera = new GameObject("Cadera").transform;
        cadera.SetParent(instancia.transform, false);
        cadera.position = (piernaIzq.position + piernaDer.position) * 0.5f;

        torso.SetParent(cadera, true);
        cabeza.SetParent(torso, true);
        brazoIzq.SetParent(torso, true);
        brazoDer.SetParent(torso, true);
        piernaIzq.SetParent(cadera, true);
        piernaDer.SetParent(cadera, true);

        var mapa = new (string Humano, Transform Hueso)[]
        {
            ("Hips", cadera),
            ("Spine", torso),
            ("Neck", cabeza),
            ("Head", cuello),
            ("LeftShoulder", brazoIzq),
            ("LeftUpperArm", hombroBrazoIzq),
            ("LeftLowerArm", antebrazoIzq),
            ("LeftHand", manoIzq),
            ("RightShoulder", brazoDer),
            ("RightUpperArm", hombroBrazoDer),
            ("RightLowerArm", antebrazoDer),
            ("RightHand", manoDer),
            ("LeftUpperLeg", piernaIzq),
            ("LeftLowerLeg", rodillaIzq),
            ("LeftFoot", pieIzq),
            ("RightUpperLeg", piernaDer),
            ("RightLowerLeg", rodillaDer),
            ("RightFoot", pieDer),
        };

        var human = mapa.Select(par => new HumanBone
        {
            boneName = par.Hueso.name,
            humanName = par.Humano,
            limit = new HumanLimit { useDefaultValues = true }
        }).ToArray();

        var transforms = instancia.GetComponentsInChildren<Transform>(true);
        var skeleton = transforms.Select(t => new SkeletonBone
        {
            name = t.name,
            position = t.localPosition,
            rotation = t.localRotation,
            scale = t.localScale
        }).ToArray();

        var descripcion = new HumanDescription
        {
            human = human,
            skeleton = skeleton,
            upperArmTwist = 0.5f,
            lowerArmTwist = 0.5f,
            upperLegTwist = 0.5f,
            lowerLegTwist = 0.5f,
            armStretch = 0.05f,
            legStretch = 0.05f,
            feetSpacing = 0f,
            hasTranslationDoF = false
        };

        var avatar = AvatarBuilder.BuildHumanAvatar(instancia, descripcion);
        avatar.name = "CJAvatar";
        log.AppendLine("BuildHumanAvatar valido=" + avatar.isValid + " humano=" + avatar.isHuman);
        if (!avatar.isValid)
        {
            Object.DestroyImmediate(instancia);
            return default;
        }

        var rutaAvatar = "Assets/Prefabs/Personaje/cj/CJAvatar.asset";
        var previo = AssetDatabase.LoadAssetAtPath<Avatar>(rutaAvatar);
        if (previo != null) AssetDatabase.DeleteAsset(rutaAvatar);
        AssetDatabase.CreateAsset(avatar, rutaAvatar);
        return new AvatarArmado { avatar = avatar, instancia = instancia };
    }

    static Avatar CargarAvatar()
    {
        return AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Avatar>().FirstOrDefault();
    }

    static void VolcarJerarquia(StringBuilder log)
    {
        var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        var instancia = Object.Instantiate(modelo);
        instancia.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        instancia.transform.localScale = Vector3.one;
        foreach (var t in instancia.GetComponentsInChildren<Transform>(true))
        {
            var profundidad = 0;
            var actual = t;
            while (actual.parent != null && actual.parent != instancia.transform.parent)
            {
                profundidad++;
                actual = actual.parent;
                if (profundidad > 30) break;
            }

            log.AppendLine(new string(' ', profundidad) + t.name);
        }

        var bounds = MedirBounds(instancia);
        log.AppendLine("bounds size=" + bounds.size + " min=" + bounds.min + " max=" + bounds.max);
        Object.DestroyImmediate(instancia);
    }

    static HumanBone[] MapearHuesos(StringBuilder log)
    {
        var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        var instancia = Object.Instantiate(modelo);
        var nombres = instancia.GetComponentsInChildren<Transform>(true)
            .Select(t => t.name)
            .Distinct()
            .ToList();
        Object.DestroyImmediate(instancia);

        var usados = new HashSet<string>();
        var huesos = new List<HumanBone>();
        foreach (var (humano, claves) in MapaHuesos)
        {
            var elegido = nombres.FirstOrDefault(nombre =>
            {
                if (usados.Contains(nombre)) return false;
                var clave = Normalizar(nombre);
                return claves.Any(c => clave == c || clave.EndsWith(c) || clave.Contains(c));
            });

            if (elegido == null)
            {
                log.AppendLine("sin hueso para " + humano);
                continue;
            }

            usados.Add(elegido);
            huesos.Add(new HumanBone
            {
                boneName = elegido,
                humanName = humano,
                limit = new HumanLimit { useDefaultValues = true }
            });
            log.AppendLine(humano + " <- " + elegido);
        }

        string[] requeridos = { "Hips", "Spine", "Head", "LeftUpperArm", "LeftLowerArm", "LeftHand", "RightUpperArm", "RightLowerArm", "RightHand", "LeftUpperLeg", "LeftLowerLeg", "LeftFoot", "RightUpperLeg", "RightLowerLeg", "RightFoot" };
        var tienen = new HashSet<string>(huesos.Select(h => h.humanName));
        if (requeridos.Any(r => !tienen.Contains(r)))
        {
            return null;
        }

        return huesos.ToArray();
    }

    static string Normalizar(string nombre)
    {
        var chars = nombre.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray();
        return new string(chars);
    }

    static void AjustarEscala(StringBuilder log)
    {
        var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        var instancia = Object.Instantiate(modelo);
        instancia.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var bounds = MedirBounds(instancia);
        Object.DestroyImmediate(instancia);

        var alto = Mathf.Max(bounds.size.y, 0.0001f);
        log.AppendLine("alto antes de escala=" + alto);
        if (alto > 1.4f && alto < 2.2f)
        {
            return;
        }

        var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
        importer.globalScale = importer.globalScale * (AlturaObjetivo / alto);
        log.AppendLine("globalScale=" + importer.globalScale);
        importer.SaveAndReimport();
    }

    static Bounds MedirBounds(GameObject modelo)
    {
        var tiene = false;
        var bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var renderer in modelo.GetComponentsInChildren<Renderer>())
        {
            var malla = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh : (renderer as MeshRenderer)?.GetComponent<MeshFilter>()?.sharedMesh;
            if (malla == null) continue;
            foreach (var vertice in malla.vertices)
            {
                var local = modelo.transform.InverseTransformPoint(renderer.transform.TransformPoint(vertice));
                if (!tiene)
                {
                    bounds = new Bounds(local, Vector3.zero);
                    tiene = true;
                }
                else
                {
                    bounds.Encapsulate(local);
                }
            }
        }

        if (!tiene)
        {
            bounds = new Bounds(Vector3.zero, Vector3.one);
        }

        return bounds;
    }

    static void AsignarTexturas(StringBuilder log)
    {
        var texturas = new Dictionary<string, Texture2D>
        {
            { "face", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prefabs/Personaje/cj/textures/face.png") },
            { "torso", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prefabs/Personaje/cj/textures/torso.png") },
            { "pants", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prefabs/Personaje/cj/textures/pants.png") },
            { "sneaker", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prefabs/Personaje/cj/textures/sneakers.png") },
            { "shoe", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prefabs/Personaje/cj/textures/sneakers.png") },
        };

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var carpeta = Path.GetDirectoryName(FbxPath);
        foreach (var material in AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Material>())
        {
            var clave = Normalizar(material.name);
            Texture2D textura = null;
            foreach (var par in texturas)
            {
                if (clave.Contains(par.Key))
                {
                    textura = par.Value;
                    break;
                }
            }

            if (textura == null)
            {
                log.AppendLine("material sin textura " + material.name);
                continue;
            }

            var ruta = carpeta + "/" + material.name + ".mat";
            var externo = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (externo == null)
            {
                externo = new Material(shader != null ? shader : Shader.Find("Standard"));
                AssetDatabase.CreateAsset(externo, ruta);
            }

            if (externo.HasProperty("_BaseMap")) externo.SetTexture("_BaseMap", textura);
            if (externo.HasProperty("_MainTex")) externo.SetTexture("_MainTex", textura);
            externo.color = Color.white;
            EditorUtility.SetDirty(externo);
            log.AppendLine("textura " + material.name + " <- " + textura.name);
        }

        AssetDatabase.SaveAssets();
        var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
        importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Local);
        importer.SaveAndReimport();
    }

    static void AsegurarTag(string tag)
    {
        if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(tag))
        {
            UnityEditorInternal.InternalEditorUtility.AddTag(tag);
        }
    }

    static GameObject CrearPrefab(AvatarArmado armado, StringBuilder log)
    {
        AsegurarTag("Player");
        AsegurarTag("CinemachineTarget");
        var fuente = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePlayer);
        var controlador = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        var avatar = armado.avatar;

        var raiz = new GameObject("CJ");
        raiz.tag = "Player";
        raiz.layer = fuente.layer;

        foreach (var componente in fuente.GetComponents<Component>())
        {
            if (componente is Transform || componente is Animator || componente is PlayerInput) continue;
            var copia = raiz.AddComponent(componente.GetType());
            EditorUtility.CopySerialized(componente, copia);
        }

        var visual = armado.instancia;
        visual.transform.SetParent(raiz.transform, false);

        var medida = MedirBounds(visual);
        var correccion = CorregirOrientacion(visual, medida, log);
        medida = correccion;
        visual.transform.localPosition = new Vector3(0f, -medida.min.y, 0f);
        log.AppendLine("offset pies=" + visual.transform.localPosition.y + " alto=" + medida.size.y);

        var animator = visual.GetComponent<Animator>();
        if (animator == null) animator = visual.AddComponent<Animator>();
        animator.avatar = avatar;
        animator.runtimeAnimatorController = controlador;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        var camara = new GameObject("PlayerCameraRoot");
        camara.tag = "CinemachineTarget";
        camara.transform.SetParent(raiz.transform, false);
        camara.transform.localPosition = new Vector3(0f, Mathf.Clamp(medida.size.y * 0.9f, 1.2f, 1.7f), 0f);

        var movimiento = raiz.GetComponent<ThirdPersonController>();
        movimiento.CinemachineCameraTarget = camara;

        var input = raiz.GetComponent<PlayerInput>();
        if (input == null) input = raiz.AddComponent<PlayerInput>();
        var inputFuente = fuente.GetComponent<PlayerInput>();
        input.actions = inputFuente.actions;
        input.defaultActionMap = inputFuente.defaultActionMap;
        input.notificationBehavior = PlayerNotifications.SendMessages;
        input.neverAutoSwitchControlSchemes = inputFuente.neverAutoSwitchControlSchemes;

        var previo = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (previo != null) AssetDatabase.DeleteAsset(PrefabPath);
        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, PrefabPath);
        Object.DestroyImmediate(raiz);
        log.AppendLine("prefab " + PrefabPath);
        return prefab;
    }

    static Bounds CorregirOrientacion(GameObject visual, Bounds medida, StringBuilder log)
    {
        if (medida.size.y >= medida.size.x && medida.size.y >= medida.size.z)
        {
            return medida;
        }

        visual.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        medida = MedirBounds(visual);
        log.AppendLine("rotacion -90 en X, bounds=" + medida.size);
        if (medida.size.y >= medida.size.x && medida.size.y >= medida.size.z)
        {
            return medida;
        }

        visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        medida = MedirBounds(visual);
        log.AppendLine("rotacion 90 en X, bounds=" + medida.size);
        return medida;
    }

    static void ReemplazarEnEscena(GameObject prefab, StringBuilder log)
    {
        var escena = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var anterior = GameObject.Find("PlayerArmature");
        if (anterior == null)
        {
            log.AppendLine("MapaV1 no tiene PlayerArmature");
            return;
        }

        var posicion = anterior.transform.position;
        var rotacion = anterior.transform.rotation;
        Object.DestroyImmediate(anterior);

        var jugador = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        jugador.transform.SetPositionAndRotation(posicion, rotacion);

        var objetivo = jugador.transform.Find("PlayerCameraRoot");
        foreach (var camara in Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
        {
            camara.Target.TrackingTarget = objetivo;
            camara.Target.LookAtTarget = objetivo;
            EditorUtility.SetDirty(camara);
            log.AppendLine("camara " + camara.name + " sigue a PlayerCameraRoot");
        }

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        log.AppendLine("CJ en " + posicion);
    }
}
