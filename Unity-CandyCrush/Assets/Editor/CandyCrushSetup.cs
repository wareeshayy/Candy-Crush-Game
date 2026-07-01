#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CandyCrush.Core;
using CandyCrush.Data;
using CandyCrush.Systems;
using CandyCrush.UI;

namespace CandyCrush.Editor
{
    public static class CandyCrushSetup
    {
        const string ConfigPath = "Assets/Resources/GameConfig.asset";
        const string PalettePath = "Assets/Resources/CandyPalette.asset";
        const string PrefabPath = "Assets/Prefabs/Candy.prefab";
        const string ScenePath = "Assets/Scenes/MainGame.unity";

        [MenuItem("Candy Crush/Setup Full Project")]
        public static void SetupFullProject()
        {
            EnsureFolders();
            var config = CreateOrLoadConfig();
            var palette = CreateOrLoadPalette();
            var candyPrefab = CreateCandyPrefab(palette);
            CreateMainScene(config, palette, candyPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Candy Crush", "Project setup complete!\nOpen Assets/Scenes/MainGame.unity and press Play.", "OK");
        }

        static void EnsureFolders()
        {
            CreateFolder("Assets/Scripts");
            CreateFolder("Assets/Resources");
            CreateFolder("Assets/Prefabs");
            CreateFolder("Assets/Scenes");
        }

        static void CreateFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static GameConfig CreateOrLoadConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (existing != null)
                return existing;

            var config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        static CandyPalette CreateOrLoadPalette()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CandyPalette>(PalettePath);
            if (existing != null)
                return existing;

            var palette = ScriptableObject.CreateInstance<CandyPalette>();
            palette.candies = new[]
            {
                new CandyVisual { type = CandyType.Red, displayName = "Red", color = new Color(0.95f, 0.2f, 0.2f) },
                new CandyVisual { type = CandyType.Green, displayName = "Green", color = new Color(0.2f, 0.85f, 0.3f) },
                new CandyVisual { type = CandyType.Blue, displayName = "Blue", color = new Color(0.2f, 0.45f, 0.95f) },
                new CandyVisual { type = CandyType.Yellow, displayName = "Yellow", color = new Color(0.95f, 0.85f, 0.15f) },
                new CandyVisual { type = CandyType.Purple, displayName = "Purple", color = new Color(0.75f, 0.25f, 0.85f) },
                new CandyVisual { type = CandyType.Orange, displayName = "Orange", color = new Color(0.95f, 0.5f, 0.1f) },
                new CandyVisual { type = CandyType.Cyan, displayName = "Cyan", color = new Color(0.2f, 0.9f, 0.9f) }
            };

            AssetDatabase.CreateAsset(palette, PalettePath);
            return palette;
        }

        static Candy CreateCandyPrefab(CandyPalette palette)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Candy>(PrefabPath);
            if (existing != null)
                return existing;

            var go = new GameObject("Candy");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.sortingOrder = 1;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            go.AddComponent<Candy>();
            go.layer = LayerMask.NameToLayer("Default");

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Candy>();
        }

        static void CreateMainScene(GameConfig config, CandyPalette palette, Candy candyPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            cam.backgroundColor = new Color(0.12f, 0.1f, 0.18f);
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            var audioGo = new GameObject("AudioManager");
            var music = audioGo.AddComponent<AudioSource>();
            var sfx = audioGo.AddComponent<AudioSource>();
            var audioManager = audioGo.AddComponent<AudioManager>();
            SetPrivateField(audioManager, "musicSource", music);
            SetPrivateField(audioManager, "sfxSource", sfx);

            var boardRoot = new GameObject("BoardRoot").transform;

            var particlesGo = new GameObject("MatchParticles");
            var ps = particlesGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = 0.4f;
            main.startSpeed = 2f;
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            particlesGo.SetActive(false);

            var boardGo = new GameObject("BoardManager");
            var boardManager = boardGo.AddComponent<BoardManager>();
            SetPrivateField(boardManager, "config", config);
            SetPrivateField(boardManager, "palette", palette);
            SetPrivateField(boardManager, "candyPrefab", candyPrefab);
            SetPrivateField(boardManager, "boardRoot", boardRoot);
            SetPrivateField(boardManager, "matchParticles", ps);

            var inputGo = new GameObject("InputHandler");
            var inputHandler = inputGo.AddComponent<InputHandler>();
            SetPrivateField(inputHandler, "mainCamera", cam);
            SetPrivateField(inputHandler, "boardManager", boardManager);
            SetPrivateField(inputHandler, "config", config);
            SetPrivateField(inputHandler, "candyLayer", LayerMask.GetMask("Default"));

            var canvasGo = CreateCanvas();
            var canvas = canvasGo.GetComponent<Canvas>();

            var hud = CreateHUD(canvasGo.transform);
            var gameOver = CreateGameOverPanel(canvasGo.transform);
            var pause = CreatePausePanel(canvasGo.transform);
            var scoreboard = CreateScoreboardPanel(canvasGo.transform);
            var instructions = CreateInstructionsPanel(canvasGo.transform);
            var menu = CreateMainMenu(canvasGo.transform, scoreboard, instructions);

            var gameManagerGo = new GameObject("GameManager");
            var gameManager = gameManagerGo.AddComponent<GameManager>();
            SetPrivateField(gameManager, "config", config);
            SetPrivateField(gameManager, "boardManager", boardManager);
            SetPrivateField(gameManager, "gameHUD", hud);
            SetPrivateField(gameManager, "gameOverPanel", gameOver);
            SetPrivateField(gameManager, "pauseMenuPanel", pause);

            gameManagerGo.AddComponent<PauseInputController>();

            SetPrivateField(gameOver, "mainMenu", menu);
            SetPrivateField(pause, "mainMenu", menu);
            SetPrivateField(menu, "scoreboardPanel", scoreboard);
            SetPrivateField(menu, "instructionsPanel", instructions);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        static GameObject CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        static GameHUD CreateHUD(Transform parent)
        {
            var root = CreatePanel("GameHUD", parent, false);
            var hud = root.AddComponent<GameHUD>();
            SetPrivateField(hud, "root", root);

            SetPrivateField(hud, "playerNameText", CreateText(root.transform, "PlayerName", new Vector2(0, 480), 28));
            SetPrivateField(hud, "scoreText", CreateText(root.transform, "Score", new Vector2(-700, 480), 32));
            SetPrivateField(hud, "targetText", CreateText(root.transform, "Target", new Vector2(-700, 430), 24));
            SetPrivateField(hud, "timerText", CreateText(root.transform, "Timer", new Vector2(700, 480), 32));
            SetPrivateField(hud, "movesText", CreateText(root.transform, "Moves", new Vector2(700, 430), 24));
            SetPrivateField(hud, "comboText", CreateText(root.transform, "Combo", new Vector2(0, 380), 36));
            SetPrivateField(hud, "scorePopupText", CreateText(root.transform, "ScorePopup", new Vector2(0, 300), 48));

            return hud;
        }

        static MainMenuController CreateMainMenu(Transform parent, ScoreboardPanel scoreboard, InstructionsPanel instructions)
        {
            var root = CreatePanel("MainMenu", parent, true);
            var menu = root.AddComponent<MainMenuController>();
            SetPrivateField(menu, "menuRoot", root);

            CreateText(root.transform, "Title", new Vector2(0, 200), 64, "CANDY CRUSH");
            var nameInputGo = new GameObject("NameInput");
            nameInputGo.transform.SetParent(root.transform, false);
            var nameInput = nameInputGo.AddComponent<TMP_InputField>();
            var nameRect = nameInputGo.AddComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(400, 50);
            nameRect.anchoredPosition = new Vector2(0, 80);
            var placeholder = CreateText(nameInputGo.transform, "Placeholder", Vector2.zero, 24, "Enter your name...");
            var text = CreateText(nameInputGo.transform, "Text", Vector2.zero, 24, "");
            nameInput.textComponent = text;
            nameInput.placeholder = placeholder;
            SetPrivateField(menu, "nameInput", nameInput);

            SetPrivateField(menu, "easyButton", CreateButton(root.transform, "EasyButton", new Vector2(0, 0), "Easy Mode (8x8)"));
            SetPrivateField(menu, "hardButton", CreateButton(root.transform, "HardButton", new Vector2(0, -70), "Hard Mode (10x10)"));
            SetPrivateField(menu, "scoresButton", CreateButton(root.transform, "ScoresButton", new Vector2(0, -140), "High Scores"));
            SetPrivateField(menu, "instructionsButton", CreateButton(root.transform, "InstructionsButton", new Vector2(0, -210), "Instructions"));
            SetPrivateField(menu, "quitButton", CreateButton(root.transform, "QuitButton", new Vector2(0, -280), "Quit"));

            return menu;
        }

        static GameOverPanel CreateGameOverPanel(Transform parent)
        {
            var root = CreatePanel("GameOverPanel", parent, false);
            var panel = root.AddComponent<GameOverPanel>();
            SetPrivateField(panel, "root", root);
            SetPrivateField(panel, "titleText", CreateText(root.transform, "Title", new Vector2(0, 120), 48));
            SetPrivateField(panel, "scoreText", CreateText(root.transform, "Score", new Vector2(0, 40), 32));
            SetPrivateField(panel, "detailText", CreateText(root.transform, "Detail", new Vector2(0, -20), 24));
            SetPrivateField(panel, "playAgainButton", CreateButton(root.transform, "PlayAgain", new Vector2(0, -100), "Play Again"));
            SetPrivateField(panel, "menuButton", CreateButton(root.transform, "Menu", new Vector2(0, -170), "Main Menu"));
            return panel;
        }

        static PauseMenuPanel CreatePausePanel(Transform parent)
        {
            var root = CreatePanel("PausePanel", parent, false);
            var panel = root.AddComponent<PauseMenuPanel>();
            SetPrivateField(panel, "root", root);
            CreateText(root.transform, "Title", new Vector2(0, 80), 48, "PAUSED");
            SetPrivateField(panel, "resumeButton", CreateButton(root.transform, "Resume", new Vector2(0, 0), "Resume"));
            SetPrivateField(panel, "menuButton", CreateButton(root.transform, "Menu", new Vector2(0, -70), "Main Menu"));
            return panel;
        }

        static ScoreboardPanel CreateScoreboardPanel(Transform parent)
        {
            var root = CreatePanel("ScoreboardPanel", parent, false);
            var panel = root.AddComponent<ScoreboardPanel>();
            SetPrivateField(panel, "root", root);
            CreateText(root.transform, "Title", new Vector2(0, 200), 40, "HIGH SCORES");
            SetPrivateField(panel, "scoreListText", CreateText(root.transform, "List", new Vector2(0, 0), 22));
            SetPrivateField(panel, "closeButton", CreateButton(root.transform, "Close", new Vector2(0, -220), "Close"));
            return panel;
        }

        static InstructionsPanel CreateInstructionsPanel(Transform parent)
        {
            var root = CreatePanel("InstructionsPanel", parent, false);
            var panel = root.AddComponent<InstructionsPanel>();
            SetPrivateField(panel, "root", root);
            CreateText(root.transform, "Title", new Vector2(0, 240), 40, "INSTRUCTIONS");
            var body = CreateText(root.transform, "Body", new Vector2(0, -20), 18);
            body.alignment = TextAlignmentOptions.TopLeft;
            body.rectTransform.sizeDelta = new Vector2(800, 400);
            SetPrivateField(panel, "bodyText", body);
            SetPrivateField(panel, "closeButton", CreateButton(root.transform, "Close", new Vector2(0, -260), "Close"));
            return panel;
        }

        static GameObject CreatePanel(string name, Transform parent, bool active)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, active ? 0.85f : 0.75f);
            go.SetActive(active);
            return go;
        }

        static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 pos, int fontSize, string content = "")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(600, 80);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            return text;
        }

        static Button CreateButton(Transform parent, string name, Vector2 pos, string label)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(320, 50);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.5f, 0.9f);
            var btn = go.AddComponent<Button>();
            CreateText(go.transform, "Label", Vector2.zero, 22, label);
            return btn;
        }

        static void SetPrivateField(Object target, string fieldName, Object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        static void SetPrivateField<T>(Object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
#endif
