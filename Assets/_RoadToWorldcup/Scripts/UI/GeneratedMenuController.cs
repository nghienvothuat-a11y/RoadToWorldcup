using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoadToWorldcup
{
    public sealed class GeneratedMenuController : MonoBehaviour
    {
        private const string MainMenuReferenceResource = "MainMenuBackground";
        private static readonly Color StadiumNavy = new Color(0.025f, 0.12f, 0.30f, 0.985f);
        private static readonly Color StadiumBlue = new Color(0.035f, 0.31f, 0.76f, 1f);
        private static readonly Color StadiumSky = new Color(0.14f, 0.68f, 1f, 1f);
        private static readonly Color TrophyGold = new Color(1f, 0.76f, 0.12f, 1f);
        private static readonly Color GrassGreen = new Color(0.06f, 0.68f, 0.24f, 1f);

        private Font uiFont;
        private GameObject comingSoonPanel;
        private Text comingSoonText;
        private RectTransform modalRoot;
        private GameObject activeModal;

        private void Awake()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!HasReferenceMenuArt())
            {
                BuildWorld();
            }

            BuildCanvas();
        }

        private void BuildWorld()
        {
            Camera camera = FindSceneObject<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("MainMenu_Camera");
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.transform.position = new Vector3(0f, 5.7f, -11.6f);
            camera.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
            camera.fieldOfView = 38f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.42f, 0.82f);

            RenderSettings.ambientLight = new Color(0.72f, 0.82f, 0.95f);

            GameObject lightObject = new GameObject("MainMenu_Sun");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

            Material grass = MakeMaterial("MenuGrass", new Color(0.14f, 0.68f, 0.22f));
            Material stripe = MakeMaterial("MenuGrassStripe", new Color(0.1f, 0.55f, 0.18f));
            Material blue = MakeMaterial("MenuBlueKit", new Color(0.07f, 0.55f, 1f));
            Material white = MakeMaterial("MenuWhite", Color.white);
            Material gold = MakeMaterial("MenuGold", new Color(1f, 0.78f, 0.18f));
            Material black = MakeMaterial("MenuBlack", new Color(0.03f, 0.025f, 0.02f));
            Material skin = MakeMaterial("MenuSkin", new Color(0.95f, 0.72f, 0.48f));
            Material crowdA = MakeMaterial("MenuCrowdA", new Color(0.95f, 0.28f, 0.3f));
            Material crowdB = MakeMaterial("MenuCrowdB", new Color(0.1f, 0.35f, 0.95f));
            Material board = MakeMaterial("MenuBoard", new Color(0.06f, 0.12f, 0.22f));
            Material confettiBlue = MakeMaterial("MenuConfettiBlue", new Color(0.12f, 0.86f, 1f));
            Material confettiGold = MakeMaterial("MenuConfettiGold", new Color(1f, 0.78f, 0.12f));
            Material confettiWhite = MakeMaterial("MenuConfettiWhite", Color.white);
            Material firework = MakeMaterial("MenuFirework", new Color(0.75f, 0.52f, 1f));

            GameObject field = GameObject.CreatePrimitive(PrimitiveType.Cube);
            field.name = "Menu_Field";
            field.transform.position = new Vector3(0f, -0.08f, 0.2f);
            field.transform.localScale = new Vector3(9f, 0.08f, 13f);
            field.GetComponent<Renderer>().material = grass;

            for (int i = 0; i < 7; i++)
            {
                GameObject stripeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripeObject.name = "Menu_Field_Stripe";
                stripeObject.transform.position = new Vector3(0f, -0.025f, -5.3f + i * 1.8f);
                stripeObject.transform.localScale = new Vector3(9.1f, 0.02f, 0.86f);
                stripeObject.GetComponent<Renderer>().material = stripe;
            }

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject stands = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stands.name = "Menu_Crowd_Block";
                stands.transform.position = new Vector3(side * 5.1f, 0.7f, 0.4f);
                stands.transform.localScale = new Vector3(0.9f, 1.2f, 9.4f);
                stands.GetComponent<Renderer>().material = side < 0 ? crowdA : crowdB;
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject boardObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boardObject.name = "Menu_Stadium_Board";
                boardObject.transform.position = new Vector3(-3.3f + i * 2.2f, 0.25f, 5.6f);
                boardObject.transform.localScale = new Vector3(1.8f, 0.45f, 0.12f);
                boardObject.GetComponent<Renderer>().material = board;
            }

            BuildCelebrationVfx(confettiBlue, confettiGold, confettiWhite, firework);
            BuildMenuHeroPlayer(blue, white, gold, black, skin);
        }

        private static T FindSceneObject<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        private void BuildCelebrationVfx(Material blue, Material gold, Material white, Material firework)
        {
            Material[] confettiMaterials = { blue, gold, white };
            for (int i = 0; i < 42; i++)
            {
                GameObject confetti = GameObject.CreatePrimitive(PrimitiveType.Cube);
                confetti.name = "Menu_Confetti";
                float x = Mathf.Sin(i * 1.73f) * 4.35f;
                float y = 2.3f + Mathf.Abs(Mathf.Sin(i * 0.91f)) * 2.8f;
                float z = 0.3f + Mathf.Cos(i * 1.21f) * 4.7f;
                confetti.transform.position = new Vector3(x, y, z);
                confetti.transform.rotation = Quaternion.Euler(i * 29f, i * 17f, i * 41f);
                confetti.transform.localScale = new Vector3(0.11f, 0.035f, 0.34f);
                confetti.GetComponent<Renderer>().material = confettiMaterials[i % confettiMaterials.Length];
            }

            CreateFireworkBurst(new Vector3(-3.15f, 4.45f, 3.25f), firework);
            CreateFireworkBurst(new Vector3(3.25f, 3.55f, 2.8f), blue);
        }

        private void CreateFireworkBurst(Vector3 center, Material material)
        {
            for (int i = 0; i < 14; i++)
            {
                float angle = (Mathf.PI * 2f * i) / 14f;
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = "Menu_Firework_Spark";
                spark.transform.position = center + new Vector3(Mathf.Cos(angle) * 0.65f, Mathf.Sin(angle) * 0.65f, 0f);
                spark.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                spark.GetComponent<Renderer>().material = material;
            }
        }

        private void BuildMenuHeroPlayer(Material blue, Material white, Material gold, Material black, Material skin)
        {
            GameObject root = new GameObject("Menu_Generic_10_Player");
            root.transform.position = new Vector3(0f, 0f, 0.75f);
            root.transform.localScale = new Vector3(1.42f, 1.42f, 1.42f);

            CreatePart(root.transform, "Body", new Vector3(0f, 0.78f, 0f), new Vector3(0.68f, 0.92f, 0.38f), blue);
            CreatePart(root.transform, "Front_Stripe", new Vector3(0f, 0.78f, -0.205f), new Vector3(0.26f, 0.94f, 0.035f), white);
            CreatePart(root.transform, "Shorts", new Vector3(0f, 0.28f, 0f), new Vector3(0.7f, 0.28f, 0.4f), black);
            CreatePart(root.transform, "Head", new Vector3(0f, 1.42f, 0f), new Vector3(0.48f, 0.48f, 0.48f), skin);
            CreatePart(root.transform, "Hair", new Vector3(0f, 1.72f, -0.02f), new Vector3(0.52f, 0.16f, 0.5f), black);
            CreatePart(root.transform, "Left_Arm", new Vector3(-0.52f, 0.82f, 0f), new Vector3(0.18f, 0.7f, 0.22f), skin);
            CreatePart(root.transform, "Right_Arm_Raised", new Vector3(0.52f, 1.1f, 0f), new Vector3(0.18f, 0.86f, 0.22f), skin);
            CreatePart(root.transform, "Left_Leg", new Vector3(-0.22f, -0.1f, 0f), new Vector3(0.22f, 0.46f, 0.24f), skin);
            CreatePart(root.transform, "Right_Leg", new Vector3(0.22f, -0.1f, 0f), new Vector3(0.22f, 0.46f, 0.24f), skin);
            CreatePart(root.transform, "Left_Boot", new Vector3(-0.22f, -0.38f, -0.03f), new Vector3(0.28f, 0.14f, 0.34f), black);
            CreatePart(root.transform, "Right_Boot", new Vector3(0.22f, -0.38f, -0.03f), new Vector3(0.28f, 0.14f, 0.34f), black);

            GameObject trophy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trophy.name = "Generic_Gold_Cup";
            trophy.transform.SetParent(root.transform, false);
            trophy.transform.localPosition = new Vector3(0f, 1.98f, 0f);
            trophy.transform.localScale = new Vector3(0.3f, 0.28f, 0.3f);
            trophy.GetComponent<Renderer>().material = gold;

            CreatePart(root.transform, "Generic_Cup_Base", new Vector3(0f, 1.5f, 0f), new Vector3(0.42f, 0.08f, 0.42f), gold);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Generic_Cup_Stem";
            stem.transform.SetParent(root.transform, false);
            stem.transform.localPosition = new Vector3(0f, 1.68f, 0f);
            stem.transform.localScale = new Vector3(0.08f, 0.24f, 0.08f);
            stem.GetComponent<Renderer>().material = gold;

            TextMesh number = new GameObject("Jersey_10_Label").AddComponent<TextMesh>();
            number.transform.SetParent(root.transform, false);
            number.transform.localPosition = new Vector3(0f, 0.78f, -0.19f);
            number.transform.localRotation = Quaternion.Euler(18f, 180f, 0f);
            number.text = "10";
            number.font = uiFont;
            number.fontSize = 72;
            number.characterSize = 0.025f;
            number.anchor = TextAnchor.MiddleCenter;
            number.alignment = TextAlignment.Center;
            number.color = Color.white;
        }

        private static GameObject CreatePart(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().material = material;
            return part;
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("Generated_MainMenu_Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            modalRoot = root;
            RectTransform safeRoot = CreateSafeAreaRoot(root);

            if (BuildReferenceCanvas(root))
            {
                BuildComingSoon(root);
                return;
            }

            GameObject titlePlate = new GameObject("Title_Plate");
            titlePlate.transform.SetParent(safeRoot, false);
            Image titlePlateImage = titlePlate.AddComponent<Image>();
            titlePlateImage.color = new Color(0.02f, 0.03f, 0.04f, 0.72f);
            SetAnchor(titlePlate.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -194f), new Vector2(780f, 210f));

            Text title = CreateText(safeRoot, "WORLDCUP", 96, FontStyle.Bold, new Color(1f, 0.86f, 0.16f), TextAnchor.MiddleCenter);
            title.name = "Title";
            SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), new Vector2(900f, 132f));
            title.AddShadow();

            Text titleTop = CreateText(safeRoot, "ROAD TO", 56, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            titleTop.name = "Title_Top_Highlight";
            SetAnchor(titleTop.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -124f), new Vector2(620f, 82f));
            titleTop.AddShadow();

            string[] utilityLabels = { "DAILY\nREWARD", "SPIN", "SHOP", "SETTINGS" };
            for (int i = 0; i < utilityLabels.Length; i++)
            {
                Button button = CreateButton(safeRoot, utilityLabels[i], new Color(0.05f, 0.14f, 0.24f, 0.88f), Color.white, 26);
                SetAnchor(button.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(112f, 210f - i * 104f), new Vector2(184f, 92f));
                int buttonIndex = i;
                button.onClick.AddListener(delegate { OpenUtility(buttonIndex); });
            }

            Button play = CreateButton(safeRoot, "PLAY", new Color(0.08f, 0.75f, 0.22f), Color.white, 54);
            SetAnchor(play.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 420f), new Vector2(650f, 128f));
            play.onClick.AddListener(delegate
            {
                SceneLoader.LoadGameplay();
            });

            CreateMenuButton(safeRoot, "RANKING", new Color(0.04f, 0.32f, 0.92f), 262f);
            CreateMenuButton(safeRoot, "CUSTOMIZE", new Color(1f, 0.63f, 0.12f), 166f);
            CreateMenuButton(safeRoot, "MISSIONS", new Color(0.48f, 0.2f, 0.88f), 70f);

            BuildComingSoon(root);
        }

        private static RectTransform CreateSafeAreaRoot(RectTransform root)
        {
            GameObject safeObject = new GameObject("MainMenu_SafeAreaRoot");
            safeObject.transform.SetParent(root, false);
            RectTransform safeRect = safeObject.AddComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.offsetMin = Vector2.zero;
            safeRect.offsetMax = Vector2.zero;
            safeObject.AddComponent<SafeAreaFitter>();
            return safeRect;
        }

        private static bool HasReferenceMenuArt()
        {
            return Resources.Load<Texture2D>(MainMenuReferenceResource) != null;
        }

        private bool BuildReferenceCanvas(RectTransform root)
        {
            Texture2D texture = Resources.Load<Texture2D>(MainMenuReferenceResource);
            if (texture == null)
            {
                return false;
            }

            GameObject referenceLayer = new GameObject("MainMenu_Reference_ArtLayer");
            referenceLayer.transform.SetParent(root, false);
            RectTransform layerRect = referenceLayer.AddComponent<RectTransform>();
            layerRect.anchorMin = new Vector2(0.5f, 0.5f);
            layerRect.anchorMax = new Vector2(0.5f, 0.5f);
            layerRect.pivot = new Vector2(0.5f, 0.5f);
            layerRect.anchoredPosition = Vector2.zero;
            MainMenuAspectFill layerAspectFill = referenceLayer.AddComponent<MainMenuAspectFill>();
            layerAspectFill.aspectRatio = (float)texture.width / texture.height;

            GameObject backgroundObject = new GameObject("MainMenu_Reference_Background");
            backgroundObject.transform.SetParent(referenceLayer.transform, false);
            backgroundObject.transform.SetAsFirstSibling();
            Image background = backgroundObject.AddComponent<Image>();
            background.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            background.preserveAspect = false;
            background.raycastTarget = false;
            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = Vector2.zero;

            CreateLayeredMenuUi(layerRect);

            return true;
        }

        private void CreateLayeredMenuUi(RectTransform root)
        {
            CreateSpriteImage(root, "Logo", "MainMenuUI/logo_roadtoworldcup", 540f, 240f, 940f, 430f, false);

            CreateSpriteButton(root, "DailyReward", "MainMenuUI/button_daily", "MainMenuUI/button_daily_pressed", 112f, 842f, 166f, 182f, ShowDailyReward, false);
            CreateSpriteButton(root, "Spin", "MainMenuUI/button_spin", "MainMenuUI/button_spin_pressed", 112f, 1026f, 166f, 182f, ShowSpin, false);
            CreateSpriteButton(root, "Shop", "MainMenuUI/button_shop", "MainMenuUI/button_shop_pressed", 112f, 1210f, 166f, 182f, ShowShop, false);
            CreateSpriteButton(root, "Settings", "MainMenuUI/button_settings", "MainMenuUI/button_settings_pressed", 112f, 1394f, 166f, 182f, ShowSettings, false);

            CreateSpriteButton(root, "Play", "MainMenuUI/button_play", "MainMenuUI/button_play_pressed", 540f, 1414f, 560f, 136f, delegate { SceneLoader.LoadGameplay(); });
            CreateSpriteButton(root, "Tournament", "MainMenuUI/button_tournament", "MainMenuUI/button_tournament_pressed", 540f, 1522f, 540f, 116f, ShowRanking);
            CreateReferenceTag(root, "RANKING", 540f, 1522f, new Color(0.9f, 0.97f, 1f));
            CreateSpriteButton(root, "Customize", "MainMenuUI/button_customize", "MainMenuUI/button_customize_pressed", 540f, 1626f, 540f, 116f, delegate { ShowCustomize(CosmeticCategory.Hair); });
            CreateSpriteButton(root, "Missions", "MainMenuUI/button_missions", "MainMenuUI/button_missions_pressed", 540f, 1730f, 540f, 116f, ShowMissions);
        }

        private Image CreateSpriteImage(RectTransform root, string name, string resourceName, float referenceX, float referenceY, float width, float height, bool preserveAspect = true)
        {
            Sprite sprite = LoadMenuSprite(resourceName);
            GameObject imageObject = new GameObject(name + "_Image");
            imageObject.transform.SetParent(root, false);
            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;
            SetReferenceRect(imageObject.GetComponent<RectTransform>(), referenceX, referenceY, width, height);
            return image;
        }

        private Button CreateSpriteButton(RectTransform root, string name, string normalResource, string pressedResource, float referenceX, float referenceY, float width, float height, UnityEngine.Events.UnityAction onClick, bool preserveAspect = true)
        {
            Sprite normalSprite = LoadMenuSprite(normalResource);
            Sprite pressedSprite = LoadMenuSprite(pressedResource);

            GameObject buttonObject = new GameObject(name + "_SpriteButton");
            buttonObject.transform.SetParent(root, false);
            Image image = buttonObject.AddComponent<Image>();
            image.sprite = normalSprite;
            image.preserveAspect = preserveAspect;
            image.color = Color.white;

            Button button = buttonObject.AddComponent<Button>();
            buttonObject.AddComponent<UiClickSound>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            SpriteState spriteState = button.spriteState;
            spriteState.highlightedSprite = normalSprite;
            spriteState.pressedSprite = pressedSprite;
            spriteState.selectedSprite = normalSprite;
            button.spriteState = spriteState;
            button.onClick.AddListener(onClick);

            SetReferenceRect(buttonObject.GetComponent<RectTransform>(), referenceX, referenceY, width, height);
            buttonObject.AddComponent<MenuButtonPressFx>();
            return button;
        }

        private static Sprite LoadMenuSprite(string resourceName)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourceName);
            if (texture == null)
            {
                Debug.LogWarning("Road To Worldcup could not load menu sprite: " + resourceName);
                return null;
            }

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private void CreateReferenceTag(RectTransform root, string value, float referenceX, float referenceY, Color color)
        {
            GameObject plateObject = new GameObject(value + "_TagPlate");
            plateObject.transform.SetParent(root, false);
            Image plate = plateObject.AddComponent<Image>();
            plate.color = new Color(0.04f, 0.24f, 0.72f, 0.96f);
            SetReferenceRect(plateObject.GetComponent<RectTransform>(), referenceX, referenceY, 350f, 58f);
            Text tag = CreateText(root, value, 28, FontStyle.Bold, color, TextAnchor.MiddleCenter);
            tag.AddShadow();
            SetReferenceRect(tag.rectTransform, referenceX, referenceY, 300f, 64f);
        }

        private Button CreateCurrencyButton(RectTransform root, string name, string value, Color accent, float referenceX, float referenceY, UnityEngine.Events.UnityAction onClick)
        {
            Button button = CreateLayeredButton(root, name, name.ToUpperInvariant(), value + "   +", new Color(0.04f, 0.1f, 0.18f, 0.92f), referenceX, referenceY, 296f, 70f, 30, onClick);
            Text[] texts = button.GetComponentsInChildren<Text>();
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].color = i == 0 ? accent : Color.white;
            }

            return button;
        }

        private Button CreateUtilityButton(RectTransform root, string name, string icon, string label, float referenceX, float referenceY, UnityEngine.Events.UnityAction onClick)
        {
            return CreateLayeredButton(root, name, icon, label, new Color(0.03f, 0.08f, 0.14f, 0.9f), referenceX, referenceY, 178f, 96f, 24, onClick);
        }

        private Button CreateLayeredButton(RectTransform root, string name, string icon, string label, Color background, float referenceX, float referenceY, float width, float height, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            GameObject shadowObject = new GameObject(name + "_Shadow");
            shadowObject.transform.SetParent(root, false);
            Image shadow = shadowObject.AddComponent<Image>();
            shadow.color = new Color(0f, 0f, 0f, 0.46f);
            SetReferenceRect(shadowObject.GetComponent<RectTransform>(), referenceX, referenceY + 8f, width, height);

            GameObject buttonObject = new GameObject(name + "_Button");
            buttonObject.transform.SetParent(root, false);

            Image image = buttonObject.AddComponent<Image>();
            image.color = background;

            Button button = buttonObject.AddComponent<Button>();
            buttonObject.AddComponent<UiClickSound>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.24f);
            colors.selectedColor = colors.highlightedColor;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.06f;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            SetReferenceRect(buttonObject.GetComponent<RectTransform>(), referenceX, referenceY, width, height);
            MenuButtonPressFx pressFx = buttonObject.AddComponent<MenuButtonPressFx>();
            pressFx.shadow = shadowObject.transform;

            Image shine = new GameObject("Shine").AddComponent<Image>();
            shine.transform.SetParent(buttonObject.transform, false);
            shine.color = new Color(1f, 1f, 1f, 0.13f);
            RectTransform shineRect = shine.GetComponent<RectTransform>();
            shineRect.anchorMin = new Vector2(0.5f, 1f);
            shineRect.anchorMax = new Vector2(0.5f, 1f);
            shineRect.pivot = new Vector2(0.5f, 1f);
            shineRect.anchoredPosition = new Vector2(0f, -5f);
            shineRect.sizeDelta = new Vector2(width - 20f, Mathf.Max(16f, height * 0.28f));
            shine.raycastTarget = false;

            Text iconText = CreateText(buttonObject.transform, icon, Mathf.Max(18, Mathf.RoundToInt(fontSize * 0.58f)), FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            RectTransform iconRect = iconText.rectTransform;
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(Mathf.Min(92f, width * 0.18f), 0f);
            iconRect.sizeDelta = new Vector2(Mathf.Min(150f, width * 0.28f), height * 0.8f);
            iconText.AddShadow();

            Text labelText = CreateText(buttonObject.transform, label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(Mathf.Min(110f, width * 0.26f), 0f);
            labelRect.offsetMax = new Vector2(-18f, 0f);
            labelText.AddShadow();
            return button;
        }

        private static void SetReferenceRect(RectTransform rect, float referenceX, float referenceY, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(referenceX - 540f, 960f - referenceY);
            rect.sizeDelta = new Vector2(width, height);
        }

        private void CreateMenuButton(RectTransform root, string label, Color color, float y)
        {
            Button button = CreateButton(root, label, color, Color.white, 38);
            SetAnchor(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(560f, 88f));
            if (label == "RANKING") button.onClick.AddListener(ShowRanking);
            else if (label == "CUSTOMIZE") button.onClick.AddListener(delegate { ShowCustomize(CosmeticCategory.Hair); });
            else button.onClick.AddListener(ShowMissions);
        }

        private void OpenUtility(int index)
        {
            if (index == 0) ShowDailyReward();
            else if (index == 1) ShowSpin();
            else if (index == 2) ShowShop();
            else ShowSettings();
        }

        private RectTransform OpenModal(string title, Vector2 size)
        {
            if (activeModal != null) Destroy(activeModal);
            activeModal = new GameObject(title + "_Modal");
            activeModal.transform.SetParent(modalRoot, false);
            activeModal.transform.SetAsLastSibling();
            Image scrim = activeModal.AddComponent<Image>();
            scrim.color = new Color(0.01f, 0.04f, 0.10f, 0.58f);
            SetAnchor(activeModal.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform card = CreateModalCard(activeModal.transform, title + "_Card", size);
            Text titleText = CreateText(card, title, 44, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetAnchor(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -64f), new Vector2(size.x - 160f, 74f));
            Button close = CreateButton(card, "X", new Color(1f, 1f, 1f, 0.12f), Color.white, 28);
            SetAnchor(close.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-50f, -52f), new Vector2(56f, 56f));
            close.onClick.AddListener(CloseModal);
            return card;
        }

        private RectTransform CreateModalCard(Transform parent, string name, Vector2 size)
        {
            GameObject cardObject = new GameObject(name);
            cardObject.transform.SetParent(parent, false);
            Image image = cardObject.AddComponent<Image>();
            image.color = new Color(0.025f, 0.10f, 0.21f, 0.94f);
            Outline outline = cardObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.78f, 1f, 0.46f);
            outline.effectDistance = new Vector2(1f, -1f);
            RectTransform card = cardObject.GetComponent<RectTransform>();
            SetAnchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);

            return card;
        }

        private RectTransform CreateFeatureCard(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);
            Image image = card.AddComponent<Image>();
            image.color = new Color(color.r, color.g, color.b, 0.68f);
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.16f);
            outline.effectDistance = new Vector2(1f, -1f);
            SetAnchor(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
            return card.GetComponent<RectTransform>();
        }

        private void CloseModal()
        {
            if (activeModal != null) Destroy(activeModal);
            activeModal = null;
        }

        private void ShowDailyReward()
        {
            RectTransform card = OpenModal("DAILY REWARD", new Vector2(860f, 1160f));
            Text intro = CreateText(card, GameSession.CanClaimDailyReward ? "Your next stadium bonus is ready!" : "Next reward available tomorrow.", 29, FontStyle.Bold, new Color(0.82f, 0.94f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(intro.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(740f, 60f));

            for (int i = 0; i < 7; i++)
            {
                int column = i % 2;
                int row = i / 2;
                RectTransform rewardCard = CreateFeatureCard(card, "Day_" + (i + 1), new Vector2(column == 0 ? -195f : 195f, 220f - row * 155f), new Vector2(350f, 126f), i == GameSession.DailyRewardDay ? new Color(0.16f, 0.45f, 0.25f, 1f) : new Color(0.07f, 0.15f, 0.24f, 1f));
                Reward reward = GetDailyReward(i);
                Text day = CreateText(rewardCard, "DAY " + (i + 1), 20, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
                SetAnchor(day.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(280f, 40f));
                Text amount = CreateText(rewardCard, reward.ToDisplayString().Replace("+", ""), 23, FontStyle.Bold, new Color(1f, 0.84f, 0.2f), TextAnchor.MiddleCenter);
                SetAnchor(amount.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(300f, 42f));
            }

            Button claim = CreateButton(card, GameSession.CanClaimDailyReward ? "CLAIM " + GameSession.GetDailyRewardPreview().ToDisplayString() : "CLAIMED TODAY", GameSession.CanClaimDailyReward ? new Color(0.08f, 0.74f, 0.2f) : new Color(0.18f, 0.24f, 0.3f), Color.white, 27);
            SetAnchor(claim.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 84f), new Vector2(690f, 92f));
            claim.interactable = GameSession.CanClaimDailyReward;
            claim.onClick.AddListener(delegate { GameSession.ClaimDailyReward(); ShowDailyReward(); });
        }

        private static Reward GetDailyReward(int index)
        {
            // The daily preview advances after every claim; use the public preview only for the current day.
            int[] gold = { 250, 350, 450, 600, 700, 900, 1200 };
            int[] gems = { 0, 0, 3, 0, 5, 0, 12 };
            return new Reward(gold[index], gems[index], "");
        }

        private void ShowSpin()
        {
            RectTransform card = OpenModal("DAILY SPIN", new Vector2(860f, 1160f));
            Text hint = CreateText(card, GameSession.CanSpinToday ? "One free spin every day" : "Your free spin has been used. Rewarded ad spins arrive later.", 27, FontStyle.Bold, new Color(0.82f, 0.94f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(740f, 70f));
            RectTransform wheel = CreateFeatureCard(card, "Lucky_Wheel", new Vector2(0f, 80f), new Vector2(560f, 560f), new Color(0.12f, 0.29f, 0.58f, 1f));
            Outline wheelOutline = wheel.gameObject.AddComponent<Outline>();
            wheelOutline.effectColor = new Color(1f, 0.78f, 0.12f, 0.95f);
            wheelOutline.effectDistance = new Vector2(10f, -10f);
            Text sectors = CreateText(wheel, "150 GOLD\n\n3 GEMS     JACKPOT\n\n500 GOLD", 30, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetAnchor(sectors.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Text pointer = CreateText(card, "V", 48, FontStyle.Bold, new Color(1f, 0.82f, 0.16f), TextAnchor.MiddleCenter);
            SetAnchor(pointer.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 395f), new Vector2(90f, 70f));
            Text status = CreateText(card, GameSession.CanSpinToday ? "Tap SPIN to turn the wheel" : "Come back tomorrow for a free spin", 27, FontStyle.Bold, new Color(1f, 0.86f, 0.18f), TextAnchor.MiddleCenter);
            SetAnchor(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 182f), new Vector2(740f, 56f));
            Button spin = CreateButton(card, GameSession.CanSpinToday ? "SPIN" : "SPIN USED", GameSession.CanSpinToday ? new Color(0.08f, 0.74f, 0.2f) : new Color(0.18f, 0.24f, 0.3f), Color.white, 36);
            SetAnchor(spin.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 84f), new Vector2(500f, 92f));
            spin.interactable = GameSession.CanSpinToday;
            spin.onClick.AddListener(delegate { StartCoroutine(SpinRoutine(wheel, spin, status)); });
        }

        private IEnumerator SpinRoutine(RectTransform wheel, Button spin, Text status)
        {
            spin.interactable = false;
            float elapsed = 0f;
            while (elapsed < 1.1f)
            {
                elapsed += Time.unscaledDeltaTime;
                wheel.Rotate(0f, 0f, -1150f * (1f - elapsed / 1.3f) * Time.unscaledDeltaTime);
                yield return null;
            }
            Reward reward = GameSession.Spin();
            status.text = "YOU WON " + reward.ToDisplayString();
            spin.GetComponentInChildren<Text>().text = "SPIN USED";
        }

        private void ShowShop()
        {
            RectTransform card = OpenModal("SHOP", new Vector2(860f, 1120f));
            Text wallet = CreateText(card, "Wallet: " + GameSession.Coins + " GOLD  |  " + GameSession.Gems + " GEMS", 28, FontStyle.Bold, new Color(0.82f, 0.94f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(wallet.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(740f, 54f));
            Text note = CreateText(card, "IAP packs are staged for the store integration.", 24, FontStyle.Bold, new Color(1f, 0.82f, 0.2f), TextAnchor.MiddleCenter);
            SetAnchor(note.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -195f), new Vector2(740f, 50f));
            string[] packs = { "STARTER KICK\n1,200 GOLD  |  $0.99", "STRIKER CHEST\n4,000 GOLD + 30 GEMS  |  $2.99", "LEGEND VAULT\n12,000 GOLD + 120 GEMS  |  $7.99" };
            for (int i = 0; i < packs.Length; i++)
            {
                RectTransform pack = CreateFeatureCard(card, "IAP_Pack_" + i, new Vector2(0f, 180f - i * 220f), new Vector2(700f, 180f), new Color(0.07f, 0.18f + i * 0.04f, 0.34f, 1f));
                Text packText = CreateText(pack, packs[i], 28, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
                SetAnchor(packText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
            Text spending = CreateText(card, "LIFETIME SPENT: " + GameSession.TotalCoinsSpent + " GOLD  |  " + GameSession.TotalGemsSpent + " GEMS", 24, FontStyle.Bold, new Color(0.65f, 0.82f, 0.95f), TextAnchor.MiddleCenter);
            SetAnchor(spending.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 72f), new Vector2(720f, 52f));
        }

        private void ShowSettings()
        {
            RectTransform card = OpenModal("SETTINGS", new Vector2(860f, 920f));
            CreateSettingsRow(card, "LANGUAGE", GameSession.Language, 180f, delegate
            {
                GameSession.SetLanguage(GameSession.Language == "Tiếng Việt" ? "English" : "Tiếng Việt");
                ShowSettings();
            });
            CreateSettingsRow(card, "BACKGROUND MUSIC", GameSession.MusicEnabled ? "ON" : "OFF", 40f, delegate { GameSession.SetMusicEnabled(!GameSession.MusicEnabled); ShowSettings(); });
            CreateSettingsRow(card, "SFX", GameSession.SfxEnabled ? "ON" : "OFF", -100f, delegate { GameSession.SetSfxEnabled(!GameSession.SfxEnabled); ShowSettings(); });
            Text footer = CreateText(card, "Audio toggles are saved on this device.", 23, FontStyle.Bold, new Color(0.64f, 0.8f, 0.94f), TextAnchor.MiddleCenter);
            SetAnchor(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 76f), new Vector2(700f, 48f));
        }

        private void CreateSettingsRow(RectTransform parent, string label, string value, float y, UnityEngine.Events.UnityAction action)
        {
            RectTransform row = CreateFeatureCard(parent, label, new Vector2(0f, y), new Vector2(710f, 110f), new Color(0.07f, 0.15f, 0.24f, 1f));
            Text name = CreateText(row, label, 25, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            SetAnchor(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(225f, 0f), new Vector2(390f, 60f));
            Button valueButton = CreateButton(row, value, new Color(0.08f, 0.52f, 0.72f), Color.white, 24);
            SetAnchor(valueButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-105f, 0f), new Vector2(180f, 66f));
            valueButton.onClick.AddListener(action);
        }

        private void ShowRanking()
        {
            RectTransform card = OpenModal("RANKING", new Vector2(860f, 1080f));
            Text subtitle = CreateText(card, "Game Center leaderboard", 30, FontStyle.Bold, new Color(0.82f, 0.94f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(720f, 55f));
            string[] names = { "1   GOLDEN BOOT     30", "2   BLUE LIGHTNING  27", "3   GOAL HUNTER     24", "4   YOU             " + GameSession.SelectedLevelNumber + "", "5   ROAD RUNNER     16" };
            for (int i = 0; i < names.Length; i++)
            {
                bool player = i == 3;
                RectTransform row = CreateFeatureCard(card, "Rank_" + i, new Vector2(0f, 210f - i * 135f), new Vector2(700f, 102f), player ? new Color(0.11f, 0.5f, 0.3f, 1f) : new Color(0.06f, 0.14f, 0.23f, 1f));
                Text line = CreateText(row, names[i], 27, FontStyle.Bold, player ? new Color(1f, 0.86f, 0.18f) : Color.white, TextAnchor.MiddleCenter);
                SetAnchor(line.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
            Text status = CreateText(card, "Game Center connection is enabled when the iOS service is integrated. This offline preview keeps your current level visible.", 23, FontStyle.Bold, new Color(0.63f, 0.8f, 0.94f), TextAnchor.MiddleCenter);
            SetAnchor(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(730f, 92f));
        }

        private void ShowCustomize(CosmeticCategory category)
        {
            RectTransform card = OpenModal("CUSTOMIZE #10", new Vector2(920f, 1500f));
            Text preview = CreateText(card, "#10", 108, FontStyle.Bold, GameSession.GetEquipped(CosmeticCategory.Jersey).color, TextAnchor.MiddleCenter);
            SetAnchor(preview.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(300f, 140f));
            Text equipped = CreateText(card, "EQUIPPED: " + GameSession.GetEquipped(CosmeticCategory.Hair).displayName + "  |  " + GameSession.GetEquipped(CosmeticCategory.Jersey).displayName + "  |  " + GameSession.GetEquipped(CosmeticCategory.Accessory).displayName, 21, FontStyle.Bold, new Color(0.8f, 0.93f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(equipped.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -265f), new Vector2(800f, 65f));
            CreateCustomizeTab(card, "HAIR", CosmeticCategory.Hair, -265f, category);
            CreateCustomizeTab(card, "JERSEY", CosmeticCategory.Jersey, 0f, category);
            CreateCustomizeTab(card, "ACCESSORY", CosmeticCategory.Accessory, 265f, category);

            IList<CosmeticItem> items = GameSession.GetCosmetics(category);
            for (int i = 0; i < items.Count; i++)
            {
                CosmeticItem item = items[i];
                RectTransform row = CreateFeatureCard(card, "Cosmetic_" + item.id, new Vector2(0f, 250f - i * 250f), new Vector2(800f, 205f), GameSession.IsEquipped(item) ? new Color(0.12f, 0.43f, 0.27f, 1f) : new Color(0.06f, 0.14f, 0.23f, 1f));
                Text icon = CreateText(row, item.icon, item.icon.Length > 2 ? 22 : 54, FontStyle.Bold, item.color, TextAnchor.MiddleCenter);
                SetAnchor(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(110f, 120f));
                Text name = CreateText(row, item.displayName + "\n" + (GameSession.IsOwned(item) ? (GameSession.IsEquipped(item) ? "EQUIPPED" : "OWNED") : item.price + " " + (item.currency == WalletCurrency.Coins ? "GOLD" : "GEMS")), 27, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
                SetAnchor(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(345f, 0f), new Vector2(360f, 120f));
                Button action = CreateButton(row, GameSession.IsEquipped(item) ? "ON" : (GameSession.IsOwned(item) ? "EQUIP" : "BUY"), GameSession.IsEquipped(item) ? new Color(0.18f, 0.25f, 0.3f) : new Color(0.08f, 0.6f, 0.28f), Color.white, 23);
                SetAnchor(action.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-104f, 0f), new Vector2(170f, 74f));
                action.interactable = !GameSession.IsEquipped(item);
                action.onClick.AddListener(delegate { string ignored; GameSession.PurchaseOrEquip(item, out ignored); ShowCustomize(category); });
            }
            Text spend = CreateText(card, "SPENT: " + GameSession.TotalCoinsSpent + " GOLD  |  " + GameSession.TotalGemsSpent + " GEMS", 22, FontStyle.Bold, new Color(0.65f, 0.83f, 0.96f), TextAnchor.MiddleCenter);
            SetAnchor(spend.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(700f, 45f));
        }

        private void CreateCustomizeTab(RectTransform parent, string label, CosmeticCategory tab, float x, CosmeticCategory active)
        {
            Button button = CreateButton(parent, label, tab == active ? new Color(0.08f, 0.58f, 0.76f) : new Color(0.13f, 0.2f, 0.29f), Color.white, 20);
            SetAnchor(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, -345f), new Vector2(240f, 66f));
            button.onClick.AddListener(delegate { ShowCustomize(tab); });
        }

        private void ShowMissions()
        {
            RectTransform card = OpenModal("DAILY MISSIONS", new Vector2(900f, 1240f));
            Text refresh = CreateText(card, "RESETS AT MIDNIGHT  |  EARN EXTRA GOLD", 25, FontStyle.Bold, new Color(0.82f, 0.94f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(refresh.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(760f, 56f));
            string[] labels = { "WIN 1 LEVEL", "WIN 3 LEVELS", "SPEND 500 GOLD" };
            for (int i = 0; i < 3; i++)
            {
                int mission = i;
                int progress = GameSession.GetMissionProgress(mission);
                int target = GameSession.GetMissionTarget(mission);
                RectTransform row = CreateFeatureCard(card, "Mission_" + mission, new Vector2(0f, 220f - mission * 250f), new Vector2(760f, 205f), new Color(0.06f, 0.15f, 0.25f, 1f));
                Text label = CreateText(row, labels[mission] + "\n" + progress + " / " + target + "     REWARD: " + GameSession.GetMissionReward(mission) + " GOLD", 27, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
                SetAnchor(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(265f, 0f), new Vector2(455f, 120f));
                bool canClaim = GameSession.CanClaimMission(mission);
                bool claimed = GameSession.IsMissionClaimed(mission);
                Button claim = CreateButton(row, claimed ? "CLAIMED" : canClaim ? "CLAIM" : "IN PROGRESS", canClaim ? new Color(0.08f, 0.72f, 0.22f) : new Color(0.18f, 0.24f, 0.31f), Color.white, 20);
                SetAnchor(claim.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-115f, 0f), new Vector2(190f, 72f));
                claim.interactable = canClaim;
                claim.onClick.AddListener(delegate { GameSession.ClaimMission(mission); ShowMissions(); });
            }
            Text footer = CreateText(card, "Today you have spent " + GameSession.DailyCoinsSpent + " / 500 gold.", 24, FontStyle.Bold, new Color(0.65f, 0.83f, 0.96f), TextAnchor.MiddleCenter);
            SetAnchor(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(700f, 48f));
        }

        private void BuildComingSoon(RectTransform root)
        {
            comingSoonPanel = new GameObject("ComingSoon_ModalOverlay");
            comingSoonPanel.transform.SetParent(root, false);
            comingSoonPanel.transform.SetAsLastSibling();
            Image scrim = comingSoonPanel.AddComponent<Image>();
            scrim.color = new Color(0f, 0f, 0f, 0.54f);
            scrim.raycastTarget = true;
            SetAnchor(comingSoonPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject card = new GameObject("ComingSoon_Card");
            card.transform.SetParent(comingSoonPanel.transform, false);
            Image image = card.AddComponent<Image>();
            image.color = new Color(0.02f, 0.06f, 0.12f, 0.95f);
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.32f, 0.88f, 1f, 0.45f);
            outline.effectDistance = new Vector2(4f, -4f);
            Shadow shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(0f, -10f);
            SetAnchor(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 420f));

            Image shine = new GameObject("Top_Shine").AddComponent<Image>();
            shine.transform.SetParent(card.transform, false);
            shine.color = new Color(1f, 1f, 1f, 0.12f);
            RectTransform shineRect = shine.GetComponent<RectTransform>();
            shineRect.anchorMin = new Vector2(0f, 1f);
            shineRect.anchorMax = new Vector2(1f, 1f);
            shineRect.pivot = new Vector2(0.5f, 1f);
            shineRect.anchoredPosition = new Vector2(0f, -8f);
            shineRect.sizeDelta = new Vector2(-32f, 76f);
            shine.raycastTarget = false;

            comingSoonText = CreateText(card.GetComponent<RectTransform>(), "COMING SOON", 52, FontStyle.Bold, new Color(1f, 0.88f, 0.22f), TextAnchor.MiddleCenter);
            SetAnchor(comingSoonText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 76f), new Vector2(620f, 120f));
            comingSoonText.AddShadow();

            Text body = CreateText(card.GetComponent<RectTransform>(), "This feature unlocks later.", 32, FontStyle.Bold, new Color(0.82f, 0.94f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(620f, 80f));
            body.AddShadow();

            Button close = CreateButton(card.GetComponent<RectTransform>(), "OK", new Color(0.08f, 0.75f, 0.22f), Color.white, 34);
            SetAnchor(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(280f, 88f));
            close.onClick.AddListener(delegate { comingSoonPanel.SetActive(false); });
            comingSoonPanel.SetActive(false);
        }

        private void ShowComingSoon(string featureName)
        {
            if (comingSoonPanel == null)
            {
                return;
            }

            comingSoonText.text = featureName + "\nCOMING SOON";
            comingSoonPanel.SetActive(true);
        }

        private Button CreateButton(Transform parent, string label, Color background, Color textColor, int fontSize)
        {
            GameObject buttonObject = new GameObject(label + "_Button");
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.AddComponent<Image>();
            image.color = background;

            Button button = buttonObject.AddComponent<Button>();
            buttonObject.AddComponent<UiClickSound>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.14f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.12f);
            colors.selectedColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            Text text = CreateText(buttonObject.GetComponent<RectTransform>(), label, fontSize, FontStyle.Bold, textColor, TextAnchor.MiddleCenter);
            SetAnchor(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private Text CreateText(Transform parent, string value, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = uiFont;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void SetAnchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            return RuntimeMaterialLibrary.Create(name, color);
        }
    }

    internal sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void LateUpdate()
        {
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            if (rectTransform == null)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            float screenWidth = Mathf.Max(1f, Screen.width);
            float screenHeight = Mathf.Max(1f, Screen.height);

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= screenWidth;
            anchorMin.y /= screenHeight;
            anchorMax.x /= screenWidth;
            anchorMax.y /= screenHeight;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }

    internal sealed class MainMenuAspectFill : MonoBehaviour
    {
        public float aspectRatio = 9f / 16f;
        public float referenceWidth = 1080f;

        private RectTransform rectTransform;
        private RectTransform parentTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentTransform = transform.parent as RectTransform;
            Resize();
        }

        private void LateUpdate()
        {
            Resize();
        }

        private void Resize()
        {
            if (rectTransform == null || parentTransform == null)
            {
                return;
            }

            Rect parentRect = parentTransform.rect;
            float parentWidth = Mathf.Max(1f, parentRect.width);
            float parentHeight = Mathf.Max(1f, parentRect.height);
            float referenceHeight = referenceWidth / Mathf.Max(0.01f, aspectRatio);
            float scale = Mathf.Max(parentWidth / referenceWidth, parentHeight / referenceHeight);

            rectTransform.sizeDelta = new Vector2(referenceWidth, referenceHeight);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    internal sealed class MenuButtonPressFx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Transform shadow;

        private Vector3 baseScale;
        private Vector3 shadowBaseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
            if (shadow != null)
            {
                shadowBaseScale = shadow.localScale;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.localScale = baseScale * 0.96f;
            if (shadow != null)
            {
                shadow.localScale = shadowBaseScale * 0.94f;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetScale();
        }

        private void ResetScale()
        {
            transform.localScale = baseScale;
            if (shadow != null)
            {
                shadow.localScale = shadowBaseScale;
            }
        }
    }

    internal static class TextShadowExtension
    {
        public static void AddShadow(this Text text)
        {
            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(3f, -3f);
        }
    }
}
