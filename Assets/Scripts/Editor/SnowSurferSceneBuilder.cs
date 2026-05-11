using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SnowSurfer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SnowSurfer.Editor
{
    public static class SnowSurferSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string PrefabRoot = "Assets/Prefabs";

        [MenuItem("Snow Surfer/Rebuild Playable Scene")]
        public static void Build()
        {
            EnsureFolders();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClearGeneratedObjects();

            Camera camera = BuildCamera();
            BuildLight();

            Sprite floorFill = LoadSprite("Assets/Sprites/Floor Tile Fill.png");
            Sprite floorRidge = LoadSprite("Assets/Sprites/Floor Tile.png");
            Sprite dino = LoadSprite("Assets/Sprites/Snowboarding Dino.png");
            Sprite gift = LoadSprite("Assets/Sprites/Gift Bag.png");

            GameObject playerPrefab = CreatePlayerPrefab(dino);
            GameObject[] obstaclePrefabs = CreateObstaclePrefabs();
            GameObject[] collectiblePrefabs = { CreateCollectiblePrefab(gift) };

            GameObject gameRoot = new GameObject("GameRoot");
            GameManager gameManager = gameRoot.AddComponent<GameManager>();
            DifficultyManager difficulty = gameRoot.AddComponent<DifficultyManager>();
            ScoreManager score = gameRoot.AddComponent<ScoreManager>();
            AudioManager audio = gameRoot.AddComponent<AudioManager>();
            ObjectPool pool = gameRoot.AddComponent<ObjectPool>();
            SpawnManager spawn = gameRoot.AddComponent<SpawnManager>();
            UIManager ui = gameRoot.AddComponent<UIManager>();
            MissionManager missions = gameRoot.AddComponent<MissionManager>();

            GameObject poolRoot = new GameObject("Pool");
            poolRoot.transform.SetParent(gameRoot.transform);
            SetField(pool, "poolRoot", poolRoot.transform);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.name = "Player";
            player.transform.position = new Vector3(0f, -2.55f, 0f);
            PlayerController playerController = player.GetComponent<PlayerController>();

            ParallaxBackground parallax = BuildBackground(floorFill, floorRidge, difficulty);
            parallax.transform.SetParent(gameRoot.transform);

            UIReferences uiReferences = BuildUI();
            AssignUI(ui, uiReferences);

            CameraShake shake = camera.GetComponent<CameraShake>();
            SetField(gameManager, "player", playerController);
            SetField(gameManager, "spawnManager", spawn);
            SetField(gameManager, "scoreManager", score);
            SetField(gameManager, "difficultyManager", difficulty);
            SetField(gameManager, "uiManager", ui);
            SetField(gameManager, "audioManager", audio);
            SetField(gameManager, "cameraShake", shake);
            SetField(gameManager, "missionManager", missions);

            SetField(spawn, "objectPool", pool);
            SetField(spawn, "obstaclePrefabs", obstaclePrefabs);
            SetField(spawn, "collectiblePrefabs", collectiblePrefabs);

            Selection.activeGameObject = gameRoot;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Snow Surfer playable scene rebuilt.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scripts/Runtime");
            Directory.CreateDirectory("Assets/Scripts/Editor");
            Directory.CreateDirectory(PrefabRoot);
            Directory.CreateDirectory(PrefabRoot + "/Player");
            Directory.CreateDirectory(PrefabRoot + "/Obstacles");
            Directory.CreateDirectory(PrefabRoot + "/Collectibles");
        }

        private static void ClearGeneratedObjects()
        {
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
            {
                if (go == null)
                {
                    continue;
                }

                if (go.name == "GameRoot" || go.name == "Player" || go.name == "Background" || go.name == "Game Canvas" || go.name == "EventSystem")
                {
                    toDestroy.Add(go);
                }
            }

            foreach (GameObject go in toDestroy)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
        }

        private static Camera BuildCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.72f, 0.86f, 0.98f, 1f);
            if (camera.GetComponent<CameraShake>() == null)
            {
                camera.gameObject.AddComponent<CameraShake>();
            }

            return camera;
        }

        private static void BuildLight()
        {
            if (Object.FindAnyObjectByType<UnityEngine.Rendering.Universal.Light2D>() != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Global Light 2D");
            var light = lightObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light.intensity = 1.05f;
        }

        private static ParallaxBackground BuildBackground(Sprite floorFill, Sprite floorRidge, DifficultyManager difficulty)
        {
            GameObject root = new GameObject("Background");
            ParallaxBackground parallax = root.AddComponent<ParallaxBackground>();
            SetField(parallax, "difficultyManager", difficulty);
            SetField(parallax, "groundTileHeight", 3.58f);

            List<Transform> groundTiles = new List<Transform>();
            for (int row = -2; row <= 2; row++)
            {
                for (int column = -2; column <= 2; column++)
                {
                    GameObject tile = CreateCenteredSprite("Snow Tile", floorFill, new Vector3(column * 3.58f, row * 3.58f, 3f), 1.4f, -20, new Color(0.93f, 0.98f, 1f, 1f));
                    tile.transform.SetParent(root.transform);
                    groundTiles.Add(tile.transform);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject ridge = CreateCenteredSprite("Snow Ridge", floorRidge, new Vector3(0f, -4.9f + i * 3.4f, 2f), 1.02f, -18, new Color(0.84f, 0.94f, 1f, 0.48f));
                ridge.transform.SetParent(root.transform);
                groundTiles.Add(ridge.transform);
            }

            Sprite[] cloudSprites =
            {
                LoadSprite("Assets/Sprites/Cloud 1.png"),
                LoadSprite("Assets/Sprites/Cloud 2.png"),
                LoadSprite("Assets/Sprites/Cloud 3.png")
            };

            List<Transform> clouds = new List<Transform>();
            Vector3[] cloudPositions =
            {
                new Vector3(-4.7f, 4.2f, 1f),
                new Vector3(2.8f, 2.7f, 1f),
                new Vector3(-1.4f, 5.9f, 1f),
                new Vector3(4.9f, 6.8f, 1f)
            };

            for (int i = 0; i < cloudPositions.Length; i++)
            {
                GameObject cloud = CreateCenteredSprite("Cloud", cloudSprites[i % cloudSprites.Length], cloudPositions[i], 0.43f, -10, new Color(1f, 1f, 1f, 0.72f));
                cloud.transform.SetParent(root.transform);
                clouds.Add(cloud.transform);
            }

            SetField(parallax, "groundTiles", groundTiles.ToArray());
            SetField(parallax, "clouds", clouds.ToArray());
            return parallax;
        }

        private static GameObject CreatePlayerPrefab(Sprite sprite)
        {
            GameObject root = new GameObject("Player");
            Transform visual = AddCenteredVisual(root, sprite, 0.38f, 20, Color.white).transform;
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.05f, 0.72f);
            collider.offset = new Vector2(0.03f, -0.05f);
            PlayerController controller = root.AddComponent<PlayerController>();
            SetField(controller, "visual", visual);

            ParticleSystem trail = CreateSnowTrail(root.transform);
            SetField(controller, "snowTrail", trail);

            return SavePrefab(root, PrefabRoot + "/Player/Player.prefab");
        }

        private static GameObject[] CreateObstaclePrefabs()
        {
            return new[]
            {
                CreateObstaclePrefab("Rock 1", "Assets/Sprites/Rock 1.png", 0.36f, new Vector2(0.65f, 0.72f), new Vector2(0f, -0.06f)),
                CreateObstaclePrefab("Rock 2", "Assets/Sprites/Rock 2.png", 0.38f, new Vector2(0.72f, 0.58f), new Vector2(0f, -0.05f)),
                CreateObstaclePrefab("Post 1", "Assets/Sprites/Post 1.png", 0.28f, new Vector2(0.42f, 0.95f), new Vector2(0f, -0.05f)),
                CreateObstaclePrefab("Post 2", "Assets/Sprites/Post 2.png", 0.22f, new Vector2(0.38f, 1.05f), new Vector2(0f, -0.08f)),
                CreateObstaclePrefab("Tree Fallen", "Assets/Sprites/Tree Fallen.png", 0.23f, new Vector2(1.46f, 0.55f), new Vector2(0f, -0.04f)),
                CreateObstaclePrefab("Tree 4", "Assets/Sprites/Tree 4.png", 0.16f, new Vector2(0.56f, 1.18f), new Vector2(0f, -0.12f)),
                CreateObstaclePrefab("Tree 3", "Assets/Sprites/Tree 3.png", 0.14f, new Vector2(0.44f, 1.25f), new Vector2(0f, -0.1f)),
                CreateObstaclePrefab("Tree 2", "Assets/Sprites/Tree 2.png", 0.12f, new Vector2(0.54f, 1.38f), new Vector2(0f, -0.12f)),
                CreateObstaclePrefab("Tree 1", "Assets/Sprites/Tree 1.png", 0.11f, new Vector2(0.58f, 1.34f), new Vector2(0f, -0.12f))
            };
        }

        private static GameObject CreateObstaclePrefab(string name, string spritePath, float scale, Vector2 colliderSize, Vector2 colliderOffset)
        {
            GameObject root = new GameObject(name);
            AddCenteredVisual(root, LoadSprite(spritePath), scale, 12, Color.white);
            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = colliderSize;
            collider.offset = colliderOffset;
            root.AddComponent<MovingEntity>();
            root.AddComponent<Obstacle>();
            root.AddComponent<PooledObject>();
            return SavePrefab(root, PrefabRoot + "/Obstacles/" + name + ".prefab");
        }

        private static GameObject CreateCollectiblePrefab(Sprite sprite)
        {
            GameObject root = new GameObject("Gift Collectible");
            AddCenteredVisual(root, sprite, 0.34f, 16, Color.white);
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.36f;
            root.AddComponent<MovingEntity>();
            root.AddComponent<Collectible>();
            root.AddComponent<PooledObject>();
            return SavePrefab(root, PrefabRoot + "/Collectibles/Gift Collectible.prefab");
        }

        private static ParticleSystem CreateSnowTrail(Transform parent)
        {
            GameObject trailObject = new GameObject("Snow Trail");
            trailObject.transform.SetParent(parent, false);
            trailObject.transform.localPosition = new Vector3(0f, -0.46f, 0.1f);
            ParticleSystem particles = trailObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 0.7f;
            main.startSize = 0.08f;
            main.startColor = new Color(1f, 1f, 1f, 0.76f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.rateOverTime = 22f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 16f;
            shape.radius = 0.08f;
            return particles;
        }

        private static GameObject CreateCenteredSprite(string name, Sprite sprite, Vector3 position, float scale, int sortingOrder, Color color)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;
            AddCenteredVisual(root, sprite, scale, sortingOrder, color);
            return root;
        }

        private static GameObject AddCenteredVisual(GameObject root, Sprite sprite, float scale, int sortingOrder, Color color)
        {
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            visual.transform.localScale = Vector3.one * scale;
            Vector2 size = sprite.rect.size / sprite.pixelsPerUnit * scale;
            visual.transform.localPosition = new Vector3(-size.x * 0.5f, -size.y * 0.5f, 0f);
            return visual;
        }

        private static UIReferences BuildUI()
        {
            GameObject canvasObject = new GameObject("Game Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            BuildEventSystem();

            UIReferences refs = new UIReferences();
            refs.startPanel = CreatePanel(canvasObject.transform, "Start Panel", new Color(0.62f, 0.8f, 0.95f, 0.28f));
            CreateText(refs.startPanel.transform, "Title", "Snow Surfer", 88, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.7f), new Vector2(780f, 130f), new Color(0.1f, 0.22f, 0.38f, 1f));
            refs.startHighScoreText = CreateText(refs.startPanel.transform, "Start High Score", "Best 0", 36, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.62f), new Vector2(520f, 64f), new Color(0.16f, 0.3f, 0.48f, 1f));
            CreateText(refs.startPanel.transform, "Start Prompt", "Tap to Start", 46, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.22f), new Vector2(560f, 82f), new Color(0.08f, 0.18f, 0.32f, 1f));

            refs.hudPanel = CreatePanel(canvasObject.transform, "HUD Panel", new Color(0f, 0f, 0f, 0f));
            refs.scoreText = CreateText(refs.hudPanel.transform, "Score", "000000", 48, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.95f), new Vector2(360f, 70f), new Color(0.08f, 0.18f, 0.32f, 1f));
            refs.highScoreText = CreateText(refs.hudPanel.transform, "High Score", "Best 0", 28, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.91f), new Vector2(320f, 52f), new Color(0.22f, 0.36f, 0.54f, 1f));
            refs.comboText = CreateText(refs.hudPanel.transform, "Combo", "Combo -", 30, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.86f), new Vector2(340f, 52f), new Color(0.14f, 0.42f, 0.42f, 1f));
            refs.missionText = CreateText(refs.hudPanel.transform, "Mission", "Collect gifts 0/10", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.94f), new Vector2(520f, 58f), new Color(0.12f, 0.25f, 0.42f, 1f));
            refs.speedText = CreateText(refs.hudPanel.transform, "Speed", "Speed 0.0", 26, TextAnchor.MiddleRight, new Vector2(0.95f, 0.91f), new Vector2(240f, 52f), new Color(0.22f, 0.36f, 0.54f, 1f));
            refs.pauseButton = CreateButton(refs.hudPanel.transform, "Pause Button", "Pause", new Vector2(0.88f, 0.96f), new Vector2(150f, 58f));
            refs.muteButton = CreateButton(refs.hudPanel.transform, "Mute Button", "SFX On", new Vector2(0.88f, 0.055f), new Vector2(170f, 58f));
            refs.muteButtonText = refs.muteButton.GetComponentInChildren<Text>();

            refs.pauseOverlay = CreatePanel(canvasObject.transform, "Pause Overlay", new Color(0.07f, 0.16f, 0.28f, 0.48f));
            CreateText(refs.pauseOverlay.transform, "Paused Text", "Paused", 76, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.55f), new Vector2(520f, 120f), Color.white);

            refs.gameOverPanel = CreatePanel(canvasObject.transform, "Game Over Panel", new Color(0.64f, 0.77f, 0.9f, 0.42f));
            CreateText(refs.gameOverPanel.transform, "Game Over Title", "Game Over", 78, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.72f), new Vector2(720f, 115f), new Color(0.1f, 0.2f, 0.34f, 1f));
            refs.newBestText = CreateText(refs.gameOverPanel.transform, "New Best", "New Best!", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.64f), new Vector2(520f, 70f), new Color(0.02f, 0.45f, 0.52f, 1f));
            refs.finalScoreText = CreateText(refs.gameOverPanel.transform, "Final Score", "Score 0", 46, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.55f), new Vector2(520f, 78f), new Color(0.08f, 0.18f, 0.32f, 1f));
            refs.finalHighScoreText = CreateText(refs.gameOverPanel.transform, "Final High Score", "Best 0", 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.49f), new Vector2(520f, 58f), new Color(0.18f, 0.34f, 0.5f, 1f));
            refs.summaryText = CreateText(refs.gameOverPanel.transform, "Summary", "Gifts 0   Near Miss 0   Time 0s", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.41f), new Vector2(820f, 62f), new Color(0.14f, 0.28f, 0.42f, 1f));
            refs.restartButton = CreateButton(refs.gameOverPanel.transform, "Restart Button", "Restart", new Vector2(0.5f, 0.27f), new Vector2(280f, 76f));

            refs.hudPanel.SetActive(false);
            refs.gameOverPanel.SetActive(false);
            refs.pauseOverlay.SetActive(false);
            return refs;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = panel.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor anchor, Vector2 normalizedPosition, Vector2 size, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = BuiltinFont();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(18, fontSize - 18);
            text.resizeTextMaxSize = fontSize;
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = normalizedPosition;
            rect.anchorMax = normalizedPosition;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 normalizedPosition, Vector2 size)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.95f, 0.98f, 1f, 0.88f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.8f, 0.93f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.86f, 0.96f, 1f);
            button.colors = colors;

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = normalizedPosition;
            rect.anchorMax = normalizedPosition;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Text text = CreateText(buttonObject.transform, "Text", label, 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), size, new Color(0.1f, 0.22f, 0.36f, 1f));
            text.raycastTarget = false;
            return button;
        }

        private static void BuildEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static void AssignUI(UIManager ui, UIReferences refs)
        {
            SetField(ui, "startPanel", refs.startPanel);
            SetField(ui, "hudPanel", refs.hudPanel);
            SetField(ui, "gameOverPanel", refs.gameOverPanel);
            SetField(ui, "pauseOverlay", refs.pauseOverlay);
            SetField(ui, "startHighScoreText", refs.startHighScoreText);
            SetField(ui, "scoreText", refs.scoreText);
            SetField(ui, "highScoreText", refs.highScoreText);
            SetField(ui, "comboText", refs.comboText);
            SetField(ui, "missionText", refs.missionText);
            SetField(ui, "speedText", refs.speedText);
            SetField(ui, "pauseButton", refs.pauseButton);
            SetField(ui, "muteButton", refs.muteButton);
            SetField(ui, "muteButtonText", refs.muteButtonText);
            SetField(ui, "finalScoreText", refs.finalScoreText);
            SetField(ui, "finalHighScoreText", refs.finalHighScoreText);
            SetField(ui, "newBestText", refs.newBestText);
            SetField(ui, "summaryText", refs.summaryText);
            SetField(ui, "restartButton", refs.restartButton);
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (sprite == null)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            if (sprite == null)
            {
                throw new FileNotFoundException("Sprite not found at " + path);
            }

            return sprite;
        }

        private static Font BuiltinFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void SetField<T>(Object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
            EditorUtility.SetDirty(target);
        }

        private sealed class UIReferences
        {
            public GameObject startPanel;
            public GameObject hudPanel;
            public GameObject gameOverPanel;
            public GameObject pauseOverlay;
            public Text startHighScoreText;
            public Text scoreText;
            public Text highScoreText;
            public Text comboText;
            public Text missionText;
            public Text speedText;
            public Button pauseButton;
            public Button muteButton;
            public Text muteButtonText;
            public Text finalScoreText;
            public Text finalHighScoreText;
            public Text newBestText;
            public Text summaryText;
            public Button restartButton;
        }
    }
}
