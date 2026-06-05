#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// One-click setup for Quest 3 (UI). Builds the HUD / GameOver / Victory canvases,
/// creates and wires the GameManager, attaches PlayerHealth, and configures the
/// jewel / saw / sign prefabs. Run via the menu:  Quest3 -> Setup Scene (UI + Prefabs)
/// </summary>
public static class Quest3Setup
{
    private const string JewelPath = "Assets/Prefabs/World/jewel.prefab";
    private const string SawPath = "Assets/Prefabs/World/saw.prefab";
    private const string SignPath = "Assets/Prefabs/World/sign.prefab";

    [MenuItem("Quest3/Bind Third-Person Camera (Mouse)")]
    public static void BindCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            EditorUtility.DisplayDialog("Quest 3", "No Main Camera found in the scene.", "OK");
            return;
        }

        PlayerMovement playerMove = Object.FindFirstObjectByType<PlayerMovement>();
        if (playerMove == null)
        {
            EditorUtility.DisplayDialog("Quest 3", "No player (PlayerMovement) found in the scene.", "OK");
            return;
        }
        Transform player = playerMove.transform;

        // Remove the simple follow component if it was added earlier.
        CameraFollow oldFollow = cam.GetComponent<CameraFollow>();
        if (oldFollow != null)
            Undo.DestroyObjectImmediate(oldFollow);

        ThirdPersonCameraController tpc = cam.GetComponent<ThirdPersonCameraController>();
        if (tpc == null)
            tpc = Undo.AddComponent<ThirdPersonCameraController>(cam.gameObject);

        SerializedObject so = new SerializedObject(tpc);
        so.FindProperty("target").objectReferenceValue = player;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Quest 3",
            "Third-person mouse camera bound to the player (behind/above). Press Play and move the mouse.", "OK");
    }

    private static readonly string[] SignTexts =
    {
        "Vorsicht, Saegen voraus!",
        "Warum springt das Huhn? Damit DU nicht stehen bleibst!",
        "Tipp: Spring von oben auf Gegner!",
        "Nicht runterfallen - sonst war's das!",
        "Das Juwel wartet am Ende auf dich.",
        "Bewegende Plattformen koennen wehtun!",
        "Mage-Regel Nr.1: Immer in Bewegung bleiben.",
        "Fast geschafft - oder doch nicht?",
    };

    [MenuItem("Quest3/Fill Sign Texts")]
    public static void FillSignTexts()
    {
        SignDisplay[] signs = Object.FindObjectsByType<SignDisplay>(FindObjectsSortMode.InstanceID);
        if (signs.Length == 0)
        {
            EditorUtility.DisplayDialog("Quest 3",
                "No signs with a SignDisplay component were found. Place sign prefabs first.", "OK");
            return;
        }

        for (int i = 0; i < signs.Length; i++)
        {
            SerializedObject so = new SerializedObject(signs[i]);
            so.FindProperty("message").stringValue = SignTexts[i % SignTexts.Length];
            so.FindProperty("alwaysVisible").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Quest 3",
            $"Assigned sayings to {signs.Length} sign(s). Walk up to a sign in Play mode to read it.", "OK");
    }

    [MenuItem("Quest3/Add Damage To Enemies")]
    public static void AddDamageToEnemies()
    {
        EnemyPatrol[] enemies = Object.FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        if (enemies.Length == 0)
        {
            EditorUtility.DisplayDialog("Quest 3",
                "No enemies with an EnemyPatrol component were found in the scene.", "OK");
            return;
        }

        int added = 0;
        foreach (EnemyPatrol enemy in enemies)
        {
            if (enemy.GetComponent<EnemyDamage>() == null)
            {
                Undo.AddComponent<EnemyDamage>(enemy.gameObject);
                added++;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Quest 3",
            $"Found {enemies.Length} enemy/enemies. Added EnemyDamage to {added} of them.\n\n" +
            "Note: the level needs at least 2 creatures for full marks.", "OK");
    }

    [MenuItem("Quest3/Fix Prefab Colliders")]
    public static void FixPrefabColliders()
    {
        ConfigurePrefab(JewelPath, typeof(JewelPickup));
        ConfigurePrefab(SawPath, typeof(SawTrap));
        ConfigurePrefab(SignPath, typeof(SignDisplay));
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Quest 3",
            "Prefab colliders fixed (convex + trigger). Jewel, saw and sign now fire OnTriggerEnter.", "OK");
    }

    [MenuItem("Quest3/Setup Scene (UI + Prefabs)")]
    public static void SetupScene()
    {
        // --- 1. Prefabs first (independent of scene) ---------------------------
        ConfigurePrefab(JewelPath, typeof(JewelPickup));
        ConfigurePrefab(SawPath, typeof(SawTrap));
        ConfigurePrefab(SignPath, typeof(SignDisplay));

        // --- 2. Event System --------------------------------------------------
        EnsureEventSystem();

        // --- 3. Canvases ------------------------------------------------------
        CanvasGroup hud = BuildHud(out Slider healthSlider, out TextMeshProUGUI coinText);
        CanvasGroup gameOver = BuildGameOver();
        CanvasGroup victory = BuildVictory();

        // --- 4. Player + respawn point ---------------------------------------
        PlayerMovement playerMove = Object.FindFirstObjectByType<PlayerMovement>();
        Transform player = playerMove != null ? playerMove.transform : null;

        if (player != null && player.GetComponent<PlayerHealth>() == null)
            Undo.AddComponent<PlayerHealth>(player.gameObject);

        Transform respawnPoint = GetOrCreateRespawnPoint(player);

        // --- 5. GameManager + wiring -----------------------------------------
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gmGo, "Create GameManager");
            gm = gmGo.AddComponent<GameManager>();
        }

        SerializedObject so = new SerializedObject(gm);
        SetRef(so, "player", player);
        SetRef(so, "respawnPoint", respawnPoint);
        SetRef(so, "healthSlider", healthSlider);
        SetRef(so, "coinText", coinText);
        SetRef(so, "hudCanvasGroup", hud);
        SetRef(so, "gameOverCanvasGroup", gameOver);
        SetRef(so, "victoryCanvasGroup", victory);
        so.ApplyModifiedPropertiesWithoutUndo();

        // --- 6. Wire the GameOver buttons to the GameManager -----------------
        WireButton(gameOver, "RespawnButton", gm.Respawn);
        WireButton(gameOver, "ExitButton", gm.QuitGame);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("[Quest3Setup] Done. Place jewel, 2+ saws, 2+ signs and 2+ enemies (add EnemyDamage) in the level. " +
                  "Verify the RespawnTrigger volume below the level uses a trigger collider.");
        EditorUtility.DisplayDialog("Quest 3 Setup",
            "UI, GameManager and prefabs are configured.\n\n" +
            "Still to do by hand (level design):\n" +
            "- Drag the jewel to the end of the level\n" +
            "- Place 2+ saws and 2+ signs\n" +
            "- Add the EnemyDamage component to 2+ creatures\n\n" +
            "Everything else is wired up.", "OK");
    }

    // ------------------------------------------------------------------ Prefabs
    private static void ConfigurePrefab(string path, System.Type scriptType)
    {
        GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (root == null)
        {
            Debug.LogWarning($"[Quest3Setup] Prefab not found: {path}");
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(path);

        if (contents.GetComponent(scriptType) == null)
            contents.AddComponent(scriptType);

        Collider col = contents.GetComponentInChildren<Collider>();
        if (col == null)
        {
            // No collider at all -> add a trigger box sized to the renderer bounds.
            BoxCollider box = contents.AddComponent<BoxCollider>();
            box.isTrigger = true;
            Renderer r = contents.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                box.center = contents.transform.InverseTransformPoint(r.bounds.center);
                box.size = r.bounds.size;
            }
            else
            {
                box.size = Vector3.one;
            }
        }
        else if (col is MeshCollider meshCol)
        {
            // A non-convex MeshCollider can NOT be a trigger, so make it convex first.
            meshCol.convex = true;
            meshCol.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }

        PrefabUtility.SaveAsPrefabAsset(contents, path);
        PrefabUtility.UnloadPrefabContents(contents);
        Debug.Log($"[Quest3Setup] Configured prefab: {path}");
    }

    // -------------------------------------------------------------- Event System
    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // ---------------------------------------------------------------- HUD canvas
    private static CanvasGroup BuildHud(out Slider healthSlider, out TextMeshProUGUI coinText)
    {
        GameObject canvasGo = CreateCanvas("HUD Canvas", 0);
        CanvasGroup cg = canvasGo.AddComponent<CanvasGroup>();

        // Health bar (top-left)
        healthSlider = CreateHealthBar(canvasGo.transform);

        // Coin counter (top-right)
        GameObject coinGo = new GameObject("CoinText", typeof(RectTransform));
        coinGo.transform.SetParent(canvasGo.transform, false);
        coinText = coinGo.AddComponent<TextMeshProUGUI>();
        coinText.text = "0";
        coinText.fontSize = 48;
        coinText.alignment = TextAlignmentOptions.TopRight;
        RectTransform coinRt = coinGo.GetComponent<RectTransform>();
        coinRt.anchorMin = new Vector2(1, 1);
        coinRt.anchorMax = new Vector2(1, 1);
        coinRt.pivot = new Vector2(1, 1);
        coinRt.anchoredPosition = new Vector2(-30, -30);
        coinRt.sizeDelta = new Vector2(200, 60);

        GameObject coinLabel = new GameObject("CoinLabel", typeof(RectTransform));
        coinLabel.transform.SetParent(coinGo.transform, false);
        TextMeshProUGUI label = coinLabel.AddComponent<TextMeshProUGUI>();
        label.text = "Coins:";
        label.fontSize = 32;
        label.alignment = TextAlignmentOptions.MidlineRight;
        RectTransform labelRt = coinLabel.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0, 0.5f);
        labelRt.anchorMax = new Vector2(0, 0.5f);
        labelRt.pivot = new Vector2(1, 0.5f);
        labelRt.anchoredPosition = new Vector2(-10, 0);
        labelRt.sizeDelta = new Vector2(200, 60);

        Undo.RegisterCreatedObjectUndo(canvasGo, "Create HUD");
        return cg;
    }

    private static Slider CreateHealthBar(Transform parent)
    {
        GameObject sliderGo = new GameObject("HealthBar", typeof(RectTransform));
        sliderGo.transform.SetParent(parent, false);
        RectTransform sRt = sliderGo.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 1);
        sRt.anchorMax = new Vector2(0, 1);
        sRt.pivot = new Vector2(0, 1);
        sRt.anchoredPosition = new Vector2(30, -30);
        sRt.sizeDelta = new Vector2(300, 30);

        Slider slider = sliderGo.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;

        // Background
        GameObject bg = CreateUiImage("Background", sliderGo.transform, new Color(0.15f, 0.15f, 0.15f, 0.9f));
        Stretch(bg.GetComponent<RectTransform>());

        // Fill area + fill
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        GameObject fill = CreateUiImage("Fill", fillArea.transform, new Color(0.85f, 0.15f, 0.15f, 1f));
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(1, 1);
        fillRt.sizeDelta = Vector2.zero;

        slider.fillRect = fillRt;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    // ---------------------------------------------------------- GameOver canvas
    private static CanvasGroup BuildGameOver()
    {
        GameObject canvasGo = CreateCanvas("GameOver Canvas", 10);
        CanvasGroup cg = canvasGo.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // dark backdrop
        GameObject bg = CreateUiImage("Backdrop", canvasGo.transform, new Color(0, 0, 0, 0.7f));
        Stretch(bg.GetComponent<RectTransform>());

        CreateTitle(canvasGo.transform, "Game Over", new Color(0.9f, 0.2f, 0.2f), 120);

        CreateButton(canvasGo.transform, "RespawnButton", "Respawn", new Vector2(0, -40));
        CreateButton(canvasGo.transform, "ExitButton", "Exit", new Vector2(0, -160));

        Undo.RegisterCreatedObjectUndo(canvasGo, "Create GameOver");
        return cg;
    }

    // ----------------------------------------------------------- Victory canvas
    private static CanvasGroup BuildVictory()
    {
        GameObject canvasGo = CreateCanvas("Victory Canvas", 10);
        CanvasGroup cg = canvasGo.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        GameObject bg = CreateUiImage("Backdrop", canvasGo.transform, new Color(0.05f, 0.05f, 0.15f, 0.85f));
        Stretch(bg.GetComponent<RectTransform>());

        CreateTitle(canvasGo.transform, "You Win!", new Color(1f, 0.85f, 0.2f), 130);

        GameObject sub = new GameObject("Subtitle", typeof(RectTransform));
        sub.transform.SetParent(canvasGo.transform, false);
        TextMeshProUGUI subTxt = sub.AddComponent<TextMeshProUGUI>();
        subTxt.text = "You collected the jewel!";
        subTxt.fontSize = 44;
        subTxt.alignment = TextAlignmentOptions.Center;
        RectTransform subRt = sub.GetComponent<RectTransform>();
        subRt.anchoredPosition = new Vector2(0, -120);
        subRt.sizeDelta = new Vector2(900, 80);

        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Victory");
        return cg;
    }

    // ------------------------------------------------------------------ Helpers
    private static GameObject CreateCanvas(string name, int sortOrder)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private static GameObject CreateUiImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static void CreateTitle(Transform parent, string text, Color color, float fontSize)
    {
        GameObject go = new GameObject("Title", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 180);
        rt.sizeDelta = new Vector2(1200, 200);
    }

    private static void CreateButton(Transform parent, string name, string label, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(360, 90);

        GameObject txtGo = new GameObject("Text", typeof(RectTransform));
        txtGo.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 44;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        Stretch(txtGo.GetComponent<RectTransform>());
    }

    private static void WireButton(CanvasGroup canvas, string buttonName, UnityEngine.Events.UnityAction action)
    {
        Transform t = canvas.transform.Find(buttonName);
        if (t == null) return;
        Button btn = t.GetComponent<Button>();
        if (btn == null) return;

        // remove existing persistent listeners to avoid duplicates on re-run
        for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(btn.onClick, i);

        UnityEventTools.AddPersistentListener(btn.onClick, action);
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Transform GetOrCreateRespawnPoint(Transform player)
    {
        GameObject existing = GameObject.Find("RespawnPoint");
        if (existing != null) return existing.transform;

        GameObject go = new GameObject("RespawnPoint");
        Undo.RegisterCreatedObjectUndo(go, "Create RespawnPoint");
        go.transform.position = player != null ? player.position : Vector3.zero;
        return go.transform;
    }

    private static void SetRef(SerializedObject so, string propName, Object value)
    {
        SerializedProperty p = so.FindProperty(propName);
        if (p != null) p.objectReferenceValue = value;
    }
}
#endif
