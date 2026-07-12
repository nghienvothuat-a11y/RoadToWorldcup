using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoadToWorldcup
{
    public sealed class RoadToWorldcupGame : MonoBehaviour
    {
        private const float PitchScale = 2f;
        private const float GameplayPositionScale = 1.45f;
        private const float OriginalPitchWidth = 8.2f;
        private const float FirstPlayerScreenYFromBottom = 0.34f;
        private const float AimChargeSpeedMultiplier = 0.7f;
        private const float PassAimSweepAmplitudeMultiplier = 1.3f;
        private const float LobActivationThreshold = 0.12f;
        private const float MinLobHeight = 2.25f;
        private const float MaxLobHeight = 4.35f;
        private const float RefereeCollisionRadius = 0.72f;
        private const float DirectGoalCatchDelay = 2f;
        private const float GoalCatchHalfWidth = 2.18f;
        private const float GoalCatchHeight = 2.02f;
        private const float GoalkeeperSavePlaneOffset = 1.55f;
        private const float BlondeHeaderRadius = 0.72f;
        private const float BlondeHeaderMinHeight = 1.15f;
        private const float BlondeHeaderMaxHeight = 3.05f;
        private const float HeaderClearDuration = 0.82f;
        private const float LightningMinInterval = 6f;
        private const float LightningMaxInterval = 8f;
        private const float LightningVisualDuration = 0.34f;
        private const int MaxDashedPreviewSegments = 18;
        private const int UiSpriteSize = 64;
        private const int RingSegments = 24;
        private const int MaxOutfieldOpponents = 10;
        private const float RingHeight = 0.045f;
        private const float BallPostRangeRollFriction = 16f;
        private const float BallStopSpeed = 0.12f;
        private const float GemCollectRadius = 0.46f;

        private static readonly Dictionary<string, Sprite> uiRoundedSpriteCache = new Dictionary<string, Sprite>();

        private static readonly Color UiPanelFill = new Color(0.025f, 0.10f, 0.20f, 0.76f);
        private static readonly Color UiPanelBorder = new Color(0.45f, 0.86f, 1f, 0.42f);
        private static readonly Color UiButtonGreen = new Color(0.06f, 0.7f, 0.23f, 1f);
        private static readonly Color UiButtonBlue = new Color(0.04f, 0.35f, 0.88f, 1f);
        private static readonly Color UiButtonDark = new Color(0.05f, 0.16f, 0.28f, 0.9f);
        private static readonly Color UiTitleGold = new Color(0.72f, 0.93f, 1f, 1f);

        private enum GameplayState
        {
            Playing,
            Aiming,
            BallTraveling,
            AutoShot,
            GoalkeeperCatch,
            HeaderClear,
            Won,
            Failed,
            Paused
        }

        private enum ActiveBehaviorType
        {
            None,
            AutoRotate,
            LateralMove,
            TargetMove
        }

        private enum FailReason
        {
            PassMissed,
            OpponentIntercepted,
            NotEnoughPower,
            OutOfBounds,
            GoalkeeperCaught,
            HeaderCleared
        }

        private sealed class FriendlyDef
        {
            public string id;
            public int number;
            public Vector3 position;
            public bool isTargetTen;
            public ActiveBehaviorType behavior;
            public float sweepMinYaw;
            public float sweepMaxYaw;
            public float sweepSpeed;
            public Vector3 lateralStart;
            public Vector3 lateralEnd;
            public float lateralSpeed;
            public int targetMovePattern;
            public float targetMovePause;
            public float targetMovePhase;
            public float fixedYaw;
        }

        private sealed class OpponentDef
        {
            public Vector3 position;
            public Vector3 lateralStart;
            public Vector3 lateralEnd;
            public float lateralSpeed;
            public float lateralPhase;
            public bool isBlondeHeader;
            public int targetMovePattern;
            public float targetMovePause;
        }

        private sealed class LevelDef
        {
            public int number;
            public string prompt;
            public Vector2 fieldMin;
            public Vector2 fieldMax;
            public Vector3 goalPosition;
            public float receiverRadius;
            public float opponentRadius;
            public float minTravelDistance;
            public float maxTravelDistance;
            public float ballSpeed;
            public float chargeTime;
            public List<FriendlyDef> friendlies = new List<FriendlyDef>();
            public List<OpponentDef> opponents = new List<OpponentDef>();
            public List<Vector3> gemPositions = new List<Vector3>();
        }

        private sealed class GemRuntime
        {
            public GameObject root;
            public Vector3 position;
            public bool collected;
            public float idleOffset;
        }

        private sealed class FriendlyRuntime
        {
            public FriendlyDef def;
            public GameObject root;
            public Transform body;
            public Transform head;
            public Transform leftArm;
            public Transform rightArm;
            public Transform leftLeg;
            public Transform rightLeg;
            public TextMesh numberText;
            public LineRenderer activeRing;
            public LineRenderer targetRing;
            public float lateralPhase;
            public float movementDirection;
            public float movementPauseTimer;
            public int movementPauseMarker;
            public float idleSeed;
            public float kickTimer;
            public bool lightningStruck;
            public GameObject smokeObject;
        }

        private sealed class OpponentRuntime
        {
            public OpponentDef def;
            public GameObject root;
            public Transform body;
            public Transform head;
            public Transform leftArm;
            public Transform rightArm;
            public Transform leftLeg;
            public Transform rightLeg;
            public LineRenderer ring;
            public float idleSeed;
            public float lateralPhase;
            public float movementDirection;
            public float movementPauseTimer;
            public int movementPauseMarker;
            public float headerJumpTimer;
        }

        private readonly List<LevelDef> levels = new List<LevelDef>();
        private readonly List<FriendlyRuntime> friendlies = new List<FriendlyRuntime>();
        private readonly List<OpponentRuntime> opponents = new List<OpponentRuntime>();
        private readonly List<GemRuntime> gems = new List<GemRuntime>();
        private readonly List<GameObject> dashedPreviewSegments = new List<GameObject>();
        private readonly List<LineRenderer> dashedPreviewLines = new List<LineRenderer>();
        private readonly List<LineRenderer> lightningBranchLines = new List<LineRenderer>();
        private readonly List<RectTransform> celebrationUiPieces = new List<RectTransform>();
        private readonly List<Vector2> celebrationUiVelocities = new List<Vector2>();
        private readonly List<float> celebrationUiSpin = new List<float>();
        private readonly Dictionary<int, Vector3[]> ringPointCache = new Dictionary<int, Vector3[]>();

        private Font uiFont;
        private LevelDef level;
        private GameplayState state;
        private GameplayState stateBeforePause;
        private int activeFriendlyIndex;
        private float charge;
        private float lobAmount;
        private float lockedLobAmount;
        private Vector3 lockedAimDirection;
        private Vector2 aimStartScreenPosition;
        private Vector2 currentAimScreenPosition;
        private Vector3 ballDirection;
        private Vector3 ballTravelStart;
        private Vector3 autoShotStart;
        private float ballTravelDistance;
        private float ballMaxDistance;
        private float ballLobHeight;
        private float ballGroundHeight;
        private float ballVerticalOffset;
        private float ballVerticalVelocity;
        private float ballGravity;
        private float ballRestitution;
        private float ballCurrentSpeed;
        private bool ballIsLob;
        private float autoShotTimer;
        private Vector3 autoShotEnd;
        private int autoShotStyle;
        private float goalkeeperDiveTimer;
        private float goalkeeperCatchTimer;
        private Vector3 goalkeeperCatchStartPosition;
        private Vector3 goalkeeperCatchTargetPosition;
        private Vector3 goalkeeperCatchBallLocalPosition;
        private float headerClearTimer;
        private OpponentRuntime headerClearOpponent;
        private Vector3 headerClearOpponentBasePosition;
        private Vector3 headerClearBallStart;
        private Vector3 headerClearBallEnd;
        private bool rainActive;
        private float nextLightningTimer;
        private float lightningVisualTimer;

        private GameObject worldRoot;
        private GameObject ball;
        private GameObject rainObject;
        private LineRenderer lightningBolt;
        private Light lightningFlash;
        private GameObject receiverTargetBall;
        private LineRenderer receiverTargetRing;
        private GameObject goalkeeper;
        private Transform goalkeeperLeftArm;
        private Transform goalkeeperRightArm;
        private Transform goalkeeperLeftLeg;
        private Transform goalkeeperRightLeg;
        private GameObject referee;
        private Transform refereeLeftArm;
        private Transform refereeRightArm;
        private Transform refereeLeftLeg;
        private Transform refereeRightLeg;
        private float refereeRunSpeed;
        private float refereePhaseOffset;
        private float refereeDeflectionCooldown;
        private Vector3 refereeMotion;
        private Vector3 goalkeeperStartPosition;
        private Vector3 goalkeeperDiveTarget;
        private Camera gameplayCamera;
        private Vector3 baseCameraPosition;
        private Quaternion baseCameraRotation;
        private LineRenderer aimArrow;
        private LineRenderer aimCurve;
        private Canvas hudCanvas;
        private Text levelLabel;
        private Text promptText;
        private Text powerText;
        private Text walletText;
        private Image powerFill;
        private GameObject resultOverlay;
        private Text resultTitle;
        private Text resultRewardLabel;
        private Text resultReason;
        private Text resultGemReward;
        private Button resultNextButton;
        private GameObject pauseOverlay;
        private GameObject levelSelectOverlay;
        private GameObject celebrationUiRoot;
        private bool celebrationUiActive;
        private bool goalSlowMotionTriggered;
        private int gemsCollectedThisLevel;
        private float defaultFixedDeltaTime;

        private Material friendlyBlue;
        private Material friendlyWhite;
        private Material opponentRed;
        private Material grassA;
        private Material grassB;
        private Material lineWhite;
        private Material ballMaterial;
        private Material darkBoard;
        private Material crowdBlue;
        private Material crowdYellow;
        private Material skinMaterial;
        private Material hairMaterial;
        private Material playerHairMaterial;
        private Material playerJerseyMaterial;
        private Material playerAccessoryMaterial;
        private Material blackMaterial;
        private Material goldMaterial;
        private Material netMaterial;
        private Material softLineWhite;
        private Material gemMaterial;

        private void Awake()
        {
            defaultFixedDeltaTime = Time.fixedDeltaTime;
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateMaterials();
            CreateLevels();
            StartLevel(GameSession.SelectedLevelIndex);
        }

        private void Update()
        {
            if (level == null)
            {
                return;
            }

#if UNITY_EDITOR
            HandleEditorLevelSelectShortcuts();
#endif

            if (state == GameplayState.Playing)
            {
                UpdateFriendlyMovement(Time.deltaTime);
                UpdateOpponentMovement(Time.deltaTime);
                UpdateActiveBehavior(Time.deltaTime);
                UpdateAimingVisuals(false);
                if (HoldStarted())
                {
                    BeginAim();
                }
            }
            else if (state == GameplayState.Aiming)
            {
                UpdateFriendlyMovement(Time.deltaTime);
                UpdateOpponentMovement(Time.deltaTime);
                charge = Mathf.Clamp01(charge + Time.deltaTime * AimChargeSpeedMultiplier / GetChargeDuration());
                UpdateAimingVisuals(true);
                if (HoldCanceled())
                {
                    CancelAim();
                }
                else if (HoldReleased())
                {
                    LaunchPass();
                }
            }
            else if (state == GameplayState.BallTraveling)
            {
                UpdateFriendlyMovement(Time.deltaTime);
                UpdateOpponentMovement(Time.deltaTime);
                UpdateBallTravel(Time.deltaTime);
            }
            else if (state == GameplayState.AutoShot)
            {
                UpdateAutoShot(Time.deltaTime);
            }
            else if (state == GameplayState.GoalkeeperCatch)
            {
                UpdateGoalkeeperCatch(Time.deltaTime);
            }
            else if (state == GameplayState.HeaderClear)
            {
                UpdateHeaderClear(Time.deltaTime);
            }

            FaceNumberLabelsToCamera();
            UpdateReferee(Time.deltaTime);
            UpdateWeather(Time.deltaTime);
            UpdateGemPresentation();
            UpdateRingPositions();
            UpdateReceiverTargetMarker();
            UpdateCharacterAnimation(Time.deltaTime);
            UpdateCelebrationUi(Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            UpdateCameraFollow(Time.deltaTime);
        }

        private void StartLevel(int levelIndex)
        {
            if (levels.Count == 0)
            {
                CreateLevels();
            }

            GameSession.SelectLevel(levelIndex);
            level = levels[GameSession.SelectedLevelIndex];
            activeFriendlyIndex = 0;
            charge = 0f;
            lobAmount = 0f;
            lockedLobAmount = 0f;
            ballIsLob = false;
            goalkeeperCatchTimer = 0f;
            headerClearTimer = 0f;
            headerClearOpponent = null;
            gemsCollectedThisLevel = 0;
            rainActive = level.number % 3 == 0;
            nextLightningTimer = GetNextLightningInterval();
            lightningVisualTimer = 0f;
            refereeDeflectionCooldown = 0f;
            refereeMotion = Vector3.zero;
            goalSlowMotionTriggered = false;
            celebrationUiActive = false;
            ResetTimeScale();
            state = GameplayState.Playing;
            ClearScene();
            BuildWorld();
            BuildHud();
            activeFriendlyIndex = 0;
            AttachBallToActive();
            UpdateAimingVisuals(false);
            SnapCameraToBall();
        }

        private void ClearScene()
        {
            if (worldRoot != null)
            {
                Destroy(worldRoot);
            }

            if (hudCanvas != null)
            {
                Destroy(hudCanvas.gameObject);
            }

            friendlies.Clear();
            opponents.Clear();
            gems.Clear();
            dashedPreviewSegments.Clear();
            dashedPreviewLines.Clear();
            ball = null;
            receiverTargetBall = null;
            receiverTargetRing = null;
            goalkeeper = null;
            rainObject = null;
            lightningBolt = null;
            lightningFlash = null;
            lightningBranchLines.Clear();
            goalkeeperLeftArm = null;
            goalkeeperRightArm = null;
            goalkeeperLeftLeg = null;
            goalkeeperRightLeg = null;
            referee = null;
            refereeLeftArm = null;
            refereeRightArm = null;
            refereeLeftLeg = null;
            refereeRightLeg = null;
            resultOverlay = null;
            pauseOverlay = null;
            levelSelectOverlay = null;
        }

        private void BuildWorld()
        {
            worldRoot = new GameObject("Generated_Gameplay_World");
            SetupCameraAndLight();
            BuildField();
            BuildGoal();
            if (level.number >= 3)
            {
                CreateReferee();
            }

            for (int i = 0; i < level.friendlies.Count; i++)
            {
                friendlies.Add(CreateFriendly(level.friendlies[i]));
            }

            for (int i = 0; i < level.opponents.Count; i++)
            {
                opponents.Add(CreateOpponent(level.opponents[i]));
            }

            ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball";
            ball.transform.SetParent(worldRoot.transform, false);
            ball.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
            ball.GetComponent<Renderer>().material = ballMaterial;
            CreateSoccerBallPattern();
            CreateLevelGems();
            CreateReceiverTargetMarker();

            aimArrow = CreateLine("Aim_Arrow", new Color(0.9f, 1f, 0.9f, 0.8f), 0.08f, false);
            aimCurve = CreateLine("Lob_Curve_Preview_Disabled", new Color(0.95f, 1f, 0.26f, 0.92f), 0.065f, false);
            aimCurve.gameObject.SetActive(false);

            BuildWeather();

            DisableWorldColliders();
            DisableWorldShadows();
        }

        // All gameplay collisions are resolved by the deterministic distance checks below.
        // Runtime primitives otherwise create hundreds of unused PhysX colliders on later levels.
        private void DisableWorldColliders()
        {
            Collider[] colliders = worldRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
                Destroy(colliders[i]);
            }
        }

        private void DisableWorldShadows()
        {
            Renderer[] renderers = worldRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
            }
        }

        private void BuildWeather()
        {
            if (!rainActive)
            {
                return;
            }

            Vector3 center = new Vector3((level.fieldMin.x + level.fieldMax.x) * 0.5f, 5.8f, (level.fieldMin.y + level.fieldMax.y) * 0.5f);
            Vector3 size = new Vector3(level.fieldMax.x - level.fieldMin.x + 3.2f, 0.4f, level.fieldMax.y - level.fieldMin.y + 3.2f);

            rainObject = new GameObject("Light_Rain");
            rainObject.transform.SetParent(worldRoot.transform, false);
            rainObject.transform.position = center;

            ParticleSystem rain = rainObject.AddComponent<ParticleSystem>();
            rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = rain.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.85f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.045f);
            main.startColor = new Color(0.68f, 0.86f, 1f, 0.42f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 180;

            ParticleSystem.EmissionModule emission = rain.emission;
            emission.rateOverTime = 56f;

            ParticleSystem.ShapeModule shape = rain.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;

            ParticleSystem.VelocityOverLifetimeModule velocity = rain.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.35f, -0.12f);
            velocity.y = new ParticleSystem.MinMaxCurve(-6.2f, -4.8f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);

            ParticleSystemRenderer renderer = rain.GetComponent<ParticleSystemRenderer>();
            renderer.material = MakeUnlitMaterial("Light_Rain_Material", new Color(0.68f, 0.86f, 1f, 0.48f));
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.8f;
            renderer.velocityScale = 0.45f;

            rain.Play();

            lightningBolt = CreateLine("Lightning_Bolt", new Color(0.84f, 0.94f, 1f, 0.95f), 0.14f, false);
            lightningBolt.gameObject.SetActive(false);

            for (int i = 0; i < 4; i++)
            {
                LineRenderer branch = CreateLine("Lightning_Branch_" + i, new Color(0.72f, 0.9f, 1f, 0.82f), 0.055f, false);
                branch.gameObject.SetActive(false);
                lightningBranchLines.Add(branch);
            }

            GameObject flashObject = new GameObject("Lightning_Flash");
            flashObject.transform.SetParent(worldRoot.transform, false);
            lightningFlash = flashObject.AddComponent<Light>();
            lightningFlash.type = LightType.Point;
            lightningFlash.color = new Color(0.72f, 0.88f, 1f, 1f);
            lightningFlash.range = 6.5f;
            lightningFlash.intensity = 0f;
            lightningFlash.enabled = false;
        }

        private void UpdateWeather(float deltaTime)
        {
            if (!rainActive)
            {
                return;
            }

            if (lightningVisualTimer > 0f)
            {
                lightningVisualTimer = Mathf.Max(0f, lightningVisualTimer - deltaTime);
                if (lightningVisualTimer <= 0f && lightningBolt != null)
                {
                    lightningBolt.gameObject.SetActive(false);
                    SetLightningBranchesActive(false);
                    if (lightningFlash != null)
                    {
                        lightningFlash.enabled = false;
                        lightningFlash.intensity = 0f;
                    }
                }
                else if (lightningFlash != null)
                {
                    lightningFlash.intensity = Mathf.Lerp(0f, 7f, lightningVisualTimer / LightningVisualDuration);
                }
            }

            if (!IsWeatherHazardActive())
            {
                return;
            }

            nextLightningTimer -= deltaTime;
            if (nextLightningTimer > 0f)
            {
                return;
            }

            TryStrikeRandomFriendly();
            nextLightningTimer = GetNextLightningInterval();
        }

        private bool IsWeatherHazardActive()
        {
            return state == GameplayState.Playing || state == GameplayState.Aiming || state == GameplayState.BallTraveling;
        }

        private float GetNextLightningInterval()
        {
            return Random.Range(LightningMinInterval, LightningMaxInterval);
        }

        private void TryStrikeRandomFriendly()
        {
            List<int> candidates = new List<int>();
            for (int i = 0; i < friendlies.Count; i++)
            {
                if (i == activeFriendlyIndex || friendlies[i].def.isTargetTen || friendlies[i].lightningStruck)
                {
                    continue;
                }

                candidates.Add(i);
            }

            if (candidates.Count == 0)
            {
                return;
            }

            int receiverIndex = candidates[Random.Range(0, candidates.Count)];
            ApplyLightningStrike(friendlies[receiverIndex]);
        }

        private void ApplyLightningStrike(FriendlyRuntime runtime)
        {
            runtime.lightningStruck = true;
            runtime.kickTimer = 0f;
            runtime.movementPauseTimer = 0f;
            runtime.root.transform.rotation = Quaternion.Euler(0f, runtime.def.fixedYaw, 0f);
            BlackenCharacter(runtime.root);
            CreateLightningBolt(runtime.root.transform.position + Vector3.up * 1.15f);
            CreateSmokeOnFriendly(runtime);
            StadiumAudio.PlayLightning();
            SetReceiverTargetVisible(false);
        }

        private void BlackenCharacter(GameObject target)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = Color.Lerp(renderers[i].material.color, Color.black, 0.86f);
            }
        }

        private void CreateLightningBolt(Vector3 target)
        {
            if (lightningBolt == null)
            {
                return;
            }

            Vector3 start = target + new Vector3(Random.Range(-0.55f, 0.55f), 5.9f, Random.Range(-0.35f, 0.35f));
            const int boltPoints = 8;
            Vector3[] points = new Vector3[boltPoints];
            lightningBolt.positionCount = boltPoints;
            for (int i = 0; i < boltPoints; i++)
            {
                float t = i / (float)(boltPoints - 1);
                Vector3 point = Vector3.Lerp(start, target, t);
                if (i > 0 && i < lightningBolt.positionCount - 1)
                {
                    float jitter = Mathf.Lerp(0.34f, 0.12f, t);
                    point += new Vector3(Random.Range(-jitter, jitter), 0f, Random.Range(-jitter, jitter));
                }
                points[i] = point;
                lightningBolt.SetPosition(i, point);
            }

            lightningBolt.gameObject.SetActive(true);
            CreateLightningBranches(points);
            if (lightningFlash != null)
            {
                lightningFlash.transform.position = target + Vector3.up * 0.22f;
                lightningFlash.intensity = 7f;
                lightningFlash.enabled = true;
            }
            lightningVisualTimer = LightningVisualDuration;
        }

        private void CreateLightningBranches(Vector3[] boltPoints)
        {
            for (int i = 0; i < lightningBranchLines.Count; i++)
            {
                LineRenderer branch = lightningBranchLines[i];
                if (branch == null || boltPoints.Length < 4)
                {
                    continue;
                }

                int startIndex = Mathf.Clamp(2 + i, 1, boltPoints.Length - 3);
                Vector3 branchStart = boltPoints[startIndex];
                float side = i % 2 == 0 ? 1f : -1f;
                Vector3 branchEnd = branchStart + new Vector3(side * Random.Range(0.28f, 0.72f), Random.Range(-0.38f, -0.08f), Random.Range(-0.42f, 0.42f));
                Vector3 branchMid = Vector3.Lerp(branchStart, branchEnd, 0.55f) + new Vector3(side * Random.Range(0.08f, 0.22f), 0f, Random.Range(-0.15f, 0.15f));

                branch.positionCount = 3;
                branch.SetPosition(0, branchStart);
                branch.SetPosition(1, branchMid);
                branch.SetPosition(2, branchEnd);
                branch.gameObject.SetActive(true);
            }
        }

        private void SetLightningBranchesActive(bool active)
        {
            for (int i = 0; i < lightningBranchLines.Count; i++)
            {
                if (lightningBranchLines[i] != null)
                {
                    lightningBranchLines[i].gameObject.SetActive(active);
                }
            }
        }

        private void CreateSmokeOnFriendly(FriendlyRuntime runtime)
        {
            if (runtime.smokeObject != null)
            {
                return;
            }

            GameObject smokeObject = new GameObject("Lightning_Smoke");
            smokeObject.transform.SetParent(runtime.root.transform, false);
            smokeObject.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            runtime.smokeObject = smokeObject;

            ParticleSystem smoke = smokeObject.AddComponent<ParticleSystem>();
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = smoke.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.25f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.18f, 0.48f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            main.startColor = new Color(0.08f, 0.08f, 0.08f, 0.62f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 36;

            ParticleSystem.EmissionModule emission = smoke.emission;
            emission.rateOverTime = 12f;

            ParticleSystem.ShapeModule shape = smoke.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.16f;

            ParticleSystem.VelocityOverLifetimeModule velocity = smoke.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.55f, 0.95f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

            ParticleSystemRenderer renderer = smoke.GetComponent<ParticleSystemRenderer>();
            renderer.material = MakeUnlitMaterial("Lightning_Smoke_Material", new Color(0.08f, 0.08f, 0.08f, 0.62f));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            smoke.Play();
        }

        private void SetupCameraAndLight()
        {
            Camera camera = FindSceneObject<Camera>();
            if (camera == null)
            {
                camera = new GameObject("Gameplay_Camera").AddComponent<Camera>();
            }

            camera.name = "Gameplay_Camera";
            camera.tag = "MainCamera";
            baseCameraPosition = new Vector3(0f, 13.9f, -9.2f);
            baseCameraRotation = Quaternion.Euler(69f, 0f, 0f);
            camera.transform.position = baseCameraPosition;
            camera.transform.rotation = baseCameraRotation;
            camera.fieldOfView = 48f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.62f, 0.18f);
            gameplayCamera = camera;

            Light light = FindSceneObject<Light>();
            if (light == null)
            {
                light = new GameObject("Gameplay_Sun").AddComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.18f;
            light.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(50f, -25f, 0f);
            RenderSettings.ambientLight = new Color(0.7f, 0.78f, 0.86f);
        }

        private void BuildField()
        {
            float width = level.fieldMax.x - level.fieldMin.x;
            float depth = level.fieldMax.y - level.fieldMin.y;
            float fieldScale = Mathf.Max(1f, width / OriginalPitchWidth);
            Vector3 center = new Vector3((level.fieldMin.x + level.fieldMax.x) * 0.5f, -0.06f, (level.fieldMin.y + level.fieldMax.y) * 0.5f);

            GameObject cameraFillGrass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cameraFillGrass.name = "Camera_Fill_Grass";
            cameraFillGrass.transform.SetParent(worldRoot.transform, false);
            cameraFillGrass.transform.position = new Vector3(center.x, -0.125f, center.z + depth * 0.08f);
            cameraFillGrass.transform.localScale = new Vector3(width * 3.4f, 0.06f, depth * 3.5f);
            cameraFillGrass.GetComponent<Renderer>().material = grassA;

            GameObject baseField = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseField.name = "Green_Field";
            baseField.transform.SetParent(worldRoot.transform, false);
            baseField.transform.position = center;
            baseField.transform.localScale = new Vector3(width, 0.08f, depth);
            baseField.GetComponent<Renderer>().material = grassA;

            GameObject foregroundGrass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foregroundGrass.name = "Foreground_Mobile_Grass";
            foregroundGrass.transform.SetParent(worldRoot.transform, false);
            foregroundGrass.transform.position = new Vector3(center.x, -0.075f, level.fieldMin.y - 1.8f * fieldScale);
            foregroundGrass.transform.localScale = new Vector3(width * 1.08f, 0.07f, 3.6f * fieldScale);
            foregroundGrass.GetComponent<Renderer>().material = grassA;

            for (int i = 0; i < 12; i++)
            {
                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "Field_Stripe";
                stripe.transform.SetParent(worldRoot.transform, false);
                stripe.transform.position = new Vector3(center.x, -0.01f, level.fieldMin.y + 0.7f * fieldScale + i * 1.55f * fieldScale);
                stripe.transform.localScale = new Vector3(width, 0.025f, 0.75f * fieldScale);
                stripe.GetComponent<Renderer>().material = i % 2 == 0 ? grassB : grassA;
            }

            float lineWidth = 0.045f * fieldScale;
            CreateFieldLine("Sideline_Left", new Vector3(level.fieldMin.x, 0.02f, center.z), new Vector3(lineWidth, 0.035f, depth));
            CreateFieldLine("Sideline_Right", new Vector3(level.fieldMax.x, 0.02f, center.z), new Vector3(lineWidth, 0.035f, depth));
            CreateFieldLine("Endline_Bottom", new Vector3(center.x, 0.02f, level.fieldMin.y), new Vector3(width, 0.035f, lineWidth));
            CreateFieldLine("Endline_Top", new Vector3(center.x, 0.02f, level.fieldMax.y), new Vector3(width, 0.035f, lineWidth));
            CreateFieldLine("Halfway_Line", new Vector3(center.x, 0.025f, -0.22f * fieldScale), new Vector3(width, 0.035f, lineWidth));
            CreateGroundCircle("Center_Circle", new Vector3(0f, 0.055f, -0.22f * fieldScale), 1.58f * fieldScale, 0.045f * fieldScale, softLineWhite);
            CreateFieldLine("Penalty_Box_Front", new Vector3(0f, 0.03f, level.fieldMax.y - 1.6f * fieldScale), new Vector3(5.2f * fieldScale, 0.035f, lineWidth));
            CreateFieldLine("Penalty_Box_Left", new Vector3(-2.6f * fieldScale, 0.03f, level.fieldMax.y - 0.8f * fieldScale), new Vector3(lineWidth, 0.035f, 1.6f * fieldScale));
            CreateFieldLine("Penalty_Box_Right", new Vector3(2.6f * fieldScale, 0.03f, level.fieldMax.y - 0.8f * fieldScale), new Vector3(lineWidth, 0.035f, 1.6f * fieldScale));
            CreateFieldLine("Goal_Box_Front", new Vector3(0f, 0.032f, level.fieldMax.y - 0.72f * fieldScale), new Vector3(3.1f * fieldScale, 0.035f, lineWidth));
            CreateFieldLine("Goal_Box_Left", new Vector3(-1.55f * fieldScale, 0.032f, level.fieldMax.y - 0.36f * fieldScale), new Vector3(lineWidth, 0.035f, 0.72f * fieldScale));
            CreateFieldLine("Goal_Box_Right", new Vector3(1.55f * fieldScale, 0.032f, level.fieldMax.y - 0.36f * fieldScale), new Vector3(lineWidth, 0.035f, 0.72f * fieldScale));

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject crowd = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crowd.name = "Crowd_Block";
                crowd.transform.SetParent(worldRoot.transform, false);
                crowd.transform.position = new Vector3(side * (width * 0.5f + 0.72f), 0.65f, center.z + 0.65f);
                crowd.transform.localScale = new Vector3(0.9f, 1.25f, depth - 0.7f);
                crowd.GetComponent<Renderer>().material = side < 0 ? crowdBlue : crowdYellow;
            }

            BuildSpectatorCrowd(width, depth, center);

            string[] boardNames = { "ROAD FC", "GOAL TIME", "CHAMPIONS ROAD", "PASS ON" };
            for (int i = 0; i < 4; i++)
            {
                GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
                board.name = "Fictional_Board_" + boardNames[i].Replace(" ", "_");
                board.transform.SetParent(worldRoot.transform, false);
                board.transform.position = new Vector3((-3f + i * 2f) * fieldScale, 0.26f, level.fieldMax.y + 0.35f * fieldScale);
                board.transform.localScale = new Vector3(1.7f * fieldScale, 0.42f, 0.12f);
                board.GetComponent<Renderer>().material = darkBoard;
                CreateBoardText(boardNames[i], board.transform.position + new Vector3(0f, 0.02f, -0.085f));
            }

            BuildCornerFlags();
        }

        private void CreateBoardText(string label, Vector3 position)
        {
            TextMesh text = new GameObject("Board_Text_" + label.Replace(" ", "_")).AddComponent<TextMesh>();
            text.transform.SetParent(worldRoot.transform, false);
            text.transform.position = position;
            text.transform.rotation = Quaternion.Euler(68f, 0f, 0f);
            text.text = label;
            text.font = uiFont;
            text.fontSize = 28;
            text.characterSize = 0.045f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.65f, 0.92f, 1f);
        }

        private void CreateFieldLine(string objectName, Vector3 position, Vector3 scale)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = objectName;
            line.transform.SetParent(worldRoot.transform, false);
            line.transform.position = position;
            line.transform.localScale = scale;
            line.GetComponent<Renderer>().material = lineWhite;
        }

        private void CreateGroundCircle(string objectName, Vector3 center, float radius, float width, Material material, float startTurn = 0f, float endTurn = 1f)
        {
            GameObject circleObject = new GameObject(objectName);
            circleObject.transform.SetParent(worldRoot.transform, false);
            LineRenderer line = circleObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = Mathf.Abs(endTurn - startTurn) >= 0.99f;
            line.widthMultiplier = width;
            line.material = material;
            line.startColor = material.color;
            line.endColor = material.color;

            int pointCount = line.loop ? 72 : 38;
            line.positionCount = line.loop ? pointCount : pointCount + 1;
            for (int i = 0; i < line.positionCount; i++)
            {
                float t = line.loop ? i / (float)pointCount : i / (float)pointCount;
                float turn = Mathf.Lerp(startTurn, endTurn, t);
                float angle = turn * Mathf.PI * 2f;
                line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private void BuildSpectatorCrowd(float width, float depth, Vector3 center)
        {
            Material[] shirts = { crowdBlue, crowdYellow, opponentRed, friendlyWhite, friendlyBlue };

            for (int row = 0; row < 4; row++)
            {
                float z = Mathf.Max(level.fieldMax.y + 2.9f, level.goalPosition.z + 2.4f) + row * 0.45f;
                float y = 0.32f + row * 0.22f;
                for (int i = 0; i < 15; i++)
                {
                    float x = -width * 0.46f + i * (width * 0.92f / 14f);
                    CreateCrowdPerson(new Vector3(x, y, z), 0.42f, shirts[(i + row) % shirts.Length], i + row * 17);
                }
            }

            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < 3; row++)
                {
                    float x = side * (width * 0.5f + 0.95f + row * 0.18f);
                    float y = 0.36f + row * 0.18f;
                    for (int i = 0; i < 12; i++)
                    {
                        float z = center.z - depth * 0.4f + i * (depth * 0.8f / 11f);
                        CreateCrowdPerson(new Vector3(x, y, z), 0.36f, shirts[(i + row + (side > 0 ? 2 : 0)) % shirts.Length], i + row * 11);
                    }
                }
            }
        }

        private void CreateCrowdPerson(Vector3 position, float scale, Material shirt, int seed)
        {
            GameObject root = new GameObject("Crowd_Person");
            root.transform.SetParent(worldRoot.transform, false);
            root.transform.position = position + new Vector3(Mathf.Sin(seed * 1.7f) * 0.04f, 0f, Mathf.Cos(seed * 1.1f) * 0.04f);
            root.transform.rotation = Quaternion.Euler(0f, 180f + Mathf.Sin(seed) * 8f, 0f);
            root.transform.localScale = new Vector3(scale * 2f, scale * 2f, scale * 2f);

            CreateBlockPart(root.transform, "Crowd_Body", new Vector3(0f, 0.58f, 0f), new Vector3(0.38f, 0.58f, 0.22f), shirt);
            CreateBlockPart(root.transform, "Crowd_Head", new Vector3(0f, 1.02f, 0f), new Vector3(0.28f, 0.28f, 0.28f), skinMaterial);
            CreateBlockPart(root.transform, "Crowd_Hair", new Vector3(0f, 1.18f, -0.02f), new Vector3(0.3f, 0.08f, 0.26f), hairMaterial);

            float cheer = seed % 3 == 0 ? 0.22f : 0f;
            CreateBlockPart(root.transform, "Crowd_Left_Arm", new Vector3(-0.3f, 0.64f + cheer, 0f), new Vector3(0.09f, 0.42f, 0.11f), skinMaterial)
                .transform.localRotation = Quaternion.Euler(0f, 0f, seed % 3 == 0 ? -32f : -8f);
            CreateBlockPart(root.transform, "Crowd_Right_Arm", new Vector3(0.3f, 0.64f + cheer, 0f), new Vector3(0.09f, 0.42f, 0.11f), skinMaterial)
                .transform.localRotation = Quaternion.Euler(0f, 0f, seed % 4 == 0 ? 32f : 8f);
        }

        private void BuildCornerFlags()
        {
            Vector3[] corners =
            {
                new Vector3(level.fieldMin.x, 0f, level.fieldMin.y),
                new Vector3(level.fieldMax.x, 0f, level.fieldMin.y),
                new Vector3(level.fieldMin.x, 0f, level.fieldMax.y),
                new Vector3(level.fieldMax.x, 0f, level.fieldMax.y)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Corner_Flag_Pole";
                pole.transform.SetParent(worldRoot.transform, false);
                pole.transform.position = corners[i] + new Vector3(i % 2 == 0 ? -0.12f : 0.12f, 0.45f, i < 2 ? -0.12f : 0.12f);
                pole.transform.localScale = new Vector3(0.025f, 0.45f, 0.025f);
                pole.GetComponent<Renderer>().material = lineWhite;

                GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flag.name = "Corner_Flag";
                flag.transform.SetParent(worldRoot.transform, false);
                flag.transform.position = pole.transform.position + new Vector3(i % 2 == 0 ? -0.18f : 0.18f, 0.34f, 0f);
                flag.transform.localScale = new Vector3(0.34f, 0.22f, 0.025f);
                flag.GetComponent<Renderer>().material = i % 2 == 0 ? friendlyBlue : goldMaterial;
            }
        }

        private void BuildBehindGoalMedia(Vector3 goal, float halfWidth, float depth)
        {
            Material[] shirts = { friendlyWhite, crowdBlue, crowdYellow, opponentRed };
            for (int i = 0; i < 7; i++)
            {
                float x = -halfWidth - 2.2f + i * ((halfWidth * 2f + 4.4f) / 6f);
                float z = goal.z + depth + 6.8f + Mathf.Abs(Mathf.Sin(i * 1.8f)) * 0.9f;
                Vector3 position = new Vector3(x, 0f, z);
                if (i % 2 == 0)
                {
                    CreatePhotographer(position, shirts[i % shirts.Length], i);
                }
                else
                {
                    CreateGoalFan(position, shirts[i % shirts.Length], i);
                }
            }
        }

        private void CreatePhotographer(Vector3 position, Material shirt, int seed)
        {
            GameObject root = new GameObject("Behind_Goal_Photographer");
            root.transform.SetParent(worldRoot.transform, false);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, 180f + Mathf.Sin(seed) * 8f, 0f);
            root.transform.localScale = new Vector3(1.76f, 1.76f, 1.76f);

            CreateBlockPart(root.transform, "Photographer_Body", new Vector3(0f, 0.58f, 0f), new Vector3(0.42f, 0.62f, 0.24f), shirt);
            CreateBlockPart(root.transform, "Photographer_Head", new Vector3(0f, 1.06f, 0f), new Vector3(0.3f, 0.3f, 0.3f), skinMaterial);
            CreateBlockPart(root.transform, "Photographer_Hair", new Vector3(0f, 1.22f, -0.02f), new Vector3(0.32f, 0.08f, 0.28f), hairMaterial);
            CreateBlockPart(root.transform, "Photographer_Camera", new Vector3(0f, 0.86f, -0.24f), new Vector3(0.36f, 0.22f, 0.18f), blackMaterial);
            CreateBlockPart(root.transform, "Photographer_Lens", new Vector3(0f, 0.86f, -0.39f), new Vector3(0.18f, 0.16f, 0.16f), blackMaterial);
            CreateBlockPart(root.transform, "Photographer_Left_Arm", new Vector3(-0.32f, 0.76f, -0.12f), new Vector3(0.1f, 0.42f, 0.1f), skinMaterial)
                .transform.localRotation = Quaternion.Euler(54f, 0f, -18f);
            CreateBlockPart(root.transform, "Photographer_Right_Arm", new Vector3(0.32f, 0.76f, -0.12f), new Vector3(0.1f, 0.42f, 0.1f), skinMaterial)
                .transform.localRotation = Quaternion.Euler(54f, 0f, 18f);
        }

        private void CreateGoalFan(Vector3 position, Material shirt, int seed)
        {
            GameObject root = new GameObject("Behind_Goal_Fan");
            root.transform.SetParent(worldRoot.transform, false);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, 180f + Mathf.Sin(seed) * 10f, 0f);
            root.transform.localScale = new Vector3(1.72f, 1.72f, 1.72f);

            CreateBlockPart(root.transform, "Fan_Body", new Vector3(0f, 0.58f, 0f), new Vector3(0.42f, 0.62f, 0.24f), shirt);
            CreateBlockPart(root.transform, "Fan_Head", new Vector3(0f, 1.06f, 0f), new Vector3(0.3f, 0.3f, 0.3f), skinMaterial);
            CreateBlockPart(root.transform, "Fan_Hair", new Vector3(0f, 1.22f, -0.02f), new Vector3(0.32f, 0.08f, 0.28f), hairMaterial);
            CreateBlockPart(root.transform, "Fan_Left_Arm", new Vector3(-0.34f, 0.9f, 0f), new Vector3(0.1f, 0.5f, 0.1f), skinMaterial)
                .transform.localRotation = Quaternion.Euler(0f, 0f, -36f);
            CreateBlockPart(root.transform, "Fan_Right_Arm", new Vector3(0.34f, 0.9f, 0f), new Vector3(0.1f, 0.5f, 0.1f), skinMaterial)
                .transform.localRotation = Quaternion.Euler(0f, 0f, 36f);

            GameObject scarf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scarf.name = "Fan_Scarf";
            scarf.transform.SetParent(root.transform, false);
            scarf.transform.localPosition = new Vector3(0f, 1.34f, -0.03f);
            scarf.transform.localScale = new Vector3(0.72f, 0.09f, 0.05f);
            scarf.GetComponent<Renderer>().material = seed % 2 == 0 ? friendlyBlue : goldMaterial;
        }

        private void BuildGoal()
        {
            Vector3 goal = level.goalPosition;
            const float halfWidth = 2.42f;
            const float height = 2.05f;
            const float depth = 1.12f;
            const float postSize = 0.13f;

            CreatePost("Goal_Left_Post", new Vector3(goal.x - halfWidth, height * 0.5f, goal.z), new Vector3(postSize, height, postSize));
            CreatePost("Goal_Right_Post", new Vector3(goal.x + halfWidth, height * 0.5f, goal.z), new Vector3(postSize, height, postSize));
            CreatePost("Goal_Crossbar", new Vector3(goal.x, height, goal.z), new Vector3(halfWidth * 2f + postSize, postSize, postSize));
            CreatePost("Goal_Left_Back_Post", new Vector3(goal.x - halfWidth, height * 0.5f, goal.z + depth), new Vector3(0.09f, height * 0.9f, 0.09f));
            CreatePost("Goal_Right_Back_Post", new Vector3(goal.x + halfWidth, height * 0.5f, goal.z + depth), new Vector3(0.09f, height * 0.9f, 0.09f));
            CreatePost("Goal_Back_Crossbar", new Vector3(goal.x, height * 0.94f, goal.z + depth), new Vector3(halfWidth * 2f, 0.08f, 0.08f));
            CreatePost("Goal_Left_Top_Rail", new Vector3(goal.x - halfWidth, height, goal.z + depth * 0.5f), new Vector3(0.08f, 0.08f, depth));
            CreatePost("Goal_Right_Top_Rail", new Vector3(goal.x + halfWidth, height, goal.z + depth * 0.5f), new Vector3(0.08f, 0.08f, depth));

            for (int i = 0; i < 9; i++)
            {
                float x = goal.x - halfWidth + i * (halfWidth * 2f / 8f);
                CreateNetLine("Goal_Back_Net_Vertical", new Vector3(x, height * 0.48f, goal.z + depth), new Vector3(0.018f, height * 0.9f, 0.035f));
                CreateNetLine("Goal_Top_Net_Depth", new Vector3(x, height, goal.z + depth * 0.5f), new Vector3(0.018f, 0.035f, depth));
            }

            for (int i = 0; i < 6; i++)
            {
                float y = 0.22f + i * (height * 0.74f / 5f);
                CreateNetLine("Goal_Back_Net_Horizontal", new Vector3(goal.x, y, goal.z + depth), new Vector3(halfWidth * 2f, 0.018f, 0.035f));
                CreateNetLine("Goal_Left_Side_Net", new Vector3(goal.x - halfWidth, y, goal.z + depth * 0.5f), new Vector3(0.035f, 0.018f, depth));
                CreateNetLine("Goal_Right_Side_Net", new Vector3(goal.x + halfWidth, y, goal.z + depth * 0.5f), new Vector3(0.035f, 0.018f, depth));
            }

            CreateFieldLine("Penalty_Spot", new Vector3(goal.x, 0.055f, goal.z - 1.7f), new Vector3(0.18f, 0.035f, 0.18f));
            BuildBehindGoalMedia(goal, halfWidth, depth);
            CreateGoalkeeper(goal);
        }

        private void CreateGoalkeeper(Vector3 goal)
        {
            goalkeeper = new GameObject("Goalkeeper");
            goalkeeper.transform.SetParent(worldRoot.transform, false);
            goalkeeper.transform.position = new Vector3(goal.x, 0f, goal.z - 0.52f);
            goalkeeper.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            goalkeeperStartPosition = goalkeeper.transform.position;

            CreateBlockPart(goalkeeper.transform, "Keeper_Body", new Vector3(0f, 0.58f, 0f), new Vector3(0.5f, 0.72f, 0.32f), opponentRed);
            CreateBlockPart(goalkeeper.transform, "Keeper_Shirt_Panel", new Vector3(0f, 0.64f, -0.17f), new Vector3(0.22f, 0.52f, 0.025f), friendlyWhite);
            CreateBlockPart(goalkeeper.transform, "Keeper_Head", new Vector3(0f, 1.08f, 0f), new Vector3(0.34f, 0.34f, 0.34f), skinMaterial);
            CreateBlockPart(goalkeeper.transform, "Keeper_Hair", new Vector3(0f, 1.28f, -0.01f), new Vector3(0.38f, 0.1f, 0.34f), hairMaterial);
            goalkeeperLeftArm = CreateBlockPart(goalkeeper.transform, "Keeper_Left_Arm", new Vector3(-0.42f, 0.64f, 0f), new Vector3(0.14f, 0.62f, 0.17f), skinMaterial).transform;
            goalkeeperRightArm = CreateBlockPart(goalkeeper.transform, "Keeper_Right_Arm", new Vector3(0.42f, 0.64f, 0f), new Vector3(0.14f, 0.62f, 0.17f), skinMaterial).transform;
            goalkeeperLeftLeg = CreateBlockPart(goalkeeper.transform, "Keeper_Left_Leg", new Vector3(-0.16f, 0f, 0f), new Vector3(0.15f, 0.42f, 0.17f), blackMaterial).transform;
            goalkeeperRightLeg = CreateBlockPart(goalkeeper.transform, "Keeper_Right_Leg", new Vector3(0.16f, 0f, 0f), new Vector3(0.15f, 0.42f, 0.17f), blackMaterial).transform;
        }

        private void CreateReferee()
        {
            referee = new GameObject("Referee");
            referee.transform.SetParent(worldRoot.transform, false);
            referee.transform.position = ScaleGameplayPosition(new Vector3(-1.85f, 0f, -0.85f));
            referee.transform.rotation = Quaternion.Euler(0f, 18f, 0f);
            refereeRunSpeed = Mathf.Lerp(0.76f, 1.42f, Mathf.PingPong(level.number * 0.23f, 1f));
            refereePhaseOffset = level.number * 0.73f;

            CreateBlockPart(referee.transform, "Referee_Body", new Vector3(0f, 0.6f, 0f), new Vector3(0.43f, 0.68f, 0.3f), goldMaterial);
            CreateBlockPart(referee.transform, "Referee_Collar", new Vector3(0f, 0.94f, -0.16f), new Vector3(0.28f, 0.08f, 0.035f), blackMaterial);
            CreateBlockPart(referee.transform, "Referee_Shorts", new Vector3(0f, 0.24f, 0f), new Vector3(0.46f, 0.2f, 0.32f), blackMaterial);
            CreateBlockPart(referee.transform, "Referee_Head", new Vector3(0f, 1.07f, 0f), new Vector3(0.32f, 0.32f, 0.32f), skinMaterial);
            CreateBlockPart(referee.transform, "Referee_Hair", new Vector3(0f, 1.25f, -0.01f), new Vector3(0.35f, 0.09f, 0.32f), hairMaterial);
            refereeLeftArm = CreateBlockPart(referee.transform, "Referee_Left_Arm", new Vector3(-0.32f, 0.6f, 0f), new Vector3(0.12f, 0.52f, 0.15f), skinMaterial).transform;
            refereeRightArm = CreateBlockPart(referee.transform, "Referee_Right_Arm", new Vector3(0.32f, 0.6f, 0f), new Vector3(0.12f, 0.52f, 0.15f), skinMaterial).transform;
            refereeLeftLeg = CreateBlockPart(referee.transform, "Referee_Left_Leg", new Vector3(-0.13f, -0.04f, 0f), new Vector3(0.13f, 0.38f, 0.15f), skinMaterial).transform;
            refereeRightLeg = CreateBlockPart(referee.transform, "Referee_Right_Leg", new Vector3(0.13f, -0.04f, 0f), new Vector3(0.13f, 0.38f, 0.15f), skinMaterial).transform;
            CreateBlockPart(referee.transform, "Referee_Left_Boot", new Vector3(-0.13f, -0.25f, -0.03f), new Vector3(0.17f, 0.1f, 0.2f), blackMaterial);
            CreateBlockPart(referee.transform, "Referee_Right_Boot", new Vector3(0.13f, -0.25f, -0.03f), new Vector3(0.17f, 0.1f, 0.2f), blackMaterial);
        }

        private void CreatePost(string name, Vector3 position, Vector3 scale)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = name;
            post.transform.SetParent(worldRoot.transform, false);
            post.transform.position = position;
            post.transform.localScale = scale;
            post.GetComponent<Renderer>().material = lineWhite;
        }

        private void CreateNetLine(string name, Vector3 position, Vector3 scale)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = name;
            line.transform.SetParent(worldRoot.transform, false);
            line.transform.position = position;
            line.transform.localScale = scale;
            line.GetComponent<Renderer>().material = netMaterial;
        }

        private FriendlyRuntime CreateFriendly(FriendlyDef def)
        {
            FriendlyRuntime runtime = new FriendlyRuntime();
            runtime.def = def;
            runtime.root = new GameObject("Friendly_" + def.id + "_" + def.number);
            runtime.root.transform.SetParent(worldRoot.transform, false);
            runtime.root.transform.position = def.position;
            runtime.root.transform.rotation = Quaternion.Euler(0f, def.fixedYaw, 0f);
            runtime.lateralPhase = def.behavior == ActiveBehaviorType.TargetMove ? Mathf.Repeat(def.targetMovePhase, 1f) : 0f;
            runtime.movementDirection = 1f;
            runtime.movementPauseMarker = -1;

            Material kitMaterial = def.isTargetTen ? playerJerseyMaterial : friendlyBlue;
            Material hairStyleMaterial = def.isTargetTen ? playerHairMaterial : hairMaterial;
            runtime.body = CreateBlockPart(runtime.root.transform, "Body", new Vector3(0f, 0.62f, 0f), new Vector3(0.46f, 0.7f, 0.3f), kitMaterial).transform;
            if (def.isTargetTen)
            {
                CreateBlockPart(runtime.root.transform, "Number_10_Backplate_Trim", new Vector3(0f, 0.72f, -0.18f), new Vector3(0.46f, 0.46f, 0.018f), playerAccessoryMaterial);
                CreateBlockPart(runtime.root.transform, "Number_10_Backplate", new Vector3(0f, 0.72f, -0.2f), new Vector3(0.38f, 0.38f, 0.018f), blackMaterial);
            }
            else
            {
                CreateBlockPart(runtime.root.transform, "Kit_Stripe", new Vector3(0f, 0.64f, -0.165f), new Vector3(0.18f, 0.7f, 0.025f), friendlyWhite);
            }
            CreateBlockPart(runtime.root.transform, "Shorts", new Vector3(0f, 0.25f, 0f), new Vector3(0.48f, 0.22f, 0.32f), blackMaterial);
            runtime.head = CreateBlockPart(runtime.root.transform, "Head", new Vector3(0f, 1.08f, 0f), new Vector3(0.34f, 0.34f, 0.34f), skinMaterial).transform;
            CreateBlockPart(runtime.root.transform, "Hair", new Vector3(0f, 1.28f, -0.01f), new Vector3(0.38f, 0.1f, 0.34f), hairStyleMaterial);
            runtime.leftArm = CreateBlockPart(runtime.root.transform, "Left_Arm", new Vector3(-0.34f, 0.62f, 0f), new Vector3(0.13f, 0.56f, 0.16f), skinMaterial).transform;
            runtime.rightArm = CreateBlockPart(runtime.root.transform, "Right_Arm", new Vector3(0.34f, 0.62f, 0f), new Vector3(0.13f, 0.56f, 0.16f), skinMaterial).transform;
            runtime.leftLeg = CreateBlockPart(runtime.root.transform, "Left_Leg", new Vector3(-0.14f, -0.02f, 0f), new Vector3(0.14f, 0.38f, 0.16f), skinMaterial).transform;
            runtime.rightLeg = CreateBlockPart(runtime.root.transform, "Right_Leg", new Vector3(0.14f, -0.02f, 0f), new Vector3(0.14f, 0.38f, 0.16f), skinMaterial).transform;
            CreateBlockPart(runtime.root.transform, "Left_Boot", new Vector3(-0.14f, -0.24f, -0.03f), new Vector3(0.18f, 0.1f, 0.22f), blackMaterial);
            CreateBlockPart(runtime.root.transform, "Right_Boot", new Vector3(0.14f, -0.24f, -0.03f), new Vector3(0.18f, 0.1f, 0.22f), blackMaterial);
            if (def.isTargetTen)
            {
                CosmeticItem accessory = GameSession.GetEquipped(CosmeticCategory.Accessory);
                if (accessory != null && accessory.id == "accessory_captain")
                {
                    CreateBlockPart(runtime.root.transform, "Captain_Band", new Vector3(-0.4f, 0.77f, -0.01f), new Vector3(0.07f, 0.12f, 0.18f), playerAccessoryMaterial);
                }
                else if (accessory != null && accessory.id == "accessory_shades")
                {
                    CreateBlockPart(runtime.root.transform, "Goal_Shades", new Vector3(0f, 1.1f, -0.19f), new Vector3(0.38f, 0.09f, 0.03f), playerAccessoryMaterial);
                }
                else if (accessory != null && accessory.id == "accessory_crown")
                {
                    CreateBlockPart(runtime.root.transform, "Golden_Crown", new Vector3(0f, 1.4f, -0.01f), new Vector3(0.42f, 0.12f, 0.3f), playerAccessoryMaterial);
                }
                else if (accessory != null && (accessory.id == "accessory_wrist" || accessory.id == "accessory_headband"))
                {
                    CreateBlockPart(runtime.root.transform, "Style_Band", new Vector3(-0.38f, accessory.id == "accessory_headband" ? 1.21f : 0.75f, -0.01f), new Vector3(0.1f, 0.11f, 0.18f), playerAccessoryMaterial);
                }
                else if (accessory != null && (accessory.id == "accessory_mask" || accessory.id == "accessory_visor"))
                {
                    CreateBlockPart(runtime.root.transform, "Face_Style", new Vector3(0f, 1.08f, -0.19f), new Vector3(0.38f, 0.09f, 0.03f), playerAccessoryMaterial);
                }
                else if (accessory != null && accessory.id == "accessory_wings")
                {
                    CreateBlockPart(runtime.root.transform, "Wing_Left", new Vector3(-0.45f, 0.78f, 0.06f), new Vector3(0.36f, 0.12f, 0.1f), playerAccessoryMaterial);
                    CreateBlockPart(runtime.root.transform, "Wing_Right", new Vector3(0.45f, 0.78f, 0.06f), new Vector3(0.36f, 0.12f, 0.1f), playerAccessoryMaterial);
                }
                else if (accessory != null)
                {
                    CreateBlockPart(runtime.root.transform, "Style_Badge", new Vector3(0.2f, 0.72f, -0.18f), new Vector3(0.12f, 0.12f, 0.025f), playerAccessoryMaterial);
                }
            }
            runtime.idleSeed = friendlies.Count * 0.73f + def.number * 0.11f;

            runtime.numberText = new GameObject("Number_" + def.number).AddComponent<TextMesh>();
            runtime.numberText.transform.SetParent(runtime.root.transform, false);
            runtime.numberText.transform.localPosition = def.isTargetTen ? new Vector3(0f, 0.73f, -0.225f) : new Vector3(0f, 0.72f, -0.17f);
            runtime.numberText.text = def.number.ToString();
            runtime.numberText.font = uiFont;
            runtime.numberText.fontSize = def.isTargetTen ? 68 : 42;
            runtime.numberText.characterSize = def.isTargetTen ? 0.036f : 0.028f;
            runtime.numberText.anchor = TextAnchor.MiddleCenter;
            runtime.numberText.alignment = TextAlignment.Center;
            runtime.numberText.color = def.isTargetTen ? new Color(1f, 0.86f, 0.02f) : Color.white;

            if (def.isTargetTen)
            {
                TextMesh shadow = new GameObject("Number_10_Shadow").AddComponent<TextMesh>();
                shadow.transform.SetParent(runtime.numberText.transform, false);
                shadow.transform.localPosition = new Vector3(0.018f, -0.018f, 0.012f);
                shadow.text = runtime.numberText.text;
                shadow.font = uiFont;
                shadow.fontSize = runtime.numberText.fontSize;
                shadow.characterSize = runtime.numberText.characterSize;
                shadow.anchor = TextAnchor.MiddleCenter;
                shadow.alignment = TextAlignment.Center;
                shadow.color = new Color(0f, 0f, 0f, 0.92f);
            }

            runtime.activeRing = CreateRing("Active_Ring_" + def.id, new Color(0f, 1f, 0.25f, 0.5f), 0.09f, 0.62f);
            runtime.activeRing.gameObject.SetActive(false);
            runtime.targetRing = CreateRing("Target_10_Ring_" + def.id, new Color(1f, 0.88f, 0.05f, 0.5f), 0.08f, def.isTargetTen ? 0.86f : 0.62f);
            runtime.targetRing.gameObject.SetActive(def.isTargetTen);

            return runtime;
        }

        private OpponentRuntime CreateOpponent(OpponentDef def)
        {
            OpponentRuntime runtime = new OpponentRuntime();
            runtime.def = def;
            runtime.root = new GameObject(def.isBlondeHeader ? "Opponent_Blonde_Header" : "Opponent");
            runtime.root.transform.SetParent(worldRoot.transform, false);
            runtime.root.transform.position = def.position;

            runtime.body = CreateBlockPart(runtime.root.transform, "Body", new Vector3(0f, 0.55f, 0f), new Vector3(0.48f, 0.72f, 0.32f), opponentRed).transform;
            CreateBlockPart(runtime.root.transform, "Shorts", new Vector3(0f, 0.22f, 0f), new Vector3(0.5f, 0.2f, 0.34f), blackMaterial);
            runtime.head = CreateBlockPart(runtime.root.transform, "Head", new Vector3(0f, 1.04f, 0f), new Vector3(0.34f, 0.34f, 0.34f), skinMaterial).transform;
            CreateBlockPart(runtime.root.transform, "Hair", new Vector3(0f, 1.24f, -0.01f), new Vector3(0.38f, 0.1f, 0.34f), def.isBlondeHeader ? goldMaterial : hairMaterial);
            runtime.leftArm = CreateBlockPart(runtime.root.transform, "Left_Arm", new Vector3(-0.34f, 0.56f, 0f), new Vector3(0.13f, 0.54f, 0.16f), skinMaterial).transform;
            runtime.rightArm = CreateBlockPart(runtime.root.transform, "Right_Arm", new Vector3(0.34f, 0.56f, 0f), new Vector3(0.13f, 0.54f, 0.16f), skinMaterial).transform;
            runtime.leftLeg = CreateBlockPart(runtime.root.transform, "Left_Leg", new Vector3(-0.14f, -0.04f, 0f), new Vector3(0.14f, 0.36f, 0.16f), skinMaterial).transform;
            runtime.rightLeg = CreateBlockPart(runtime.root.transform, "Right_Leg", new Vector3(0.14f, -0.04f, 0f), new Vector3(0.14f, 0.36f, 0.16f), skinMaterial).transform;
            CreateBlockPart(runtime.root.transform, "Left_Boot", new Vector3(-0.14f, -0.25f, -0.03f), new Vector3(0.18f, 0.1f, 0.22f), blackMaterial);
            CreateBlockPart(runtime.root.transform, "Right_Boot", new Vector3(0.14f, -0.25f, -0.03f), new Vector3(0.18f, 0.1f, 0.22f), blackMaterial);
            runtime.idleSeed = opponents.Count * 0.61f + def.position.sqrMagnitude * 0.07f;
            runtime.lateralPhase = def.lateralPhase;
            runtime.movementDirection = 1f;
            runtime.movementPauseMarker = -1;

            runtime.ring = CreateRing("Opponent_Ring", def.isBlondeHeader ? new Color(1f, 0.78f, 0.12f, 0.55f) : new Color(1f, 0.1f, 0.06f, 0.5f), 0.075f, def.isBlondeHeader ? 0.74f : 0.66f);
            return runtime;
        }

        private void CreateSoccerBallPattern()
        {
            Vector3[] panelNormals =
            {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right,
                Vector3.up,
                Vector3.down,
                new Vector3(0.62f, 0.46f, 0.64f),
                new Vector3(-0.62f, 0.46f, 0.64f),
                new Vector3(0.62f, -0.46f, 0.64f),
                new Vector3(-0.62f, -0.46f, 0.64f),
                new Vector3(0.64f, 0.42f, -0.64f),
                new Vector3(-0.64f, -0.42f, -0.64f)
            };

            for (int i = 0; i < panelNormals.Length; i++)
            {
                CreateBallPanel("Ball_Black_Panel_" + i, panelNormals[i].normalized, i < 6 ? 0.22f : 0.17f, blackMaterial);
            }

            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI * 2f / 10f;
                Vector3 normal = new Vector3(Mathf.Cos(angle) * 0.78f, Mathf.Sin(angle * 1.7f) * 0.28f, Mathf.Sin(angle) * 0.78f).normalized;
                CreateBallPanel("Ball_Seam_Dimple_" + i, normal, 0.055f, softLineWhite);
            }
        }

        private void CreateBallPanel(string name, Vector3 normal, float radius, Material material)
        {
            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            patch.name = name;
            patch.transform.SetParent(ball.transform, false);
            patch.transform.localPosition = normal * 0.512f;
            patch.transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
            patch.transform.localScale = new Vector3(radius, 0.018f, radius);
            patch.GetComponent<Renderer>().material = material;
        }

        private void CreateLevelGems()
        {
            for (int i = 0; i < level.gemPositions.Count; i++)
            {
                Vector3 position = level.gemPositions[i];
                GemRuntime runtime = new GemRuntime();
                runtime.position = position;
                runtime.idleOffset = i * 0.83f + level.number * 0.21f;
                runtime.root = new GameObject("Pass_Gem_" + (i + 1));
                runtime.root.transform.SetParent(worldRoot.transform, false);
                runtime.root.transform.position = position;

                GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
                core.name = "Gem_Core";
                core.transform.SetParent(runtime.root.transform, false);
                core.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
                core.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);
                core.GetComponent<Renderer>().material = gemMaterial;

                GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                glow.name = "Gem_Glow";
                glow.transform.SetParent(runtime.root.transform, false);
                glow.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);
                glow.GetComponent<Renderer>().material = goldMaterial;
                gems.Add(runtime);
            }
        }

        private void UpdateGemPresentation()
        {
            for (int i = 0; i < gems.Count; i++)
            {
                GemRuntime gem = gems[i];
                if (gem.collected || gem.root == null)
                {
                    continue;
                }

                float phase = Time.time * 2.6f + gem.idleOffset;
                gem.root.transform.position = gem.position + Vector3.up * (0.14f + Mathf.Sin(phase) * 0.06f);
                gem.root.transform.Rotate(Vector3.up, 110f * Time.deltaTime, Space.World);
            }
        }

        private void TryCollectGems(Vector3 previousBallPosition, Vector3 currentBallPosition)
        {
            for (int i = 0; i < gems.Count; i++)
            {
                GemRuntime gem = gems[i];
                if (gem.collected || gem.root == null)
                {
                    continue;
                }

                if (DistanceToSegment(gem.root.transform.position, previousBallPosition, currentBallPosition) > GemCollectRadius)
                {
                    continue;
                }

                gem.collected = true;
                gem.root.SetActive(false);
                gemsCollectedThisLevel++;
                GameSession.CollectGems(1);
                UpdateWalletText();
                CreateBurstParticles("Gem_Collect", gem.root.transform.position, new Color(0.10f, 0.92f, 1f), 14, 0.08f, 1.5f);
            }
        }

        private void CreateReceiverTargetMarker()
        {
            receiverTargetRing = CreateRing("Receiver_Target_Ring", new Color(1f, 0.86f, 0.08f, 0.5f), 0.055f, 0.5f);
            receiverTargetRing.gameObject.SetActive(false);
        }

        private static GameObject CreateBlockPart(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().material = material;
            return part;
        }

        private void BuildHud()
        {
            GameObject canvasObject = new GameObject("Generated_Gameplay_HUD");
            hudCanvas = canvasObject.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            RectTransform root = canvasObject.GetComponent<RectTransform>();

            RectTransform levelBadge = CreateArtPanel(root, "Level_Badge", UiPanelFill, UiPanelBorder, true);
            SetAnchor(levelBadge, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(164f, -76f), new Vector2(276f, 68f));

            levelLabel = CreateText(levelBadge, "LEVEL " + level.number, 38, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetAnchor(levelLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform walletBadge = CreateArtPanel(root, "Gem_Wallet", UiPanelFill, new Color(0.18f, 0.94f, 1f, 0.52f), true);
            SetAnchor(walletBadge, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-178f, -76f), new Vector2(292f, 68f));
            walletText = CreateText(walletBadge, string.Empty, 31, FontStyle.Bold, new Color(0.35f, 0.94f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(walletText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UpdateWalletText();

            RectTransform goalCard = CreateArtPanel(root, "Goal_Card", new Color(0.02f, 0.08f, 0.13f, 0.76f), UiPanelBorder, true);
            SetAnchor(goalCard, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -166f), new Vector2(300f, 114f));

            Text goalTitle = CreateText(goalCard, "OBJECTIVE", 23, FontStyle.Bold, new Color(0.68f, 0.88f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(goalTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(250f, 36f));

            Text goalText = CreateText(goalCard, "PASS TO #10", 30, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetAnchor(goalText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(250f, 46f));

            RectTransform howToCard = CreateArtPanel(root, "How_To_Play_Card", new Color(0.02f, 0.11f, 0.07f, 0.72f), new Color(0.86f, 1f, 0.64f, 0.36f), true);
            // Reserve the lower screen area for the future ad banner without covering the aiming controls.
            SetAnchor(howToCard, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(250f, 300f), new Vector2(460f, 164f));

            Text howToTitle = CreateText(howToCard, "TIP", 22, FontStyle.Bold, new Color(0.68f, 0.88f, 1f), TextAnchor.MiddleLeft);
            SetAnchor(howToTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -26f), new Vector2(60f, 32f));

            promptText = CreateText(howToCard, "HOLD TO AIM  |  RELEASE TO PASS\nPULL BACK FOR A LOB", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetAnchor(promptText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 10f), new Vector2(-32f, -48f));

            Button pauseButton = CreateButton(root, "II", UiPanelFill, Color.white, 36);
            SetAnchor(pauseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-62f, -68f), new Vector2(64f, 60f));
            pauseButton.onClick.AddListener(Pause);

            Button levelsButton = CreateButton(root, "LEVELS", UiButtonBlue, Color.white, 34);
            SetAnchor(levelsButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-126f, -134f), new Vector2(196f, 64f));
            levelsButton.onClick.AddListener(ShowLevelSelect);

            RectTransform powerPanel = CreateArtPanel(root, "Power_Meter", new Color(0f, 0f, 0f, 0.58f), new Color(0.86f, 1f, 0.68f, 0.34f), true);
            SetAnchor(powerPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(580f, 68f));

            GameObject fillObject = new GameObject("Power_Fill");
            fillObject.transform.SetParent(powerPanel, false);
            powerFill = fillObject.AddComponent<Image>();
            powerFill.color = new Color(0.15f, 1f, 0.24f, 0.9f);
            powerFill.type = Image.Type.Filled;
            powerFill.fillMethod = Image.FillMethod.Horizontal;
            powerFill.fillAmount = 0f;
            SetAnchor(powerFill.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-28f, -18f));

            powerText = CreateText(powerPanel, "POWER 0%", 29, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetAnchor(powerText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            powerPanel.gameObject.SetActive(false);

            BuildPauseOverlay(root);
            BuildLevelSelectOverlay(root);
            BuildResultOverlay(root);
            BuildCelebrationUi(root);
        }

        private void BuildPauseOverlay(RectTransform root)
        {
            pauseOverlay = new GameObject("Pause_Overlay");
            pauseOverlay.transform.SetParent(root, false);
            Image scrim = pauseOverlay.AddComponent<Image>();
            scrim.color = new Color(0f, 0f, 0f, 0.54f);
            SetAnchor(pauseOverlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform card = CreateArtPanel(pauseOverlay.transform, "Pause_Card", new Color(0.02f, 0.06f, 0.12f, 0.96f), UiPanelBorder, true);
            SetAnchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 640f));

            Text title = CreateText(card, "PAUSED", 56, FontStyle.Bold, UiTitleGold, TextAnchor.MiddleCenter);
            SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(560f, 100f));

            Button resume = CreateButton(card, "RESUME", UiButtonGreen, Color.white, 38);
            SetAnchor(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 132f), new Vector2(440f, 88f));
            resume.onClick.AddListener(Resume);

            Button retry = CreateButton(card, "RETRY", UiButtonBlue, Color.white, 38);
            SetAnchor(retry.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 24f), new Vector2(440f, 88f));
            retry.onClick.AddListener(Retry);

            Button levels = CreateButton(card, "LEVELS", UiButtonBlue, Color.white, 38);
            SetAnchor(levels.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -84f), new Vector2(440f, 88f));
            levels.onClick.AddListener(ShowLevelSelect);

            Button menu = CreateButton(card, "MENU", UiButtonDark, Color.white, 38);
            SetAnchor(menu.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -192f), new Vector2(440f, 88f));
            menu.onClick.AddListener(SceneLoader.LoadMainMenu);

            pauseOverlay.SetActive(false);
        }

        private void BuildLevelSelectOverlay(RectTransform root)
        {
            levelSelectOverlay = new GameObject("Level_Select_Overlay");
            levelSelectOverlay.transform.SetParent(root, false);
            Image scrim = levelSelectOverlay.AddComponent<Image>();
            scrim.color = new Color(0f, 0f, 0f, 0.58f);
            SetAnchor(levelSelectOverlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform card = CreateArtPanel(levelSelectOverlay.transform, "Level_Select_Card", new Color(0.02f, 0.06f, 0.12f, 0.96f), UiPanelBorder, true);
            SetAnchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 1280f));

            Text title = CreateText(card, "SELECT LEVEL", 54, FontStyle.Bold, UiTitleGold, TextAnchor.MiddleCenter);
            SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(700f, 100f));

            Text hint = CreateText(card, "Tap a level to jump in", 34, FontStyle.Bold, new Color(0.82f, 0.94f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -164f), new Vector2(700f, 68f));

            for (int i = 0; i < GameSession.LevelCount; i++)
            {
                int levelIndex = i;
                int column = i % 3;
                int row = i / 3;
                bool current = i == GameSession.SelectedLevelIndex;
                Color color = current ? UiButtonGreen : UiButtonBlue;
                Button levelButton = CreateButton(card, "LEVEL " + (i + 1), color, Color.white, 27);
                float x = (column - 1) * 250f;
                float y = 360f - row * 84f;
                SetAnchor(levelButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(214f, 68f));
                levelButton.onClick.AddListener(delegate { SelectLevelFromOverlay(levelIndex); });
            }

            Button close = CreateButton(card, "CLOSE", UiButtonDark, Color.white, 36);
            SetAnchor(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 86f), new Vector2(430f, 82f));
            close.onClick.AddListener(CloseLevelSelect);

            levelSelectOverlay.SetActive(false);
        }

        private void BuildResultOverlay(RectTransform root)
        {
            resultOverlay = new GameObject("Result_Overlay");
            resultOverlay.transform.SetParent(root, false);
            Image scrim = resultOverlay.AddComponent<Image>();
            scrim.color = new Color(0f, 0f, 0f, 0.54f);
            SetAnchor(resultOverlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform card = CreateArtPanel(resultOverlay.transform, "Result_Card", new Color(0.02f, 0.06f, 0.12f, 0.96f), UiPanelBorder, true);
            SetAnchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 650f));

            resultTitle = CreateText(card, "LEVEL COMPLETE", 54, FontStyle.Bold, UiTitleGold, TextAnchor.MiddleCenter);
            SetAnchor(resultTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(660f, 92f));

            resultRewardLabel = CreateText(card, "MATCH REWARDS", 28, FontStyle.Bold, new Color(0.68f, 0.88f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(resultRewardLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -154f), new Vector2(660f, 48f));

            resultReason = CreateText(card, "+100 GOLD", 48, FontStyle.Bold, new Color(1f, 0.76f, 0.12f), TextAnchor.MiddleCenter);
            SetAnchor(resultReason.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -236f), new Vector2(660f, 120f));

            resultGemReward = CreateText(card, "+1 GEM", 42, FontStyle.Bold, new Color(0.20f, 0.92f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(resultGemReward.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -286f), new Vector2(660f, 64f));

            resultNextButton = CreateButton(card, "NEXT", UiButtonGreen, Color.white, 38);
            SetAnchor(resultNextButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(450f, 88f));
            resultNextButton.onClick.AddListener(NextLevel);

            Button retry = CreateButton(card, "RETRY", UiButtonBlue, Color.white, 38);
            SetAnchor(retry.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(450f, 88f));
            retry.onClick.AddListener(Retry);

            Button menu = CreateButton(card, "MENU", UiButtonDark, Color.white, 38);
            SetAnchor(menu.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -270f), new Vector2(450f, 88f));
            menu.onClick.AddListener(SceneLoader.LoadMainMenu);

            resultOverlay.SetActive(false);
        }

        private void BuildCelebrationUi(RectTransform root)
        {
            celebrationUiRoot = new GameObject("Celebration_Foreground_VFX", typeof(RectTransform));
            celebrationUiRoot.transform.SetParent(root, false);
            SetAnchor(celebrationUiRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Color[] shardColors =
            {
                new Color(0.12f, 0.84f, 1f, 0.95f),
                new Color(1f, 0.84f, 0.16f, 0.95f),
                new Color(1f, 1f, 1f, 0.95f),
                new Color(0.18f, 1f, 0.35f, 0.95f),
                new Color(1f, 0.24f, 0.24f, 0.95f)
            };

            celebrationUiPieces.Clear();
            celebrationUiVelocities.Clear();
            celebrationUiSpin.Clear();

            for (int i = 0; i < 72; i++)
            {
                GameObject piece = new GameObject("Confetti_" + i);
                piece.transform.SetParent(celebrationUiRoot.transform, false);
                Image image = piece.AddComponent<Image>();
                image.color = shardColors[i % shardColors.Length];
                image.raycastTarget = false;

                RectTransform rect = piece.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = i % 3 == 0 ? new Vector2(18f, 46f) : new Vector2(14f, 34f);
                celebrationUiPieces.Add(rect);
                celebrationUiVelocities.Add(Vector2.zero);
                celebrationUiSpin.Add(0f);
            }

            for (int burst = 0; burst < 3; burst++)
            {
                Vector2 center = new Vector2(-250f + burst * 250f, 350f + (burst % 2) * 120f);
                for (int i = 0; i < 18; i++)
                {
                    GameObject spark = new GameObject("Firework_Spark_" + burst + "_" + i);
                    spark.transform.SetParent(celebrationUiRoot.transform, false);
                    Image image = spark.AddComponent<Image>();
                    image.color = shardColors[(i + burst) % shardColors.Length];
                    image.raycastTarget = false;

                    RectTransform rect = spark.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(12f, 12f);
                    float angle = Mathf.PI * 2f * i / 18f;
                    rect.anchoredPosition = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (54f + burst * 18f);
                    celebrationUiPieces.Add(rect);
                    celebrationUiVelocities.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (70f + burst * 16f));
                    celebrationUiSpin.Add((i % 2 == 0 ? 1f : -1f) * 180f);
                }
            }

            celebrationUiRoot.SetActive(false);
        }

        private void StartCelebrationUi()
        {
            if (celebrationUiRoot == null)
            {
                return;
            }

            celebrationUiRoot.SetActive(true);
            celebrationUiActive = true;
            for (int i = 0; i < celebrationUiPieces.Count; i++)
            {
                RectTransform rect = celebrationUiPieces[i];
                if (i < 72)
                {
                    float x = -500f + (i % 18) * 58f + Mathf.Sin(i * 1.17f) * 30f;
                    float y = 720f + (i / 18) * 86f;
                    rect.anchoredPosition = new Vector2(x, y);
                    rect.localRotation = Quaternion.Euler(0f, 0f, i * 19f);
                    celebrationUiVelocities[i] = new Vector2(Mathf.Sin(i * 2.3f) * 135f, -380f - (i % 7) * 28f);
                    celebrationUiSpin[i] = (i % 2 == 0 ? 1f : -1f) * (180f + (i % 5) * 36f);
                }
                else
                {
                    rect.localScale = Vector3.one;
                }

                Image image = rect.GetComponent<Image>();
                if (image != null)
                {
                    Color color = image.color;
                    color.a = 0.98f;
                    image.color = color;
                }
            }
        }

        private void StopCelebrationUi()
        {
            celebrationUiActive = false;
            if (celebrationUiRoot != null)
            {
                celebrationUiRoot.SetActive(false);
            }
        }

        private void UpdateCelebrationUi(float deltaTime)
        {
            if (!celebrationUiActive || celebrationUiRoot == null)
            {
                return;
            }

            for (int i = 0; i < celebrationUiPieces.Count; i++)
            {
                RectTransform rect = celebrationUiPieces[i];
                Vector2 velocity = celebrationUiVelocities[i];
                if (i < 72)
                {
                    velocity += Vector2.down * 230f * deltaTime;
                    if (rect.anchoredPosition.y < -760f)
                    {
                        rect.anchoredPosition = new Vector2(-520f + (i % 18) * 62f, 760f + (i % 4) * 64f);
                        velocity = new Vector2(Mathf.Sin((Time.unscaledTime + i) * 1.7f) * 120f, -360f - (i % 6) * 32f);
                    }
                }
                else
                {
                    Image image = rect.GetComponent<Image>();
                    if (image != null)
                    {
                        Color color = image.color;
                        color.a = Mathf.PingPong(Time.unscaledTime * 1.8f + i * 0.13f, 0.75f) + 0.2f;
                        image.color = color;
                    }
                }

                rect.anchoredPosition += velocity * deltaTime;
                rect.localRotation *= Quaternion.Euler(0f, 0f, celebrationUiSpin[i] * deltaTime);
                celebrationUiVelocities[i] = velocity;
            }
        }

        private void StartWorldCelebrationVfx()
        {
            if (worldRoot == null)
            {
                return;
            }

            CreateBurstParticles("Goal_Confetti_Left", level.goalPosition + new Vector3(-2.6f, 1.1f, 0.25f), new Color(1f, 0.86f, 0.16f), 82, 0.11f, 4.6f);
            CreateBurstParticles("Goal_Confetti_Right", level.goalPosition + new Vector3(2.6f, 1.1f, 0.25f), new Color(0.12f, 0.86f, 1f), 82, 0.11f, 4.6f);
            CreateBurstParticles("Goal_Firework_Gold", level.goalPosition + new Vector3(-1.7f, 3.2f, 1.2f), new Color(1f, 0.72f, 0.08f), 42, 0.16f, 3.2f);
            CreateBurstParticles("Goal_Firework_Blue", level.goalPosition + new Vector3(1.8f, 3.55f, 1.1f), new Color(0.2f, 0.82f, 1f), 42, 0.16f, 3.2f);
        }

        private void CreateBurstParticles(string objectName, Vector3 position, Color color, int count, float size, float speed)
        {
            GameObject particleObject = new GameObject(objectName);
            particleObject.transform.SetParent(worldRoot.transform, false);
            particleObject.transform.position = position;

            ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.duration = 2.4f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.55f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.55f, size);
            main.startColor = color;
            main.gravityModifier = 0.45f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 220;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.22f;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = MakeUnlitMaterial(objectName + "_Material", color);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(count);
        }

        private void BeginAim()
        {
            state = GameplayState.Aiming;
            charge = 0f;
            lobAmount = 0f;
            lockedLobAmount = 0f;
            aimStartScreenPosition = GetPointerScreenPosition();
            currentAimScreenPosition = aimStartScreenPosition;
            lockedAimDirection = Flatten(friendlies[activeFriendlyIndex].root.transform.forward).normalized;
            if (lockedAimDirection.sqrMagnitude < 0.001f)
            {
                lockedAimDirection = Vector3.forward;
            }
        }

        private void CancelAim()
        {
            state = GameplayState.Playing;
            charge = 0f;
            lobAmount = 0f;
            lockedLobAmount = 0f;
            HideDashedPreview();
            SetPowerVisible(false);
        }

        private void LaunchPass()
        {
            state = GameplayState.BallTraveling;
            ballDirection = lockedAimDirection.normalized;
            lockedLobAmount = lobAmount;
            ballTravelStart = ball.transform.position;
            ballTravelDistance = 0f;
            ballMaxDistance = Mathf.Lerp(level.minTravelDistance, level.maxTravelDistance, Mathf.Clamp01(charge));
            ballLobHeight = GetLobHeight(lockedLobAmount);
            InitializeBallBouncePhysics();
            friendlies[activeFriendlyIndex].kickTimer = 0.38f;
            StadiumAudio.PlayPassKick();
            HideDashedPreview();
            SetPowerVisible(false);
        }

        private float GetChargeDuration()
        {
            return Mathf.Clamp(level.chargeTime * 0.88f, 0.62f, 0.92f);
        }

        private void InitializeBallBouncePhysics()
        {
            float power = Mathf.Clamp01(charge);
            ballIsLob = lockedLobAmount >= LobActivationThreshold;
            ballGroundHeight = ballTravelStart.y;
            ballVerticalOffset = 0f;

            float horizontalSpeed = level.ballSpeed * Mathf.Lerp(0.9f, 1.08f, power);
            float expectedFlightTime = Mathf.Max(0.2f, ballMaxDistance / Mathf.Max(0.1f, horizontalSpeed));
            float launchApex = ballIsLob ? ballLobHeight : Mathf.Lerp(0.03f, 0.16f, power);
            ballGravity = ballIsLob ? (8f * launchApex) / (expectedFlightTime * expectedFlightTime) : Mathf.Lerp(13.5f, 18.5f, power);
            ballVerticalVelocity = Mathf.Sqrt(Mathf.Max(0.01f, 2f * ballGravity * launchApex));
            ballRestitution = ballIsLob ? 0.58f : Mathf.Lerp(0.22f, 0.42f, power);
            ballCurrentSpeed = horizontalSpeed;
        }

        private void UpdateBallTravel(float deltaTime)
        {
            refereeDeflectionCooldown = Mathf.Max(0f, refereeDeflectionCooldown - deltaTime);
            Vector3 previousBallPosition = ball.transform.position;
            SimulateBallBounce(deltaTime);
            ApplyPostRangeRollingFriction(deltaTime);

            float deltaDistance = ballCurrentSpeed * deltaTime;
            ballTravelDistance += deltaDistance;
            Vector3 nextPosition = ballTravelStart + ballDirection * ballTravelDistance;
            nextPosition.y = ballGroundHeight + ballVerticalOffset;
            ball.transform.position = nextPosition;
            RotateBall(deltaTime);
            TryCollectGems(previousBallPosition, nextPosition);

            Vector3 ballPosition = ball.transform.position;
            TryDeflectBallFromReferee(ballPosition);
            ballPosition = ball.transform.position;

            if (TryBeginDirectGoalkeeperCatch(previousBallPosition, ballPosition))
            {
                return;
            }

            if (TryBeginBlondeHeaderClear(ballPosition))
            {
                return;
            }

            if (IsOutOfBounds(ballPosition))
            {
                Fail(FailReason.OutOfBounds);
                return;
            }

            for (int i = 0; i < opponents.Count; i++)
            {
                if (IsOpponentInterceptHeight(ballPosition.y) && DistanceXZ(ballPosition, opponents[i].root.transform.position) <= level.opponentRadius)
                {
                    FlashObject(opponents[i].root, opponentRed, Color.white);
                    StadiumAudio.PlayOpponentReceive();
                    Fail(FailReason.OpponentIntercepted);
                    return;
                }
            }

            for (int i = 0; i < friendlies.Count; i++)
            {
                if (i == activeFriendlyIndex)
                {
                    continue;
                }

                bool closeEnough = DistanceXZ(ballPosition, friendlies[i].root.transform.position) <= level.receiverRadius;
                bool playableHeight = IsFriendlyBlockHeight(ballPosition.y);
                if (closeEnough && playableHeight)
                {
                    if (IsEligiblePassReceiver(i))
                    {
                        CompletePass(i);
                    }
                    else
                    {
                        Fail(FailReason.PassMissed);
                    }
                    return;
                }
            }

            if (IsBallStoppedOnGround())
            {
                Fail(NextTargetDistance() > ballTravelDistance + level.receiverRadius ? FailReason.NotEnoughPower : FailReason.PassMissed);
            }
        }

        private bool TryBeginBlondeHeaderClear(Vector3 ballPosition)
        {
            if (!ballIsLob || ballPosition.y < BlondeHeaderMinHeight || ballPosition.y > BlondeHeaderMaxHeight)
            {
                return false;
            }

            for (int i = 0; i < opponents.Count; i++)
            {
                OpponentRuntime opponent = opponents[i];
                if (!opponent.def.isBlondeHeader)
                {
                    continue;
                }

                if (DistanceXZ(ballPosition, opponent.root.transform.position) <= BlondeHeaderRadius)
                {
                    BeginHeaderClear(opponent, ballPosition);
                    return true;
                }
            }

            return false;
        }

        private void BeginHeaderClear(OpponentRuntime opponent, Vector3 ballPosition)
        {
            state = GameplayState.HeaderClear;
            headerClearTimer = 0f;
            headerClearOpponent = opponent;
            headerClearOpponentBasePosition = opponent.root.transform.position;
            headerClearBallStart = ballPosition;
            Vector3 clearDirection = Flatten(-ballDirection + Vector3.right * (opponent.root.transform.position.x >= 0f ? 0.45f : -0.45f));
            if (clearDirection.sqrMagnitude < 0.001f)
            {
                clearDirection = Vector3.back;
            }
            headerClearBallEnd = ballPosition + clearDirection.normalized * 1.55f + Vector3.up * 0.5f;
            ballCurrentSpeed = 0f;
            ballVerticalVelocity = 0f;
            opponent.headerJumpTimer = HeaderClearDuration;
            FlashObject(opponent.root, opponentRed, Color.white);
            StadiumAudio.PlayOpponentReceive();
            HideDashedPreview();
            SetPowerVisible(false);
            SetReceiverTargetVisible(false);
        }

        private void UpdateHeaderClear(float deltaTime)
        {
            headerClearTimer += deltaTime;
            float t = Mathf.Clamp01(headerClearTimer / HeaderClearDuration);
            if (headerClearOpponent != null)
            {
                Vector3 jumpOffset = Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.46f;
                headerClearOpponent.root.transform.position = headerClearOpponentBasePosition + jumpOffset;
                headerClearOpponent.root.transform.rotation = Quaternion.LookRotation(Flatten(-ballDirection).sqrMagnitude > 0.001f ? Flatten(-ballDirection).normalized : Vector3.back, Vector3.up);
            }

            if (ball != null)
            {
                ball.transform.position = Vector3.Lerp(headerClearBallStart, headerClearBallEnd, Mathf.SmoothStep(0f, 1f, t));
                ball.transform.Rotate(Vector3.right, 520f * deltaTime, Space.World);
            }

            if (t >= 1f)
            {
                Fail(FailReason.HeaderCleared);
            }
        }

        private bool TryBeginDirectGoalkeeperCatch(Vector3 previousPosition, Vector3 currentPosition)
        {
            if (goalkeeper == null || activeFriendlyIndex < 0 || activeFriendlyIndex >= friendlies.Count)
            {
                return false;
            }

            if (friendlies[activeFriendlyIndex].def.isTargetTen || ballDirection.z <= 0.05f)
            {
                return false;
            }

            float savePlaneZ = level.goalPosition.z - GoalkeeperSavePlaneOffset;
            if (previousPosition.z > savePlaneZ || currentPosition.z < savePlaneZ)
            {
                return false;
            }

            float zDelta = currentPosition.z - previousPosition.z;
            float t = Mathf.Abs(zDelta) <= 0.001f ? 1f : Mathf.Clamp01((savePlaneZ - previousPosition.z) / zDelta);
            Vector3 savePoint = Vector3.Lerp(previousPosition, currentPosition, t);
            Vector3 predictedGoalPoint = PredictGoalMouthPoint(savePoint);
            if (Mathf.Abs(predictedGoalPoint.x - level.goalPosition.x) > GoalCatchHalfWidth || predictedGoalPoint.y > GoalCatchHeight)
            {
                return false;
            }

            BeginGoalkeeperCatch(savePoint, predictedGoalPoint);
            return true;
        }

        private Vector3 PredictGoalMouthPoint(Vector3 savePoint)
        {
            float goalPlaneZ = level.goalPosition.z - 0.08f;
            float zToGoal = Mathf.Max(0f, goalPlaneZ - savePoint.z);
            float travelTime = zToGoal / Mathf.Max(0.1f, ballCurrentSpeed * Mathf.Max(0.05f, ballDirection.z));
            float predictedX = savePoint.x + (ballDirection.x / Mathf.Max(0.05f, ballDirection.z)) * zToGoal;
            float predictedY = savePoint.y + ballVerticalVelocity * travelTime - 0.5f * ballGravity * travelTime * travelTime;
            return new Vector3(predictedX, Mathf.Max(0f, predictedY), goalPlaneZ);
        }

        private void BeginGoalkeeperCatch(Vector3 savePoint, Vector3 predictedGoalPoint)
        {
            state = GameplayState.GoalkeeperCatch;
            goalkeeperCatchTimer = 0f;
            goalkeeperDiveTimer = 0f;
            ballCurrentSpeed = 0f;
            ballVerticalVelocity = 0f;
            ballVerticalOffset = Mathf.Max(0f, ball.transform.position.y - ballGroundHeight);
            goalkeeperCatchStartPosition = goalkeeper.transform.position;

            Vector3 catchPoint = new Vector3(
                Mathf.Clamp(predictedGoalPoint.x, level.goalPosition.x - 1.65f, level.goalPosition.x + 1.65f),
                Mathf.Clamp(savePoint.y, 0.72f, 1.45f),
                savePoint.z - 0.08f);
            goalkeeperCatchTargetPosition = new Vector3(catchPoint.x * 0.88f, Mathf.Max(0.18f, catchPoint.y - 0.72f), catchPoint.z - 0.12f);
            goalkeeperCatchBallLocalPosition = new Vector3(0f, 0.84f, -0.28f);

            HideDashedPreview();
            SetPowerVisible(false);
            SetReceiverTargetVisible(false);
        }

        private void UpdateGoalkeeperCatch(float deltaTime)
        {
            goalkeeperCatchTimer += deltaTime;
            goalkeeperDiveTimer += deltaTime;

            if (goalkeeper != null)
            {
                float dive = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(goalkeeperDiveTimer / 0.58f));
                goalkeeper.transform.position = Vector3.Lerp(goalkeeperCatchStartPosition, goalkeeperCatchTargetPosition, dive);
                float roll = goalkeeperCatchTargetPosition.x >= goalkeeperCatchStartPosition.x ? -82f : 82f;
                goalkeeper.transform.rotation = Quaternion.Euler(0f, 180f, Mathf.Lerp(0f, roll, dive));
                ApplyGoalkeeperCatchPose(dive);

                if (ball != null)
                {
                    Vector3 holdPosition = goalkeeper.transform.TransformPoint(goalkeeperCatchBallLocalPosition);
                    ball.transform.position = Vector3.Lerp(ball.transform.position, holdPosition, 1f - Mathf.Exp(-deltaTime * 18f));
                    ball.transform.Rotate(Vector3.up, 240f * Mathf.Max(0f, 1f - dive) * deltaTime, Space.World);
                }
            }

            if (goalkeeperCatchTimer >= DirectGoalCatchDelay)
            {
                Fail(FailReason.GoalkeeperCaught);
            }
        }

        private void ApplyGoalkeeperCatchPose(float catchBlend)
        {
            if (goalkeeperLeftArm != null)
            {
                goalkeeperLeftArm.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 74f, catchBlend), 0f, Mathf.Lerp(0f, -42f, catchBlend));
            }
            if (goalkeeperRightArm != null)
            {
                goalkeeperRightArm.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 74f, catchBlend), 0f, Mathf.Lerp(0f, 42f, catchBlend));
            }
            if (goalkeeperLeftLeg != null)
            {
                goalkeeperLeftLeg.localRotation = Quaternion.Euler(Mathf.Lerp(0f, -28f, catchBlend), 0f, Mathf.Lerp(0f, -18f, catchBlend));
            }
            if (goalkeeperRightLeg != null)
            {
                goalkeeperRightLeg.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 28f, catchBlend), 0f, Mathf.Lerp(0f, 18f, catchBlend));
            }
        }

        private void ApplyPostRangeRollingFriction(float deltaTime)
        {
            if (ballTravelDistance < ballMaxDistance || !IsBallGroundedForRoll())
            {
                return;
            }

            ballCurrentSpeed = Mathf.MoveTowards(ballCurrentSpeed, 0f, BallPostRangeRollFriction * deltaTime);
        }

        private bool IsBallStoppedOnGround()
        {
            return IsBallGroundedForRoll() && ballCurrentSpeed <= BallStopSpeed;
        }

        private bool IsBallGroundedForRoll()
        {
            return ballVerticalOffset <= 0.001f && Mathf.Abs(ballVerticalVelocity) <= 0.001f;
        }

        private void SimulateBallBounce(float deltaTime)
        {
            ballVerticalVelocity -= ballGravity * deltaTime;
            ballVerticalOffset += ballVerticalVelocity * deltaTime;

            if (ballVerticalOffset > 0f || ballVerticalVelocity >= 0f)
            {
                return;
            }

            float impactSpeed = -ballVerticalVelocity;
            ballVerticalOffset = 0f;
            StadiumAudio.PlayBallBounce(impactSpeed);
            if (impactSpeed < 0.65f || ballRestitution < 0.08f)
            {
                ballVerticalVelocity = 0f;
                ballRestitution = 0f;
                return;
            }

            ballVerticalVelocity = impactSpeed * ballRestitution;
            ballRestitution *= 0.58f;
            ballCurrentSpeed *= Mathf.Lerp(0.82f, 0.93f, Mathf.Clamp01(charge));
        }

        private void RotateBall(float deltaTime)
        {
            Vector3 spinAxis = Vector3.Cross(Vector3.up, Flatten(ballDirection));
            if (spinAxis.sqrMagnitude < 0.001f)
            {
                spinAxis = Vector3.right;
            }

            ball.transform.Rotate(spinAxis.normalized, 720f * Mathf.Clamp01(ballCurrentSpeed / Mathf.Max(0.1f, level.ballSpeed)) * deltaTime, Space.World);
        }

        private void TryDeflectBallFromReferee(Vector3 ballPosition)
        {
            if (referee == null || refereeDeflectionCooldown > 0f || !IsFriendlyBlockHeight(ballPosition.y))
            {
                return;
            }

            if (DistanceXZ(ballPosition, referee.transform.position) > RefereeCollisionRadius)
            {
                return;
            }

            Vector3 away = Flatten(ballPosition - referee.transform.position);
            if (away.sqrMagnitude < 0.001f)
            {
                away = Vector3.Cross(Vector3.up, ballDirection);
            }

            away.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, ballDirection).normalized;
            float sideSign = Vector3.Dot(away, side) >= 0f ? 1f : -1f;
            float deflectAngle = sideSign * Mathf.Lerp(24f, 43f, Mathf.PingPong(level.number * 0.31f + ballTravelDistance * 0.17f, 1f));
            Vector3 motionInfluence = Flatten(refereeMotion);
            Vector3 deflected = Quaternion.Euler(0f, deflectAngle, 0f) * ballDirection + (motionInfluence.sqrMagnitude > 0.001f ? motionInfluence.normalized * 0.22f : Vector3.zero);
            float remainingDistance = Mathf.Max(level.receiverRadius * 1.4f, ballMaxDistance - ballTravelDistance + level.receiverRadius);

            ballDirection = Flatten(deflected).normalized;
            ballTravelStart = ballPosition + ballDirection * 0.08f;
            ballTravelDistance = 0f;
            ballMaxDistance = remainingDistance;
            ballCurrentSpeed *= 0.82f;
            refereeDeflectionCooldown = 0.45f;
            FlashObject(referee, goldMaterial, Color.white);
        }

        private void CompletePass(int receiverIndex)
        {
            activeFriendlyIndex = receiverIndex;
            AttachBallToActive();
            StadiumAudio.PlayTeammateReceive();

            if (friendlies[receiverIndex].def.isTargetTen)
            {
                BeginAutoShot();
            }
            else
            {
                promptText.text = "Good! Pass again.";
                state = GameplayState.Playing;
                charge = 0f;
            }
        }

        private void BeginAutoShot()
        {
            state = GameplayState.AutoShot;
            autoShotStart = ball.transform.position;
            autoShotTimer = 0f;
            autoShotStyle = Random.Range(0, 3);
            autoShotEnd = level.goalPosition + new Vector3(Random.Range(-1.55f, 1.55f), Random.Range(0.48f, 1.62f), 0.42f + Random.Range(-0.08f, 0.22f));
            goalkeeperDiveTimer = 0f;
            friendlies[activeFriendlyIndex].root.transform.LookAt(new Vector3(level.goalPosition.x, friendlies[activeFriendlyIndex].root.transform.position.y, level.goalPosition.z));
            friendlies[activeFriendlyIndex].kickTimer = autoShotStyle == 0 ? 0.64f : 0.54f;
            StadiumAudio.PlayPassKick(1.15f);

            if (goalkeeper != null)
            {
                goalkeeper.transform.position = goalkeeperStartPosition;
                goalkeeper.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                float missSide = autoShotEnd.x >= goalkeeperStartPosition.x ? -1f : 1f;
                goalkeeperDiveTarget = goalkeeperStartPosition + new Vector3(missSide * Random.Range(0.85f, 1.65f), Random.Range(0.36f, 0.82f), Random.Range(-0.02f, 0.18f));
            }

            HideDashedPreview();
            SetPowerVisible(false);
        }

        private void UpdateAutoShot(float deltaTime)
        {
            autoShotTimer += deltaTime;
            goalkeeperDiveTimer += deltaTime;
            float t = Mathf.Clamp01(autoShotTimer / 1.05f);
            Vector3 arc = Vector3.up * Mathf.Sin(t * Mathf.PI) * (autoShotStyle == 0 ? 1.35f : 1.05f);
            Vector3 bend = Vector3.right * Mathf.Sin(t * Mathf.PI) * (autoShotStyle == 2 ? (autoShotEnd.x >= 0f ? 0.35f : -0.35f) : 0f);
            ball.transform.position = Vector3.Lerp(autoShotStart, autoShotEnd, t) + arc + bend;
            Vector3 shotDirection = Flatten(autoShotEnd - autoShotStart).normalized;
            Vector3 spinAxis = autoShotStyle == 2 ? Vector3.up : Vector3.Cross(Vector3.up, shotDirection);
            ball.transform.Rotate(spinAxis.sqrMagnitude > 0.001f ? spinAxis.normalized : Vector3.right, 860f * deltaTime, Space.World);

            if (goalkeeper != null)
            {
                float dive = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(goalkeeperDiveTimer / 0.82f));
                goalkeeper.transform.position = Vector3.Lerp(goalkeeperStartPosition, goalkeeperDiveTarget, dive);
                float roll = goalkeeperDiveTarget.x >= goalkeeperStartPosition.x ? -78f : 78f;
                goalkeeper.transform.rotation = Quaternion.Euler(0f, 180f, Mathf.Lerp(0f, roll, dive));
            }

            if (!goalSlowMotionTriggered && t >= 0.68f)
            {
                goalSlowMotionTriggered = true;
                SetTimeScale(0.28f);
            }

            if (t >= 1f)
            {
                Win();
            }
        }

        private void Fail(FailReason reason)
        {
            state = GameplayState.Failed;
            ResetTimeScale();
            HideDashedPreview();
            SetPowerVisible(false);
            SetReceiverTargetVisible(false);
            StopCelebrationUi();
            resultTitle.text = "TRY AGAIN";
            resultRewardLabel.gameObject.SetActive(false);
            resultGemReward.gameObject.SetActive(false);
            resultReason.text = GetReasonText(reason);
            resultReason.fontSize = 40;
            resultReason.rectTransform.anchoredPosition = new Vector2(0f, -176f);
            resultReason.rectTransform.sizeDelta = new Vector2(660f, 92f);
            resultReason.color = new Color(1f, 0.36f, 0.26f);
            resultNextButton.gameObject.SetActive(false);
            resultOverlay.SetActive(true);
            StadiumAudio.PlayLose();
        }

        private void Win()
        {
            state = GameplayState.Won;
            ResetTimeScale();
            SetReceiverTargetVisible(false);
            int earnedCoins = GameSession.CompleteLevel(level.number);
            resultTitle.text = level.number >= GameSession.LevelCount ? "CUP COMPLETE" : "LEVEL COMPLETE";
            resultRewardLabel.gameObject.SetActive(true);
            resultGemReward.gameObject.SetActive(gemsCollectedThisLevel > 0);
            resultReason.text = "+" + earnedCoins + " GOLD";
            resultReason.fontSize = 48;
            resultReason.color = new Color(1f, 0.76f, 0.12f);
            resultReason.rectTransform.anchoredPosition = new Vector2(0f, gemsCollectedThisLevel > 0 ? -218f : -244f);
            resultReason.rectTransform.sizeDelta = new Vector2(660f, 64f);
            if (gemsCollectedThisLevel > 0)
            {
                resultGemReward.text = "+" + gemsCollectedThisLevel + (gemsCollectedThisLevel == 1 ? " GEM" : " GEMS");
            }
            resultNextButton.gameObject.SetActive(true);
            Text nextText = resultNextButton.GetComponentInChildren<Text>();
            if (nextText != null)
            {
                nextText.text = level.number >= GameSession.LevelCount ? "MENU" : "NEXT";
            }
            resultOverlay.SetActive(true);
            StadiumAudio.PlayGoal();
            StartWorldCelebrationVfx();
            StartCelebrationUi();
        }

        private string GetReasonText(FailReason reason)
        {
            if (reason == FailReason.OpponentIntercepted)
            {
                return "Opponent intercepted";
            }

            if (reason == FailReason.NotEnoughPower)
            {
                return "Not enough power";
            }

            if (reason == FailReason.OutOfBounds)
            {
                return "Out of bounds";
            }

            if (reason == FailReason.GoalkeeperCaught)
            {
                return "Goalkeeper caught it";
            }

            if (reason == FailReason.HeaderCleared)
            {
                return "Header clearance";
            }

            return "Pass missed";
        }

        private void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0.05f, 1f);
            Time.fixedDeltaTime = Mathf.Max(0.001f, defaultFixedDeltaTime * Time.timeScale);
        }

        private void ResetTimeScale()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = defaultFixedDeltaTime > 0f ? defaultFixedDeltaTime : 0.02f;
        }

        private void Retry()
        {
            ResetTimeScale();
            StartLevel(GameSession.SelectedLevelIndex);
        }

        private void NextLevel()
        {
            ResetTimeScale();
            if (GameSession.SelectNextLevel())
            {
                StartLevel(GameSession.SelectedLevelIndex);
                return;
            }

            SceneLoader.LoadMainMenu();
        }

        private void Pause()
        {
            if (state == GameplayState.Won || state == GameplayState.Failed || state == GameplayState.AutoShot || state == GameplayState.GoalkeeperCatch || state == GameplayState.HeaderClear)
            {
                return;
            }

            if (state == GameplayState.Aiming)
            {
                CancelAim();
                stateBeforePause = GameplayState.Playing;
            }
            else
            {
                stateBeforePause = state;
            }

            state = GameplayState.Paused;
            pauseOverlay.SetActive(true);
            if (levelSelectOverlay != null)
            {
                levelSelectOverlay.SetActive(false);
            }
            SetPowerVisible(false);
            HideDashedPreview();
        }

        private void Resume()
        {
            pauseOverlay.SetActive(false);
            state = stateBeforePause == GameplayState.Paused ? GameplayState.Playing : stateBeforePause;
        }

        private void ShowLevelSelect()
        {
            if (levelSelectOverlay == null || state == GameplayState.AutoShot || state == GameplayState.GoalkeeperCatch || state == GameplayState.HeaderClear)
            {
                return;
            }

            if (state != GameplayState.Paused)
            {
                if (state == GameplayState.Aiming)
                {
                    CancelAim();
                    stateBeforePause = GameplayState.Playing;
                }
                else
                {
                    stateBeforePause = state;
                }
            }

            state = GameplayState.Paused;
            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(false);
            }
            SetPowerVisible(false);
            HideDashedPreview();
            levelSelectOverlay.SetActive(true);
        }

        private void CloseLevelSelect()
        {
            if (levelSelectOverlay != null)
            {
                levelSelectOverlay.SetActive(false);
            }

            state = stateBeforePause == GameplayState.Paused ? GameplayState.Playing : stateBeforePause;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && state == GameplayState.Aiming)
            {
                CancelAim();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && state == GameplayState.Aiming)
            {
                CancelAim();
            }
        }

        private void SelectLevelFromOverlay(int levelIndex)
        {
            if (levelSelectOverlay != null)
            {
                levelSelectOverlay.SetActive(false);
            }

            StartLevel(levelIndex);
        }

#if UNITY_EDITOR
        private void HandleEditorLevelSelectShortcuts()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (levelSelectOverlay != null && levelSelectOverlay.activeSelf)
                {
                    CloseLevelSelect();
                }
                else
                {
                    ShowLevelSelect();
                }
            }

            if (levelSelectOverlay == null || !levelSelectOverlay.activeSelf)
            {
                return;
            }

            int shortcutCount = Mathf.Min(10, GameSession.LevelCount);
            for (int i = 0; i < shortcutCount; i++)
            {
                KeyCode key = i == 9 ? KeyCode.Alpha0 : KeyCode.Alpha1 + i;
                if (Input.GetKeyDown(key))
                {
                    SelectLevelFromOverlay(i);
                    return;
                }
            }
        }
#endif

        private void UpdateActiveBehavior(float deltaTime)
        {
            FriendlyRuntime active = friendlies[activeFriendlyIndex];
            FriendlyDef def = active.def;

            if (def.behavior == ActiveBehaviorType.AutoRotate)
            {
                float t = (Mathf.Sin(Time.time * def.sweepSpeed) + 1f) * 0.5f;
                float yaw = Mathf.Lerp(def.sweepMinYaw, def.sweepMaxYaw, t);
                active.root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            else if (def.behavior == ActiveBehaviorType.LateralMove)
            {
                active.lateralPhase += deltaTime * Mathf.Max(0.1f, def.lateralSpeed);
                float t = Mathf.PingPong(active.lateralPhase, 1f);
                active.root.transform.position = Vector3.Lerp(def.lateralStart, def.lateralEnd, t);
                active.root.transform.rotation = Quaternion.Euler(0f, def.fixedYaw, 0f);
                AttachBallToActive();
            }
            else if (def.behavior == ActiveBehaviorType.TargetMove)
            {
                UpdateTargetFriendlyMovement(active, deltaTime);
                AttachBallToActive();
            }
            else
            {
                active.root.transform.rotation = Quaternion.Euler(0f, def.fixedYaw, 0f);
            }
        }

        private void UpdateFriendlyMovement(float deltaTime)
        {
            for (int i = 0; i < friendlies.Count; i++)
            {
                if (i == activeFriendlyIndex)
                {
                    continue;
                }

                FriendlyRuntime friendly = friendlies[i];
                if (friendly.lightningStruck)
                {
                    continue;
                }

                FriendlyDef def = friendly.def;
                if (def.behavior == ActiveBehaviorType.LateralMove)
                {
                    friendly.lateralPhase += deltaTime * Mathf.Max(0.1f, def.lateralSpeed);
                    float t = (Mathf.Sin(friendly.lateralPhase) + 1f) * 0.5f;
                    friendly.root.transform.position = Vector3.Lerp(def.lateralStart, def.lateralEnd, t);
                    friendly.root.transform.rotation = Quaternion.Euler(0f, def.fixedYaw, 0f);
                }
                else if (def.behavior == ActiveBehaviorType.TargetMove)
                {
                    UpdateTargetFriendlyMovement(friendly, deltaTime);
                }
                else if (def.behavior == ActiveBehaviorType.AutoRotate)
                {
                    float t = (Mathf.Sin(Time.time * def.sweepSpeed + friendly.idleSeed) + 1f) * 0.5f;
                    float yaw = Mathf.Lerp(def.sweepMinYaw, def.sweepMaxYaw, t);
                    friendly.root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                }
            }
        }

        private void UpdateTargetFriendlyMovement(FriendlyRuntime runtime, float deltaTime)
        {
            if (runtime.lightningStruck)
            {
                return;
            }

            FriendlyDef def = runtime.def;
            if (runtime.movementPauseTimer > 0f)
            {
                runtime.movementPauseTimer = Mathf.Max(0f, runtime.movementPauseTimer - deltaTime);
                runtime.root.transform.position = GetTargetMovePosition(def, runtime.lateralPhase);
                runtime.root.transform.rotation = Quaternion.Euler(0f, def.fixedYaw, 0f);
                return;
            }

            if (def.targetMovePattern == 2)
            {
                runtime.lateralPhase = Mathf.Repeat(runtime.lateralPhase + deltaTime * Mathf.Max(0.08f, def.lateralSpeed), 1f);
                int marker = Mathf.FloorToInt(runtime.lateralPhase * 4f);
                if (runtime.movementPauseMarker >= 0 && marker != runtime.movementPauseMarker)
                {
                    runtime.movementPauseTimer = def.targetMovePause;
                }
                runtime.movementPauseMarker = marker;
            }
            else
            {
                runtime.lateralPhase += deltaTime * Mathf.Max(0.08f, def.lateralSpeed) * runtime.movementDirection;
                if (runtime.lateralPhase >= 1f)
                {
                    runtime.lateralPhase = 1f;
                    runtime.movementDirection = -1f;
                    runtime.movementPauseTimer = def.targetMovePause;
                }
                else if (runtime.lateralPhase <= 0f)
                {
                    runtime.lateralPhase = 0f;
                    runtime.movementDirection = 1f;
                    runtime.movementPauseTimer = def.targetMovePause;
                }
            }

            runtime.root.transform.position = GetTargetMovePosition(def, runtime.lateralPhase);
            runtime.root.transform.rotation = Quaternion.Euler(0f, def.fixedYaw, 0f);
        }

        private static Vector3 GetTargetMovePosition(FriendlyDef def, float phase)
        {
            if (def.targetMovePattern == 2)
            {
                Vector3 center = (def.lateralStart + def.lateralEnd) * 0.5f;
                float width = Mathf.Abs(def.lateralEnd.x - def.lateralStart.x) * 0.5f;
                float depth = Mathf.Abs(def.lateralEnd.z - def.lateralStart.z) * 0.5f;
                float angle = Mathf.Repeat(phase, 1f) * Mathf.PI * 2f;
                return center + new Vector3(Mathf.Sin(angle) * width, 0f, Mathf.Sin(angle * 2f) * depth);
            }

            return Vector3.Lerp(def.lateralStart, def.lateralEnd, Mathf.Clamp01(phase));
        }

        private void UpdateOpponentMovement(float deltaTime)
        {
            for (int i = 0; i < opponents.Count; i++)
            {
                OpponentRuntime opponent = opponents[i];
                OpponentDef def = opponent.def;
                if (def.isBlondeHeader && def.lateralSpeed > 0.01f && def.targetMovePause > 0f)
                {
                    UpdateBlondeHeaderMovement(opponent, deltaTime);
                    continue;
                }

                if (def.lateralSpeed <= 0.01f)
                {
                    continue;
                }

                Vector3 previous = opponent.root.transform.position;
                opponent.lateralPhase += deltaTime * def.lateralSpeed;
                float t = (Mathf.Sin(opponent.lateralPhase) + 1f) * 0.5f;
                Vector3 next = Vector3.Lerp(def.lateralStart, def.lateralEnd, t);
                opponent.root.transform.position = next;

                Vector3 motion = next - previous;
                if (motion.sqrMagnitude > 0.0001f)
                {
                    opponent.root.transform.rotation = Quaternion.LookRotation(Flatten(motion).normalized, Vector3.up);
                }
            }
        }

        private void UpdateBlondeHeaderMovement(OpponentRuntime runtime, float deltaTime)
        {
            OpponentDef def = runtime.def;
            Vector3 previous = runtime.root.transform.position;

            if (runtime.movementPauseTimer > 0f)
            {
                runtime.movementPauseTimer = Mathf.Max(0f, runtime.movementPauseTimer - deltaTime);
            }
            else if (def.targetMovePattern == 2)
            {
                runtime.lateralPhase = Mathf.Repeat(runtime.lateralPhase + deltaTime * Mathf.Max(0.08f, def.lateralSpeed), 1f);
                int marker = Mathf.FloorToInt(runtime.lateralPhase * 4f);
                if (runtime.movementPauseMarker >= 0 && marker != runtime.movementPauseMarker)
                {
                    runtime.movementPauseTimer = def.targetMovePause;
                }
                runtime.movementPauseMarker = marker;
            }
            else
            {
                runtime.lateralPhase += deltaTime * Mathf.Max(0.08f, def.lateralSpeed) * runtime.movementDirection;
                if (runtime.lateralPhase >= 1f)
                {
                    runtime.lateralPhase = 1f;
                    runtime.movementDirection = -1f;
                    runtime.movementPauseTimer = def.targetMovePause;
                }
                else if (runtime.lateralPhase <= 0f)
                {
                    runtime.lateralPhase = 0f;
                    runtime.movementDirection = 1f;
                    runtime.movementPauseTimer = def.targetMovePause;
                }
            }

            runtime.root.transform.position = GetOpponentMovePosition(def, runtime.lateralPhase);
            Vector3 motion = runtime.root.transform.position - previous;
            if (motion.sqrMagnitude > 0.0001f)
            {
                runtime.root.transform.rotation = Quaternion.LookRotation(Flatten(motion).normalized, Vector3.up);
            }
        }

        private static Vector3 GetOpponentMovePosition(OpponentDef def, float phase)
        {
            if (def.targetMovePattern == 2)
            {
                Vector3 center = (def.lateralStart + def.lateralEnd) * 0.5f;
                float width = Mathf.Abs(def.lateralEnd.x - def.lateralStart.x) * 0.5f;
                float depth = Mathf.Abs(def.lateralEnd.z - def.lateralStart.z) * 0.5f;
                float angle = Mathf.Repeat(phase, 1f) * Mathf.PI * 2f;
                return center + new Vector3(Mathf.Sin(angle) * width, 0f, Mathf.Sin(angle * 2f) * depth);
            }

            return Vector3.Lerp(def.lateralStart, def.lateralEnd, Mathf.Clamp01(phase));
        }

        private void UpdateReferee(float deltaTime)
        {
            if (referee == null)
            {
                return;
            }

            float t = Time.time * Mathf.Max(0.2f, refereeRunSpeed) + refereePhaseOffset;
            Vector3 previous = referee.transform.position;
            Vector3 center = ScaleGameplayPosition(new Vector3(-0.45f, 0f, -0.2f));
            Vector3 offset = new Vector3(Mathf.Sin(t * 0.78f) * 3.1f, 0f, Mathf.Sin(t * 1.17f + 0.8f) * 4.3f);
            Vector3 next = center + offset;
            next.x = Mathf.Clamp(next.x, level.fieldMin.x + 1.2f, level.fieldMax.x - 1.2f);
            next.z = Mathf.Clamp(next.z, level.fieldMin.y + 1.1f, level.fieldMax.y - 1.5f);
            referee.transform.position = Vector3.Lerp(previous, next, Mathf.Clamp01(deltaTime * (3.6f + refereeRunSpeed * 1.25f)));

            Vector3 motion = referee.transform.position - previous;
            refereeMotion = deltaTime > 0f ? motion / deltaTime : Vector3.zero;
            if (motion.sqrMagnitude > 0.0001f)
            {
                referee.transform.rotation = Quaternion.LookRotation(Flatten(motion).normalized, Vector3.up);
            }

            float stride = Mathf.Sin(t * 8.5f) * (22f + refereeRunSpeed * 8f);
            if (refereeLeftArm != null)
            {
                refereeLeftArm.localRotation = Quaternion.Euler(stride, 0f, 0f);
            }
            if (refereeRightArm != null)
            {
                refereeRightArm.localRotation = Quaternion.Euler(-stride, 0f, 0f);
            }
            if (refereeLeftLeg != null)
            {
                refereeLeftLeg.localRotation = Quaternion.Euler(-stride * 0.85f, 0f, 0f);
            }
            if (refereeRightLeg != null)
            {
                refereeRightLeg.localRotation = Quaternion.Euler(stride * 0.85f, 0f, 0f);
            }
        }

        private void UpdateAimingVisuals(bool isCharging)
        {
            if (friendlies.Count == 0 || aimArrow == null)
            {
                return;
            }

            FriendlyRuntime active = friendlies[activeFriendlyIndex];
            Vector3 origin = active.root.transform.position + Vector3.up * 0.12f;
            Vector3 direction = isCharging ? lockedAimDirection : Flatten(active.root.transform.forward).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            if (isCharging)
            {
                UpdateLobAmount(origin, direction);
            }
            else
            {
                lobAmount = 0f;
            }

            UpdateAimArrow(origin + direction * 0.45f, direction, isCharging);
            aimArrow.startColor = isCharging ? Color.white : new Color(0.65f, 1f, 0.7f, 0.75f);
            aimArrow.endColor = aimArrow.startColor;

            if (isCharging)
            {
                SetPowerVisible(true);
                powerFill.fillAmount = charge;
                powerText.text = "POWER " + Mathf.RoundToInt(charge * 100f) + "%  " + (lobAmount >= LobActivationThreshold ? "CHIP" : "GROUND");
                HideDashedPreview();
            }
            else
            {
                SetPowerVisible(false);
                HideDashedPreview();
            }
        }

        private void UpdateAimArrow(Vector3 origin, Vector3 direction, bool isCharging)
        {
            if (aimArrow == null)
            {
                return;
            }

            float distance = isCharging
                ? Mathf.Lerp(1.45f, level.maxTravelDistance * 0.82f, Mathf.Clamp01(charge))
                : 1.25f;
            float lobHeight = isCharging ? GetLobHeight(lobAmount) : 0f;
            bool curved = lobHeight > 0.08f;
            int bodyPoints = curved ? 18 : 2;
            int pointCount = bodyPoints + 3;
            aimArrow.positionCount = pointCount;

            Vector3 lastBodyPoint = origin;
            for (int i = 0; i < bodyPoints; i++)
            {
                float t = bodyPoints <= 1 ? 1f : i / (bodyPoints - 1f);
                Vector3 point = origin + direction * (distance * t) + Vector3.up * (curved ? GetArcHeight(t, lobHeight) : 0f);
                aimArrow.SetPosition(i, point);
                lastBodyPoint = point;
            }

            Vector3 flatBack = -direction.normalized;
            Vector3 headLeft = lastBodyPoint + Quaternion.Euler(0f, -28f, 0f) * flatBack * 0.48f;
            Vector3 headRight = lastBodyPoint + Quaternion.Euler(0f, 28f, 0f) * flatBack * 0.48f;
            aimArrow.SetPosition(bodyPoints, headLeft);
            aimArrow.SetPosition(bodyPoints + 1, lastBodyPoint);
            aimArrow.SetPosition(bodyPoints + 2, headRight);
        }

        private void UpdateDashedPreview(Vector3 origin, Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f)
            {
                HideDashedPreview();
                return;
            }

            direction = direction.normalized;
            float distance = Mathf.Lerp(level.minTravelDistance, level.maxTravelDistance, Mathf.Clamp01(charge));
            float lobHeight = GetLobHeight(lobAmount);
            const float dashLength = 0.42f;
            const float gapLength = 0.26f;
            float stride = dashLength + gapLength;
            int segmentCount = Mathf.Clamp(Mathf.FloorToInt(distance / stride), 3, MaxDashedPreviewSegments);

            for (int i = 0; i < segmentCount; i++)
            {
                LineRenderer line = GetDashedPreviewLine(i);
                float startDistance = i == segmentCount - 1 ? Mathf.Max(0f, distance - dashLength) : i * stride;
                float endDistance = i == segmentCount - 1 ? distance : Mathf.Min(startDistance + dashLength, distance);
                float startT = Mathf.Clamp01(startDistance / distance);
                float endT = Mathf.Clamp01(endDistance / distance);
                Vector3 start = origin + direction * startDistance + Vector3.up * (0.05f + GetArcHeight(startT, lobHeight));
                Vector3 end = origin + direction * endDistance + Vector3.up * (0.05f + GetArcHeight(endT, lobHeight));
                line.positionCount = 2;
                line.SetPosition(0, start);
                line.SetPosition(1, end);
                line.gameObject.SetActive(true);
            }

            for (int i = segmentCount; i < dashedPreviewSegments.Count; i++)
            {
                dashedPreviewSegments[i].SetActive(false);
            }
        }

        private void HideDashedPreview()
        {
            for (int i = 0; i < dashedPreviewSegments.Count; i++)
            {
                dashedPreviewSegments[i].SetActive(false);
            }

            if (aimCurve != null)
            {
                aimCurve.positionCount = 0;
                aimCurve.gameObject.SetActive(false);
            }
        }

        private LineRenderer GetDashedPreviewLine(int index)
        {
            while (dashedPreviewLines.Count <= index)
            {
                LineRenderer line = CreateLine("Pass_Dash_" + dashedPreviewLines.Count, new Color(1f, 1f, 1f, 0.9f), 0.085f, false);
                dashedPreviewLines.Add(line);
                dashedPreviewSegments.Add(line.gameObject);
            }

            return dashedPreviewLines[index];
        }

        private void UpdateLobAmount(Vector3 origin, Vector3 direction)
        {
            currentAimScreenPosition = GetPointerScreenPosition();
            Vector2 backDirection = GetScreenBackDirection(origin, direction);
            Vector2 drag = currentAimScreenPosition - aimStartScreenPosition;
            float pullBack = Mathf.Max(0f, Vector2.Dot(drag, backDirection));
            float referencePixels = Mathf.Max(180f, Mathf.Min(Screen.width, Screen.height) * 0.28f);
            lobAmount = Mathf.Clamp01(pullBack / referencePixels);
        }

        private Vector2 GetScreenBackDirection(Vector3 origin, Vector3 direction)
        {
            if (gameplayCamera == null)
            {
                return Vector2.down;
            }

            Vector2 originScreen = gameplayCamera.WorldToScreenPoint(origin);
            Vector2 forwardScreen = gameplayCamera.WorldToScreenPoint(origin + direction.normalized * 2f);
            Vector2 backDirection = originScreen - forwardScreen;
            if (backDirection.sqrMagnitude < 0.001f)
            {
                return Vector2.down;
            }

            return backDirection.normalized;
        }

        private void UpdateAimCurve(Vector3 origin, Vector3 direction, float distance, float lobHeight)
        {
            if (aimCurve == null)
            {
                return;
            }

            const int pointCount = 24;
            aimCurve.gameObject.SetActive(true);
            aimCurve.positionCount = pointCount;
            for (int i = 0; i < pointCount; i++)
            {
                float t = i / (pointCount - 1f);
                Vector3 position = origin + direction * (t * distance) + Vector3.up * (0.06f + GetArcHeight(t, lobHeight));
                aimCurve.SetPosition(i, position);
            }
        }

        private static float GetLobHeight(float amount)
        {
            if (amount < LobActivationThreshold)
            {
                return 0f;
            }

            float normalizedPullBack = Mathf.InverseLerp(LobActivationThreshold, 1f, Mathf.Clamp01(amount));
            return Mathf.Lerp(MinLobHeight, MaxLobHeight, normalizedPullBack);
        }

        private static float GetArcHeight(float normalizedDistance, float height)
        {
            return Mathf.Sin(Mathf.Clamp01(normalizedDistance) * Mathf.PI) * height;
        }

        private static bool IsOpponentInterceptHeight(float ballHeight)
        {
            return ballHeight <= 1.05f;
        }

        private static bool IsFriendlyBlockHeight(float ballHeight)
        {
            return ballHeight <= 1.35f;
        }

        private void SetPowerVisible(bool visible)
        {
            if (powerFill != null)
            {
                powerFill.transform.parent.gameObject.SetActive(visible);
            }
        }

        private void UpdateReceiverTargetMarker()
        {
            if (receiverTargetRing == null || friendlies.Count == 0)
            {
                return;
            }

            bool visible = state == GameplayState.Playing || state == GameplayState.Aiming || state == GameplayState.BallTraveling;
            int receiverIndex = GetSuggestedForwardReceiverIndex();
            if (!visible || receiverIndex < 0)
            {
                SetReceiverTargetVisible(false);
                return;
            }

            FriendlyRuntime receiver = friendlies[receiverIndex];
            SetRingPosition(receiverTargetRing, receiver.root.transform.position);
            SetReceiverTargetVisible(true);
        }

        private int GetSuggestedForwardReceiverIndex()
        {
            if (friendlies.Count == 0 || activeFriendlyIndex < 0 || activeFriendlyIndex >= friendlies.Count)
            {
                return -1;
            }

            Vector3 direction = GetCurrentAimDirection();
            Vector3 activePosition = friendlies[activeFriendlyIndex].root.transform.position;
            int bestIndex = -1;
            float bestScore = float.MinValue;

            for (int i = 0; i < friendlies.Count; i++)
            {
                if (!IsEligiblePassReceiver(i))
                {
                    continue;
                }

                Vector3 toReceiver = Flatten(friendlies[i].root.transform.position - activePosition);
                float distance = Mathf.Max(0.01f, toReceiver.magnitude);
                float alignment = Vector3.Dot(direction, toReceiver / distance);
                float forwardDelta = friendlies[i].root.transform.position.z - activePosition.z;
                float score = alignment * 3f + forwardDelta * 0.05f - distance * 0.08f;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private bool IsEligiblePassReceiver(int receiverIndex)
        {
            if (receiverIndex < 0 || receiverIndex >= friendlies.Count || receiverIndex == activeFriendlyIndex)
            {
                return false;
            }

            if (friendlies[receiverIndex].lightningStruck)
            {
                return false;
            }

            float activeZ = friendlies[activeFriendlyIndex].root.transform.position.z;
            float receiverZ = friendlies[receiverIndex].root.transform.position.z;
            return receiverZ > activeZ + 0.35f;
        }

        private Vector3 GetCurrentAimDirection()
        {
            Vector3 direction = Vector3.forward;
            if (state == GameplayState.Aiming)
            {
                direction = lockedAimDirection;
            }
            else if (state == GameplayState.BallTraveling)
            {
                direction = ballDirection;
            }
            else if (activeFriendlyIndex >= 0 && activeFriendlyIndex < friendlies.Count)
            {
                direction = Flatten(friendlies[activeFriendlyIndex].root.transform.forward);
            }

            direction = Flatten(direction);
            return direction.sqrMagnitude < 0.001f ? Vector3.forward : direction.normalized;
        }

        private void SetReceiverTargetVisible(bool visible)
        {
            if (receiverTargetBall != null)
            {
                receiverTargetBall.SetActive(visible);
            }

            if (receiverTargetRing != null)
            {
                receiverTargetRing.gameObject.SetActive(visible);
            }
        }

        private Vector3 GetBallControlPoint(FriendlyRuntime runtime)
        {
            return runtime.root.transform.position + runtime.root.transform.forward * 0.34f + Vector3.up * 0.16f;
        }

        private void AttachBallToActive()
        {
            if (ball == null || friendlies.Count == 0)
            {
                return;
            }

            ball.transform.position = GetBallControlPoint(friendlies[activeFriendlyIndex]);

            for (int i = 0; i < friendlies.Count; i++)
            {
                friendlies[i].activeRing.gameObject.SetActive(i == activeFriendlyIndex);
                friendlies[i].targetRing.gameObject.SetActive(friendlies[i].def.isTargetTen);
            }
        }

        private void UpdateCharacterAnimation(float deltaTime)
        {
            for (int i = 0; i < friendlies.Count; i++)
            {
                FriendlyRuntime runtime = friendlies[i];
                runtime.kickTimer = Mathf.Max(0f, runtime.kickTimer - deltaTime);
                if (runtime.lightningStruck)
                {
                    ApplyLightningStruckPose(runtime);
                    continue;
                }

                float idle = Mathf.Sin((Time.time + runtime.idleSeed) * 3.4f);
                float breathe = 1f + idle * 0.018f;
                float kick = runtime.kickTimer > 0f ? Mathf.Sin((runtime.kickTimer / 0.46f) * Mathf.PI) : 0f;

                if (runtime.body != null)
                {
                    runtime.body.localPosition = new Vector3(0f, 0.62f + idle * 0.018f, -0.02f * kick);
                    runtime.body.localRotation = Quaternion.Euler(7f * kick, 0f, 0f);
                    runtime.body.localScale = new Vector3(0.46f, 0.7f * breathe, 0.3f);
                }

                if (runtime.head != null)
                {
                    runtime.head.localPosition = new Vector3(0f, 1.08f + idle * 0.014f, 0f);
                    runtime.head.localRotation = Quaternion.Euler(-5f * kick, 0f, 0f);
                }

                if (runtime.leftArm != null)
                {
                    runtime.leftArm.localRotation = Quaternion.Euler(-8f * idle - 18f * kick, 0f, -4f);
                }

                if (runtime.rightArm != null)
                {
                    runtime.rightArm.localRotation = Quaternion.Euler(8f * idle + 24f * kick, 0f, 4f);
                }

                if (runtime.leftLeg != null)
                {
                    runtime.leftLeg.localRotation = Quaternion.Euler(5f * idle, 0f, -2f);
                }

                if (runtime.rightLeg != null)
                {
                    runtime.rightLeg.localRotation = Quaternion.Euler(-4f * idle - 58f * kick, 0f, 2f);
                }

                if (state == GameplayState.AutoShot && i == activeFriendlyIndex)
                {
                    ApplyAutoShotPose(runtime, Mathf.Clamp01(autoShotTimer / 1.05f));
                }
            }

            Vector3 focus = ball != null ? ball.transform.position : Vector3.zero;
            for (int i = 0; i < opponents.Count; i++)
            {
                OpponentRuntime runtime = opponents[i];
                Vector3 lookTarget = new Vector3(focus.x, runtime.root.transform.position.y, focus.z);
                Vector3 lookDirection = lookTarget - runtime.root.transform.position;
                if (runtime.def.lateralSpeed <= 0.01f && lookDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                    runtime.root.transform.rotation = Quaternion.Slerp(runtime.root.transform.rotation, targetRotation, deltaTime * 4.5f);
                }

                float idle = Mathf.Sin((Time.time + runtime.idleSeed) * 4.2f);
                float guard = Mathf.Sin((Time.time + runtime.idleSeed) * 6.1f);
                if (runtime.body != null)
                {
                    runtime.body.localPosition = new Vector3(0f, 0.55f + idle * 0.022f, 0f);
                    runtime.body.localRotation = Quaternion.Euler(0f, 0f, guard * 2.5f);
                }

                if (runtime.head != null)
                {
                    runtime.head.localPosition = new Vector3(0f, 1.04f + idle * 0.014f, 0f);
                    runtime.head.localRotation = Quaternion.Euler(0f, guard * 5f, 0f);
                }

                if (runtime.leftArm != null)
                {
                    runtime.leftArm.localRotation = Quaternion.Euler(-18f + idle * 18f, 0f, -10f);
                }

                if (runtime.rightArm != null)
                {
                    runtime.rightArm.localRotation = Quaternion.Euler(-18f - idle * 18f, 0f, 10f);
                }

                if (runtime.leftLeg != null)
                {
                    runtime.leftLeg.localRotation = Quaternion.Euler(guard * 9f, 0f, -3f);
                }

                if (runtime.rightLeg != null)
                {
                    runtime.rightLeg.localRotation = Quaternion.Euler(-guard * 9f, 0f, 3f);
                }

                if (runtime.headerJumpTimer > 0f)
                {
                    runtime.headerJumpTimer = Mathf.Max(0f, runtime.headerJumpTimer - deltaTime);
                    float header = Mathf.Sin(Mathf.Clamp01(runtime.headerJumpTimer / HeaderClearDuration) * Mathf.PI);
                    if (runtime.body != null)
                    {
                        runtime.body.localRotation = Quaternion.Euler(22f * header, 0f, guard * 2.5f);
                    }
                    if (runtime.head != null)
                    {
                        runtime.head.localPosition = new Vector3(0f, 1.08f + header * 0.16f, -0.08f * header);
                        runtime.head.localRotation = Quaternion.Euler(28f * header, guard * 5f, 0f);
                    }
                    if (runtime.leftArm != null)
                    {
                        runtime.leftArm.localRotation = Quaternion.Euler(-48f * header, 0f, -36f);
                    }
                    if (runtime.rightArm != null)
                    {
                        runtime.rightArm.localRotation = Quaternion.Euler(-48f * header, 0f, 36f);
                    }
                }
            }
        }

        private static void ApplyLightningStruckPose(FriendlyRuntime runtime)
        {
            runtime.root.transform.rotation = Quaternion.Euler(0f, runtime.def.fixedYaw, 0f);

            if (runtime.body != null)
            {
                runtime.body.localPosition = new Vector3(0f, 0.2f, 0f);
                runtime.body.localRotation = Quaternion.Euler(0f, 0f, 86f);
                runtime.body.localScale = new Vector3(0.46f, 0.7f, 0.3f);
            }

            if (runtime.head != null)
            {
                runtime.head.localPosition = new Vector3(0.42f, 0.23f, 0.02f);
                runtime.head.localRotation = Quaternion.Euler(0f, -12f, 74f);
            }

            if (runtime.leftArm != null)
            {
                runtime.leftArm.localRotation = Quaternion.Euler(8f, 0f, -82f);
            }

            if (runtime.rightArm != null)
            {
                runtime.rightArm.localRotation = Quaternion.Euler(-16f, 0f, 92f);
            }

            if (runtime.leftLeg != null)
            {
                runtime.leftLeg.localRotation = Quaternion.Euler(0f, 0f, -42f);
            }

            if (runtime.rightLeg != null)
            {
                runtime.rightLeg.localRotation = Quaternion.Euler(0f, 0f, 36f);
            }

            if (runtime.numberText != null)
            {
                runtime.numberText.gameObject.SetActive(false);
            }
        }

        private void ApplyAutoShotPose(FriendlyRuntime runtime, float normalizedTime)
        {
            float strike = Mathf.Sin(Mathf.Clamp01(normalizedTime) * Mathf.PI);
            if (autoShotStyle == 0)
            {
                if (runtime.body != null)
                {
                    runtime.body.localRotation = Quaternion.Euler(24f * strike, 0f, 0f);
                }
                if (runtime.head != null)
                {
                    runtime.head.localPosition = new Vector3(0f, 1.02f + strike * 0.08f, -0.08f * strike);
                    runtime.head.localRotation = Quaternion.Euler(34f * strike, 0f, 0f);
                }
                if (runtime.leftArm != null)
                {
                    runtime.leftArm.localRotation = Quaternion.Euler(-42f * strike, 0f, -18f);
                }
                if (runtime.rightArm != null)
                {
                    runtime.rightArm.localRotation = Quaternion.Euler(-42f * strike, 0f, 18f);
                }
                return;
            }

            if (autoShotStyle == 1)
            {
                if (runtime.body != null)
                {
                    runtime.body.localRotation = Quaternion.Euler(-18f * strike, 0f, 9f * strike);
                }
                if (runtime.leftLeg != null)
                {
                    runtime.leftLeg.localRotation = Quaternion.Euler(18f * strike, 0f, -12f);
                }
                if (runtime.rightLeg != null)
                {
                    runtime.rightLeg.localRotation = Quaternion.Euler(-112f * strike, 0f, 14f);
                }
                if (runtime.leftArm != null)
                {
                    runtime.leftArm.localRotation = Quaternion.Euler(28f * strike, 0f, -38f);
                }
                if (runtime.rightArm != null)
                {
                    runtime.rightArm.localRotation = Quaternion.Euler(-24f * strike, 0f, 36f);
                }
                return;
            }

            if (runtime.body != null)
            {
                runtime.body.localRotation = Quaternion.Euler(8f * strike, 24f * strike, -12f * strike);
            }
            if (runtime.leftArm != null)
            {
                runtime.leftArm.localRotation = Quaternion.Euler(-18f * strike, 0f, -42f);
            }
            if (runtime.rightArm != null)
            {
                runtime.rightArm.localRotation = Quaternion.Euler(34f * strike, 0f, 46f);
            }
            if (runtime.rightLeg != null)
            {
                runtime.rightLeg.localRotation = Quaternion.Euler(-76f * strike, 18f * strike, 28f);
            }
        }

        private void UpdateCameraFollow(float deltaTime)
        {
            if (gameplayCamera == null)
            {
                return;
            }

            Vector3 targetPosition = baseCameraPosition;
            if (ShouldCameraTrackBall())
            {
                targetPosition = GetGameplayCameraPosition();
            }

            float followRate = state == GameplayState.BallTraveling ? 3.15f : (state == GameplayState.AutoShot ? 3.65f : 4.2f);
            float blend = 1f - Mathf.Exp(-deltaTime * followRate);
            gameplayCamera.transform.position = Vector3.Lerp(gameplayCamera.transform.position, targetPosition, blend);
            gameplayCamera.transform.rotation = baseCameraRotation;
        }

        private bool ShouldCameraTrackBall()
        {
            return ball != null
                && (state == GameplayState.Playing
                    || state == GameplayState.Aiming
                    || state == GameplayState.BallTraveling
                    || state == GameplayState.AutoShot
                    || state == GameplayState.GoalkeeperCatch
                    || state == GameplayState.HeaderClear);
        }

        private Vector3 GetGameplayCameraPosition()
        {
            Vector3 cameraForward = baseCameraRotation * Vector3.forward;
            float forwardDrop = -cameraForward.y;
            if (forwardDrop <= 0.001f)
            {
                return baseCameraPosition;
            }

            Vector3 focus = GetCameraFocusPoint();
            float distanceToFocus = (baseCameraPosition.y - focus.y) / forwardDrop;
            Vector3 position = focus - cameraForward * distanceToFocus;
            if (ShouldUseOpeningCameraOffset())
            {
                Vector3 cameraUp = baseCameraRotation * Vector3.up;
                float halfVerticalSpan = Mathf.Tan(gameplayCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distanceToFocus;
                float offset = (0.5f - FirstPlayerScreenYFromBottom) * 2f * halfVerticalSpan;
                position += cameraUp * offset;
            }

            return position;
        }

        private Vector3 GetCameraFocusPoint()
        {
            if (ShouldUseOpeningCameraOffset() && friendlies.Count > 0)
            {
                return friendlies[0].root.transform.position + Vector3.up * 0.15f;
            }

            if (ball == null)
            {
                return Vector3.zero;
            }

            Vector3 focus = ball.transform.position;
            if (state == GameplayState.BallTraveling)
            {
                focus.y = Mathf.Lerp(0.12f, Mathf.Min(ball.transform.position.y, 0.82f), 0.35f);
                Vector3 travelDirection = Flatten(ballDirection);
                if (travelDirection.sqrMagnitude > 0.001f)
                {
                    focus += travelDirection.normalized * Mathf.Lerp(0.35f, 0.9f, Mathf.Clamp01(charge));
                }

                int receiverIndex = GetSuggestedForwardReceiverIndex();
                if (receiverIndex >= 0)
                {
                    Vector3 receiver = friendlies[receiverIndex].root.transform.position + Vector3.up * 0.12f;
                    focus = Vector3.Lerp(focus, receiver, 0.24f);
                }

                return focus;
            }

            if (state == GameplayState.AutoShot)
            {
                Vector3 goalLead = level.goalPosition + new Vector3(0.35f, 0.36f, 0.24f);
                return Vector3.Lerp(focus, goalLead, 0.2f);
            }

            if (state == GameplayState.GoalkeeperCatch)
            {
                Vector3 keeperFocus = goalkeeper != null ? goalkeeper.transform.position + Vector3.up * 0.55f : focus;
                return Vector3.Lerp(focus, keeperFocus, 0.35f);
            }

            return focus;
        }

        private bool ShouldUseOpeningCameraOffset()
        {
            return activeFriendlyIndex == 0
                && (state == GameplayState.Playing || state == GameplayState.Aiming)
                && friendlies.Count > 0;
        }

        private void SnapCameraToBall()
        {
            if (gameplayCamera == null || ball == null)
            {
                return;
            }

            gameplayCamera.transform.position = GetGameplayCameraPosition();
            gameplayCamera.transform.rotation = baseCameraRotation;
        }

        private void UpdateRingPositions()
        {
            for (int i = 0; i < friendlies.Count; i++)
            {
                SetRingPosition(friendlies[i].activeRing, friendlies[i].root.transform.position);
                SetRingPosition(friendlies[i].targetRing, friendlies[i].root.transform.position);
            }

            for (int i = 0; i < opponents.Count; i++)
            {
                SetRingPosition(opponents[i].ring, opponents[i].root.transform.position);
            }
        }

        private static void SetRingPosition(LineRenderer ring, Vector3 center)
        {
            if (ring == null)
            {
                return;
            }

            ring.transform.position = center + Vector3.up * RingHeight;
        }

        private LineRenderer CreateLine(string name, Color color, float width, bool loop)
        {
            GameObject lineObject = new GameObject(name);
            lineObject.transform.SetParent(worldRoot.transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = loop;
            line.widthMultiplier = width;
            line.material = MakeUnlitMaterial(name + "_Material", color);
            line.startColor = color;
            line.endColor = color;
            line.positionCount = 0;
            return line;
        }

        private LineRenderer CreateRing(string name, Color color, float width, float radius)
        {
            LineRenderer ring = CreateLine(name, color, width, true);
            ring.useWorldSpace = false;
            ring.positionCount = RingSegments;
            ring.SetPositions(GetRingPoints(radius));
            return ring;
        }

        private Vector3[] GetRingPoints(float radius)
        {
            int cacheKey = Mathf.RoundToInt(radius * 1000f);
            Vector3[] points;
            if (ringPointCache.TryGetValue(cacheKey, out points))
            {
                return points;
            }

            points = new Vector3[RingSegments];
            for (int i = 0; i < RingSegments; i++)
            {
                float angle = Mathf.PI * 2f * i / RingSegments;
                points[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            ringPointCache.Add(cacheKey, points);
            return points;
        }

        private static Vector2 GetPointerScreenPosition()
        {
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(0).position;
            }

            return Input.mousePosition;
        }

        private bool HoldStarted()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
            }

            return false;
        }

        private bool HoldReleased()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                return touch.phase == TouchPhase.Ended;
            }

            return Input.GetMouseButtonUp(0);
        }

        private bool HoldCanceled()
        {
            if (Input.touchCount <= 0)
            {
                return false;
            }

            return Input.GetTouch(0).phase == TouchPhase.Canceled;
        }

        private bool IsOutOfBounds(Vector3 position)
        {
            const float margin = 0.9f;
            return position.x < level.fieldMin.x - margin ||
                   position.x > level.fieldMax.x + margin ||
                   position.z < level.fieldMin.y - margin ||
                   position.z > level.fieldMax.y + margin;
        }

        private float NextTargetDistance()
        {
            if (friendlies.Count == 0 || activeFriendlyIndex >= friendlies.Count)
            {
                return 0f;
            }

            float bestDistance = float.MaxValue;
            Vector3 activePosition = friendlies[activeFriendlyIndex].root.transform.position;
            for (int i = 0; i < friendlies.Count; i++)
            {
                if (!IsEligiblePassReceiver(i))
                {
                    continue;
                }

                bestDistance = Mathf.Min(bestDistance, DistanceXZ(activePosition, friendlies[i].root.transform.position));
            }

            return bestDistance == float.MaxValue ? 0f : bestDistance;
        }

        private static Vector3 Flatten(Vector3 value)
        {
            return new Vector3(value.x, 0f, value.z);
        }

        private static float DistanceXZ(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private static float DistanceToSegment(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector3 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared < 0.0001f)
            {
                return Vector3.Distance(point, start);
            }

            float t = Mathf.Clamp01(Vector3.Dot(point - start, segment) / lengthSquared);
            return Vector3.Distance(point, start + segment * t);
        }

        private void UpdateWalletText()
        {
            if (walletText != null)
            {
                walletText.text = GameSession.Gems + " GEM";
            }
        }

        private static T FindSceneObject<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        private void FlashObject(GameObject target, Material original, Color flash)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = Color.Lerp(original.color, flash, 0.45f);
            }
        }

        private void FaceNumberLabelsToCamera()
        {
            Camera camera = gameplayCamera;
            if (camera == null)
            {
                return;
            }

            for (int i = 0; i < friendlies.Count; i++)
            {
                if (friendlies[i].numberText != null)
                {
                    friendlies[i].numberText.transform.rotation = camera.transform.rotation;
                }
            }
        }

        private RectTransform CreateArtPanel(Transform parent, string name, Color fill, Color border, bool shadow)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();

            Color themedFill = Color.Lerp(fill, UiPanelFill, 0.38f);
            themedFill.a = fill.a;
            Color themedBorder = Color.Lerp(border, UiPanelBorder, 0.8f);
            Image face = new GameObject("Panel_Face").AddComponent<Image>();
            face.transform.SetParent(panel.transform, false);
            ConfigureRoundedImage(face, themedFill, themedBorder, 14, 1);
            RectTransform faceRect = face.rectTransform;
            faceRect.anchorMin = Vector2.zero;
            faceRect.anchorMax = Vector2.one;
            faceRect.offsetMin = Vector2.zero;
            faceRect.offsetMax = Vector2.zero;
            face.raycastTarget = false;

            return panelRect;
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
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(16, Mathf.RoundToInt(size * 0.78f));
            text.resizeTextMaxSize = size;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Color background, Color textColor, int fontSize)
        {
            GameObject buttonObject = new GameObject(label + "_Button");
            buttonObject.transform.SetParent(parent, false);
            buttonObject.AddComponent<RectTransform>();

            Image face = new GameObject("Button_Face").AddComponent<Image>();
            face.transform.SetParent(buttonObject.transform, false);
            Color border = new Color(1f, 1f, 1f, 0.2f);
            ConfigureRoundedImage(face, background, border, 12, 1);
            RectTransform faceRect = face.rectTransform;
            faceRect.anchorMin = Vector2.zero;
            faceRect.anchorMax = Vector2.one;
            faceRect.offsetMin = Vector2.zero;
            faceRect.offsetMax = Vector2.zero;

            Button button = buttonObject.AddComponent<Button>();
            buttonObject.AddComponent<UiClickSound>();
            button.targetGraphic = face;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            colors.selectedColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            Text labelText = CreateText(buttonObject.transform, label, fontSize, FontStyle.Bold, textColor, TextAnchor.MiddleCenter);
            SetAnchor(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void ConfigureRoundedImage(Image image, Color fill, Color border, int radius, int borderPixels)
        {
            image.sprite = GetRoundedUiSprite(fill, border, radius, borderPixels);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        private static Sprite GetRoundedUiSprite(Color fill, Color border, int radius, int borderPixels)
        {
            string key = ColorUtility.ToHtmlStringRGBA(fill) + "_" + ColorUtility.ToHtmlStringRGBA(border) + "_" + radius + "_" + borderPixels;
            Sprite sprite;
            if (uiRoundedSpriteCache.TryGetValue(key, out sprite))
            {
                return sprite;
            }

            Texture2D texture = new Texture2D(UiSpriteSize, UiSpriteSize, TextureFormat.RGBA32, false);
            texture.name = "Generated_Rounded_UI_" + key;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            float half = UiSpriteSize * 0.5f;
            float roundedRadius = Mathf.Clamp(radius, 1f, half - 1f);
            float innerHalf = half - roundedRadius - 1f;
            for (int y = 0; y < UiSpriteSize; y++)
            {
                for (int x = 0; x < UiSpriteSize; x++)
                {
                    float px = Mathf.Abs(x + 0.5f - half) - innerHalf;
                    float py = Mathf.Abs(y + 0.5f - half) - innerHalf;
                    float outsideX = Mathf.Max(px, 0f);
                    float outsideY = Mathf.Max(py, 0f);
                    float distance = Mathf.Min(Mathf.Max(px, py), 0f) + Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY) - roundedRadius;
                    float alpha = Mathf.Clamp01(0.5f - distance);
                    Color pixel = borderPixels > 0 && distance > -borderPixels ? border : fill;
                    pixel.a *= alpha;
                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0f, 0f, UiSpriteSize, UiSpriteSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            uiRoundedSpriteCache[key] = sprite;
            return sprite;
        }

        private static void SetAnchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private void CreateMaterials()
        {
            friendlyBlue = MakeMaterial("Friendly_Blue", new Color(0.02f, 0.3f, 0.92f));
            friendlyWhite = MakeMaterial("Friendly_White", Color.white);
            opponentRed = MakeMaterial("Opponent_Red", new Color(0.9f, 0.04f, 0.04f));
            grassA = MakeMaterial("Grass_A", new Color(0.08f, 0.62f, 0.18f));
            grassB = MakeMaterial("Grass_B", new Color(0.1f, 0.72f, 0.22f));
            lineWhite = MakeMaterial("Line_White", Color.white);
            ballMaterial = MakeMaterial("Ball_Generic", new Color(0.96f, 0.96f, 0.9f));
            ballMaterial.mainTexture = CreateBallTexture();
            darkBoard = MakeMaterial("Stadium_Blue_Board", new Color(0.03f, 0.24f, 0.66f));
            crowdBlue = MakeMaterial("Crowd_Blue", new Color(0.06f, 0.24f, 0.76f));
            crowdYellow = MakeMaterial("Crowd_Yellow", new Color(1f, 0.76f, 0.1f));
            skinMaterial = MakeMaterial("Skin_Generic", new Color(0.96f, 0.72f, 0.48f));
            hairMaterial = MakeMaterial("Hair_Dark", new Color(0.08f, 0.045f, 0.025f));
            CosmeticItem playerHair = GameSession.GetEquipped(CosmeticCategory.Hair);
            CosmeticItem playerJersey = GameSession.GetEquipped(CosmeticCategory.Jersey);
            CosmeticItem playerAccessory = GameSession.GetEquipped(CosmeticCategory.Accessory);
            playerHairMaterial = MakeMaterial("Player_Hair_Style", playerHair != null ? playerHair.color : hairMaterial.color);
            playerJerseyMaterial = MakeMaterial("Player_Jersey_Style", playerJersey != null ? playerJersey.color : friendlyBlue.color);
            playerAccessoryMaterial = MakeMaterial("Player_Accessory_Style", playerAccessory != null ? playerAccessory.color : new Color(1f, 0.76f, 0.12f));
            gemMaterial = MakeMaterial("Collectible_Gem", new Color(0.10f, 0.92f, 1f));
            blackMaterial = MakeMaterial("Kit_Black", new Color(0.02f, 0.02f, 0.025f));
            goldMaterial = MakeMaterial("Gold_Accent", new Color(1f, 0.76f, 0.12f));
            netMaterial = MakeMaterial("Goal_Net", new Color(0.75f, 0.92f, 1f));
            softLineWhite = MakeUnlitMaterial("Soft_White_Field_Line", new Color(1f, 1f, 1f, 0.42f));
        }

        private static Material MakeMaterial(string name, Color color)
        {
            return RuntimeMaterialLibrary.Create(name, color);
        }

        private static Material MakeUnlitMaterial(string name, Color color)
        {
            return RuntimeMaterialLibrary.Create(name, color);
        }

        private static Texture2D CreateBallTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Soccer_Ball_Leather_Texture";
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float grain = Mathf.PerlinNoise(x * 0.19f, y * 0.19f) * 0.055f;
                    float seam = Mathf.Min(Mathf.Abs((x % 16) - 8), Mathf.Abs((y % 16) - 8)) < 0.8f ? 0.035f : 0f;
                    Color color = new Color(0.94f - seam + grain, 0.94f - seam + grain, 0.88f - seam + grain, 1f);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private void CreateLevels()
        {
            levels.Clear();
            for (int number = 1; number <= GameSession.LevelCount; number++)
            {
                levels.Add(CreateGeneratedLevel(number));
            }

            for (int i = 0; i < levels.Count; i++)
            {
                ExpandPassAimSweep(levels[i]);
                AddPassRouteGems(levels[i]);
                ApplyPitchScale(levels[i]);
            }
        }

        private static void ExpandPassAimSweep(LevelDef levelDef)
        {
            for (int i = 0; i < levelDef.friendlies.Count; i++)
            {
                FriendlyDef friendly = levelDef.friendlies[i];
                if (friendly.behavior != ActiveBehaviorType.AutoRotate)
                {
                    continue;
                }

                float center = (friendly.sweepMinYaw + friendly.sweepMaxYaw) * 0.5f;
                float halfAmplitude = (friendly.sweepMaxYaw - friendly.sweepMinYaw) * 0.5f * PassAimSweepAmplitudeMultiplier;
                friendly.sweepMinYaw = center - halfAmplitude;
                friendly.sweepMaxYaw = center + halfAmplitude;
            }
        }

        private static void ApplyPitchScale(LevelDef levelDef)
        {
            levelDef.fieldMin *= PitchScale;
            levelDef.fieldMax *= PitchScale;
            levelDef.goalPosition = ScalePitchPosition(levelDef.goalPosition);
            levelDef.minTravelDistance *= PitchScale;
            levelDef.maxTravelDistance *= PitchScale;
            levelDef.ballSpeed *= PitchScale * 0.5f;
            levelDef.receiverRadius *= 1.2f;
            levelDef.opponentRadius *= 1.2f;

            for (int i = 0; i < levelDef.friendlies.Count; i++)
            {
                FriendlyDef friendly = levelDef.friendlies[i];
                friendly.position = ScaleGameplayPosition(friendly.position);
                friendly.lateralStart = ScaleGameplayPosition(friendly.lateralStart);
                friendly.lateralEnd = ScaleGameplayPosition(friendly.lateralEnd);
            }

            for (int i = 0; i < levelDef.opponents.Count; i++)
            {
                OpponentDef opponent = levelDef.opponents[i];
                opponent.position = ScaleGameplayPosition(opponent.position);
                opponent.lateralStart = ScaleGameplayPosition(opponent.lateralStart);
                opponent.lateralEnd = ScaleGameplayPosition(opponent.lateralEnd);
            }

            for (int i = 0; i < levelDef.gemPositions.Count; i++)
            {
                levelDef.gemPositions[i] = ScaleGameplayPosition(levelDef.gemPositions[i]);
            }
        }

        private static void AddPassRouteGems(LevelDef levelDef)
        {
            if (levelDef.friendlies.Count < 2)
            {
                return;
            }

            int gemCount = levelDef.number % 2 == 0 ? 3 : 2;
            int segmentCount = levelDef.friendlies.Count - 1;
            for (int i = 0; i < gemCount; i++)
            {
                int segmentIndex = Mathf.Clamp(Mathf.FloorToInt((i + 0.5f) * segmentCount / gemCount), 0, segmentCount - 1);
                Vector3 start = levelDef.friendlies[segmentIndex].position;
                Vector3 end = levelDef.friendlies[segmentIndex + 1].position;
                float along = 0.46f + ((levelDef.number + i * 3) % 3 - 1) * 0.05f;
                Vector3 position = Vector3.Lerp(start, end, along);
                position.y = 0.2f;
                levelDef.gemPositions.Add(position);
            }
        }

        private static Vector3 ScalePitchPosition(Vector3 position)
        {
            return new Vector3(position.x * PitchScale, position.y, position.z * PitchScale);
        }

        private static Vector3 ScaleGameplayPosition(Vector3 position)
        {
            return new Vector3(position.x * GameplayPositionScale, position.y, position.z * GameplayPositionScale);
        }

        private static float GetYawDegrees(Vector3 direction)
        {
            Vector3 flat = new Vector3(direction.x, 0f, direction.z);
            if (flat.sqrMagnitude < 0.001f)
            {
                return 0f;
            }

            return Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
        }

        private static LevelDef CreateGeneratedLevel(int number)
        {
            LevelDef levelDef = BaseLevel(number, GetGeneratedPrompt(number));
            float progress = (number - 1f) / Mathf.Max(1f, GameSession.LevelCount - 1f);
            levelDef.receiverRadius = Mathf.Lerp(0.94f, 0.56f, progress);
            levelDef.opponentRadius = Mathf.Lerp(0.5f, 0.69f, progress);
            levelDef.chargeTime = Mathf.Lerp(0.98f, 0.58f, progress);
            levelDef.ballSpeed = Mathf.Lerp(8.35f, 9.45f, progress);
            levelDef.maxTravelDistance = Mathf.Lerp(6.6f, 8.9f, progress);

            int friendlyCount = Mathf.Clamp(3 + number / 4, 3, 8);
            float zStart = -5.2f;
            float zEnd = 5.35f;
            for (int i = 0; i < friendlyCount; i++)
            {
                bool isFirst = i == 0;
                bool isTarget = i == friendlyCount - 1;
                float t = friendlyCount == 1 ? 0f : i / (friendlyCount - 1f);
                float z = Mathf.Lerp(zStart, zEnd, t);
                float lane = i == 0 || isTarget ? 0f : ((i % 2 == 0 ? 1f : -1f) * Mathf.Lerp(0.85f, 1.75f, progress));
                lane += Mathf.Sin(number * 0.73f + i * 1.31f) * Mathf.Lerp(0.05f, 0.36f, progress);
                if (isTarget)
                {
                    Vector3 targetPosition = GetTargetTenPosition(number, progress, zEnd);
                    lane = targetPosition.x;
                    z = targetPosition.z;
                }
                int shirtNumber = isTarget ? 10 : Mathf.Clamp(2 + i + (number % 4), 2, 9);

                ActiveBehaviorType behavior = ActiveBehaviorType.AutoRotate;
                float sweepMin = -18f + lane * -12f;
                float sweepMax = 18f + lane * -12f;
                float speed = Mathf.Lerp(0.62f, 1.55f, progress) + i * 0.035f;
                float fixedYaw = Mathf.Clamp(lane * -18f, -54f, 54f);

                if (!isFirst && !isTarget && number >= 4 && (i + number) % 3 == 0)
                {
                    behavior = ActiveBehaviorType.LateralMove;
                }

                FriendlyDef friendly = Friendly("F" + (i + 1), shirtNumber, new Vector3(lane, 0f, z), isTarget, behavior, sweepMin, sweepMax, speed, fixedYaw);
                if (behavior == ActiveBehaviorType.LateralMove)
                {
                    float width = Mathf.Lerp(0.55f, 1.35f, progress);
                    friendly.lateralStart = new Vector3(Mathf.Clamp(lane - width, -2.4f, 2.4f), 0f, z);
                    friendly.lateralEnd = new Vector3(Mathf.Clamp(lane + width, -2.4f, 2.4f), 0f, z + Mathf.Sin(i * 1.7f + number) * 0.22f);
                    friendly.lateralSpeed = Mathf.Lerp(0.35f, 0.82f, progress) + i * 0.035f;
                    friendly.fixedYaw = fixedYaw;
                }
                if (isTarget)
                {
                    ConfigureTargetTenMovement(friendly, number, progress);
                }

                levelDef.friendlies.Add(friendly);
            }

            AddBlondeHeaderOpponent(levelDef, number, progress);
            AddOpponentWalls(levelDef, number, progress);

            int lineCount = number <= 1 ? 0 : Mathf.Clamp(1 + (number - 2) / 3, 1, 8);
            for (int line = 0; line < lineCount; line++)
            {
                if (levelDef.opponents.Count >= MaxOutfieldOpponents)
                {
                    break;
                }

                float t = lineCount <= 1 ? 0.35f : line / (lineCount - 1f);
                float z = Mathf.Lerp(-3.95f, 3.6f, t);
                int runners = number < 8 ? 1 : (number < 18 ? 2 : 3);
                if (number >= 12 && line % 3 == 2)
                {
                    runners = Mathf.Min(3, runners + 1);
                }

                for (int j = 0; j < runners; j++)
                {
                    if (levelDef.opponents.Count >= MaxOutfieldOpponents)
                    {
                        break;
                    }

                    float rowOffset = (j - (runners - 1) * 0.5f) * 0.38f;
                    float width = Mathf.Lerp(1.65f, 3.0f, progress) - j * 0.16f;
                    float centerX = Mathf.Sin(number * 0.41f + line * 0.77f + j) * 0.45f;
                    float startX = Mathf.Clamp(centerX - width, -2.85f, 2.85f);
                    float endX = Mathf.Clamp(centerX + width, -2.85f, 2.85f);
                    float speed = Mathf.Lerp(0.45f, 1.1f, progress) + line * 0.035f + j * 0.08f;
                    float phase = number * 0.44f + line * 1.1f + j * 1.7f;
                    Vector3 position = new Vector3(Mathf.Lerp(startX, endX, (Mathf.Sin(phase) + 1f) * 0.5f), 0f, z + rowOffset);
                    levelDef.opponents.Add(MovingOpponent(position, new Vector3(startX, 0f, z + rowOffset), new Vector3(endX, 0f, z + rowOffset), speed, phase));
                }
            }

            return levelDef;
        }

        private static Vector3 GetTargetTenPosition(int number, float progress, float defaultEndZ)
        {
            float laneLimit = Mathf.Lerp(0.95f, 2.05f, progress);
            float laneWave = Mathf.Sin(number * 1.17f) * 0.72f + Mathf.Sin(number * 0.43f + 1.9f) * 0.38f;
            float lane = Mathf.Clamp(laneWave * laneLimit, -2.2f, 2.2f);

            float depthOffset = Mathf.Lerp(-0.42f, 0.34f, (Mathf.Sin(number * 0.81f + 0.6f) + 1f) * 0.5f);
            float z = Mathf.Clamp(defaultEndZ + depthOffset, 4.72f, 5.62f);
            return new Vector3(lane, 0f, z);
        }

        private static void ConfigureTargetTenMovement(FriendlyDef friendly, int number, float progress)
        {
            if (number < 8 || !ShouldTargetTenMove(number))
            {
                return;
            }

            float movementDifficulty = Mathf.InverseLerp(8f, GameSession.LevelCount, number);
            friendly.behavior = ActiveBehaviorType.TargetMove;
            friendly.targetMovePattern = GetTargetTenMovePattern(number);
            friendly.targetMovePause = Mathf.Clamp(Mathf.Lerp(1.85f, 1.05f, movementDifficulty) + (number % 2) * 0.08f, 1f, 2f);
            friendly.targetMovePhase = Mathf.Repeat(number * 0.137f, 1f);
            friendly.lateralSpeed = Mathf.Lerp(0.22f, 0.9f, movementDifficulty) + (number % 5) * 0.025f;

            if (friendly.targetMovePattern == 2)
            {
                friendly.lateralSpeed = Mathf.Lerp(0.16f, 0.48f, movementDifficulty) + (number % 4) * 0.018f;
            }

            float width = Mathf.Lerp(0.48f, 1.42f, movementDifficulty);
            float depth = Mathf.Lerp(0.24f, 0.82f, movementDifficulty);
            Vector3 center = friendly.position;

            if (friendly.targetMovePattern == 0)
            {
                friendly.lateralStart = new Vector3(Mathf.Clamp(center.x - width, -2.35f, 2.35f), 0f, center.z);
                friendly.lateralEnd = new Vector3(Mathf.Clamp(center.x + width, -2.35f, 2.35f), 0f, center.z);
            }
            else if (friendly.targetMovePattern == 1)
            {
                friendly.lateralStart = new Vector3(center.x, 0f, Mathf.Clamp(center.z - depth, 4.68f, 5.68f));
                friendly.lateralEnd = new Vector3(center.x, 0f, Mathf.Clamp(center.z + depth, 4.68f, 5.68f));
            }
            else
            {
                friendly.lateralStart = new Vector3(Mathf.Clamp(center.x - width, -2.35f, 2.35f), 0f, Mathf.Clamp(center.z - depth, 4.68f, 5.68f));
                friendly.lateralEnd = new Vector3(Mathf.Clamp(center.x + width, -2.35f, 2.35f), 0f, Mathf.Clamp(center.z + depth, 4.68f, 5.68f));
            }

            friendly.position = GetTargetMovePosition(friendly, friendly.targetMovePhase);
            friendly.fixedYaw = Mathf.Clamp(center.x * -16f, -46f, 46f);
        }

        private static bool ShouldTargetTenMove(int number)
        {
            if (number < 14)
            {
                return number % 2 == 0;
            }

            if (number < 22)
            {
                return number % 3 != 1;
            }

            return number % 4 != 1;
        }

        private static int GetTargetTenMovePattern(int number)
        {
            if (number < 14)
            {
                return 0;
            }

            if (number < 22)
            {
                return number % 2 == 0 ? 0 : 1;
            }

            if (number % 3 == 0)
            {
                return 2;
            }

            return number % 2 == 0 ? 1 : 0;
        }

        private static bool AddBlondeHeaderOpponent(LevelDef levelDef, int number, float progress)
        {
            if (number < 12 || !ShouldBlondeHeaderAppear(number) || levelDef.opponents.Count >= MaxOutfieldOpponents || levelDef.friendlies.Count == 0)
            {
                return false;
            }

            FriendlyDef targetTen = levelDef.friendlies[levelDef.friendlies.Count - 1];
            Vector3 tenPosition = targetTen.position;
            float side = Mathf.Sin(number * 0.91f) >= 0f ? 1f : -1f;
            float xOffset = side * Mathf.Lerp(0.82f, 1.05f, progress);
            Vector3 position = new Vector3(
                Mathf.Clamp(tenPosition.x + xOffset, -2.25f, 2.25f),
                0f,
                Mathf.Clamp(tenPosition.z - Mathf.Lerp(0.82f, 1.18f, progress), 3.82f, 4.82f));

            OpponentDef blonde = Opponent(position);
            blonde.isBlondeHeader = true;

            if (number >= 16)
            {
                ConfigureBlondeHeaderMovement(blonde, number, progress, tenPosition.z);
            }

            levelDef.opponents.Add(blonde);
            ArrangeFinalFriendlyGroundLane(levelDef, blonde, progress);
            return true;
        }

        private static void ArrangeFinalFriendlyGroundLane(LevelDef levelDef, OpponentDef blonde, float progress)
        {
            if (levelDef.friendlies.Count < 2)
            {
                return;
            }

            FriendlyDef targetTen = levelDef.friendlies[levelDef.friendlies.Count - 1];
            FriendlyDef passer = levelDef.friendlies[levelDef.friendlies.Count - 2];
            float defenderSide = blonde.position.x >= targetTen.position.x ? 1f : -1f;
            float laneOffset = Mathf.Lerp(1.18f, 1.62f, progress);
            float passerZ = Mathf.Clamp(targetTen.position.z - Mathf.Lerp(1.45f, 1.9f, progress), 3.0f, targetTen.position.z - 0.85f);
            passer.position = new Vector3(Mathf.Clamp(targetTen.position.x - defenderSide * laneOffset, -2.35f, 2.35f), 0f, passerZ);
            passer.lateralStart = passer.position;
            passer.lateralEnd = passer.position;
            passer.lateralSpeed = 0f;
            passer.behavior = ActiveBehaviorType.AutoRotate;

            Vector3 toTarget = targetTen.position - passer.position;
            float yaw = GetYawDegrees(toTarget);
            passer.fixedYaw = yaw;
            passer.sweepMinYaw = yaw - 10f;
            passer.sweepMaxYaw = yaw + 10f;
            passer.sweepSpeed = Mathf.Lerp(0.46f, 0.72f, progress);
        }

        private static bool ShouldBlondeHeaderAppear(int number)
        {
            if (number < 18)
            {
                return number % 3 != 1;
            }

            if (number < 24)
            {
                return number % 4 != 1;
            }

            return number % 5 != 2;
        }

        private static void ConfigureBlondeHeaderMovement(OpponentDef opponent, int number, float progress, float targetTenZ)
        {
            float movementDifficulty = Mathf.InverseLerp(16f, GameSession.LevelCount, number);
            opponent.targetMovePattern = GetTargetTenMovePattern(number);
            opponent.targetMovePause = Mathf.Clamp(Mathf.Lerp(1.8f, 1.05f, movementDifficulty), 1f, 2f);
            opponent.lateralPhase = Mathf.Repeat(number * 0.173f, 1f);
            opponent.lateralSpeed = Mathf.Lerp(0.18f, 0.68f, movementDifficulty) + (number % 4) * 0.02f;

            if (opponent.targetMovePattern == 2)
            {
                opponent.lateralSpeed *= 0.62f;
            }

            Vector3 center = opponent.position;
            float width = Mathf.Lerp(0.36f, 0.98f, movementDifficulty);
            float depth = Mathf.Lerp(0.16f, 0.48f, movementDifficulty);
            float maxZBeforeTen = Mathf.Clamp(targetTenZ - 0.42f, 3.72f, 4.98f);
            if (opponent.targetMovePattern == 0)
            {
                opponent.lateralStart = new Vector3(Mathf.Clamp(center.x - width, -2.35f, 2.35f), 0f, center.z);
                opponent.lateralEnd = new Vector3(Mathf.Clamp(center.x + width, -2.35f, 2.35f), 0f, center.z);
            }
            else if (opponent.targetMovePattern == 1)
            {
                opponent.lateralStart = new Vector3(center.x, 0f, Mathf.Clamp(center.z - depth, 3.72f, maxZBeforeTen));
                opponent.lateralEnd = new Vector3(center.x, 0f, Mathf.Clamp(center.z + depth, 3.72f, maxZBeforeTen));
            }
            else
            {
                opponent.lateralStart = new Vector3(Mathf.Clamp(center.x - width, -2.35f, 2.35f), 0f, Mathf.Clamp(center.z - depth, 3.72f, maxZBeforeTen));
                opponent.lateralEnd = new Vector3(Mathf.Clamp(center.x + width, -2.35f, 2.35f), 0f, Mathf.Clamp(center.z + depth, 3.72f, maxZBeforeTen));
            }

            opponent.position = GetOpponentMovePosition(opponent, opponent.lateralPhase);
        }

        private static void AddOpponentWalls(LevelDef levelDef, int number, float progress)
        {
            if (number < 6)
            {
                return;
            }

            int wallCount = number < 14 ? 1 : (number < 24 ? 2 : 3);
            for (int wall = 0; wall < wallCount; wall++)
            {
                if (levelDef.opponents.Count >= MaxOutfieldOpponents)
                {
                    return;
                }

                int defenders = (number + wall) % 3 == 0 ? 3 : 2;
                float z = Mathf.Lerp(-2.75f, 3.15f, wallCount == 1 ? 0.52f : wall / (wallCount - 1f));
                z += Mathf.Sin(number * 0.37f + wall) * 0.35f;
                float centerX = Mathf.Sin(number * 0.51f + wall * 1.8f) * 0.42f;
                float spacing = Mathf.Lerp(0.48f, 0.62f, progress);
                bool movingWall = number >= 9 && (number + wall) % 2 == 0;
                float moveWidth = Mathf.Lerp(0.55f, 1.25f, progress);
                float groupStart = Mathf.Clamp(centerX - moveWidth, -2.4f, 2.4f);
                float groupEnd = Mathf.Clamp(centerX + moveWidth, -2.4f, 2.4f);
                float speed = Mathf.Lerp(0.34f, 0.82f, progress) + wall * 0.08f;
                float phase = number * 0.62f + wall * 1.43f;

                for (int i = 0; i < defenders; i++)
                {
                    if (levelDef.opponents.Count >= MaxOutfieldOpponents)
                    {
                        return;
                    }

                    float offset = (i - (defenders - 1) * 0.5f) * spacing;
                    Vector3 position = new Vector3(centerX + offset, 0f, z);
                    if (!movingWall)
                    {
                        levelDef.opponents.Add(Opponent(position));
                        continue;
                    }

                    Vector3 start = new Vector3(Mathf.Clamp(groupStart + offset, -2.85f, 2.85f), 0f, z);
                    Vector3 end = new Vector3(Mathf.Clamp(groupEnd + offset, -2.85f, 2.85f), 0f, z);
                    float t = (Mathf.Sin(phase) + 1f) * 0.5f;
                    levelDef.opponents.Add(MovingOpponent(Vector3.Lerp(start, end, t), start, end, speed, phase));
                }
            }
        }

        private static string GetGeneratedPrompt(int number)
        {
            if (number <= 3)
            {
                return "Pick the open pass";
            }

            if (number <= 6)
            {
                return "Moving teammates";
            }

            if (number <= 10)
            {
                return "Beat the red line";
            }

            if (number <= 15)
            {
                return "Use short and long passes";
            }

            if (number <= 22)
            {
                return "Time the horizontal runners";
            }

            return "Final build-up";
        }

        private static LevelDef CreateLevelOne()
        {
            LevelDef levelDef = BaseLevel(1, "Short or long pass");
            levelDef.receiverRadius = 0.9f;
            levelDef.opponentRadius = 0.55f;
            levelDef.friendlies.Add(Friendly("F1", 7, new Vector3(0f, 0f, -4.7f), false, ActiveBehaviorType.AutoRotate, -6f, 8f, 0.65f, 0f));
            levelDef.friendlies.Add(Friendly("F2", 8, new Vector3(-0.75f, 0f, -2.15f), false, ActiveBehaviorType.AutoRotate, 6f, 24f, 0.7f, 13f));
            levelDef.friendlies.Add(Friendly("F3", 9, new Vector3(0.85f, 0f, 1.05f), false, ActiveBehaviorType.AutoRotate, -18f, 8f, 0.72f, -7f));
            levelDef.friendlies.Add(Friendly("F4", 10, new Vector3(0f, 0f, 4.6f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            return levelDef;
        }

        private static LevelDef CreateLevelTwo()
        {
            LevelDef levelDef = BaseLevel(2, "Avoid red defender");
            levelDef.receiverRadius = 0.84f;
            levelDef.opponentRadius = 0.56f;
            levelDef.friendlies.Add(Friendly("F1", 6, new Vector3(-0.25f, 0f, -4.75f), false, ActiveBehaviorType.AutoRotate, -18f, 25f, 0.86f, 8f));
            levelDef.friendlies.Add(Friendly("F2", 8, new Vector3(1.35f, 0f, -1.95f), false, ActiveBehaviorType.AutoRotate, -38f, -9f, 0.82f, -20f));
            levelDef.friendlies.Add(Friendly("F3", 9, new Vector3(-1.25f, 0f, 1.25f), false, ActiveBehaviorType.AutoRotate, 20f, 45f, 0.78f, 33f));
            levelDef.friendlies.Add(Friendly("F4", 10, new Vector3(0.3f, 0f, 4.85f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(-0.05f, 0f, -2.35f)));
            return levelDef;
        }

        private static LevelDef CreateLevelThree()
        {
            LevelDef levelDef = BaseLevel(3, "Lob over cones");
            levelDef.receiverRadius = 0.8f;
            levelDef.opponentRadius = 0.58f;
            levelDef.friendlies.Add(Friendly("F1", 5, new Vector3(0f, 0f, -4.85f), false, ActiveBehaviorType.AutoRotate, -16f, 18f, 0.92f, 0f));
            levelDef.friendlies.Add(Friendly("F2", 7, new Vector3(-1.25f, 0f, -1.75f), false, ActiveBehaviorType.AutoRotate, 18f, 42f, 0.9f, 31f));
            levelDef.friendlies.Add(Friendly("F3", 8, new Vector3(1.3f, 0f, 1.1f), false, ActiveBehaviorType.AutoRotate, -39f, -12f, 0.86f, -24f));
            levelDef.friendlies.Add(Friendly("F4", 10, new Vector3(0.05f, 0f, 4.9f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(1.25f, 0f, -3.1f)));
            return levelDef;
        }

        private static LevelDef CreateLevelFour()
        {
            LevelDef levelDef = BaseLevel(4, "Moving angle");
            levelDef.receiverRadius = 0.74f;
            levelDef.opponentRadius = 0.6f;
            levelDef.chargeTime = 0.78f;
            FriendlyDef first = Friendly("F1", 4, new Vector3(-0.6f, 0f, -4.9f), false, ActiveBehaviorType.LateralMove, 0f, 0f, 0f, 12f);
            first.lateralStart = new Vector3(-1.5f, 0f, -4.9f);
            first.lateralEnd = new Vector3(0.9f, 0f, -4.9f);
            first.lateralSpeed = 0.42f;
            levelDef.friendlies.Add(first);
            levelDef.friendlies.Add(Friendly("F2", 6, new Vector3(1.35f, 0f, -2.1f), false, ActiveBehaviorType.AutoRotate, -48f, -12f, 0.95f, -28f));
            levelDef.friendlies.Add(Friendly("F3", 8, new Vector3(-1.4f, 0f, 0.95f), false, ActiveBehaviorType.AutoRotate, 24f, 54f, 0.9f, 39f));
            levelDef.friendlies.Add(Friendly("F4", 9, new Vector3(1.15f, 0f, 3.05f), false, ActiveBehaviorType.AutoRotate, -36f, -10f, 0.84f, -23f));
            levelDef.friendlies.Add(Friendly("F5", 10, new Vector3(0f, 0f, 5.15f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(0.05f, 0f, -2.9f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.2f, 0f, 1.9f)));
            return levelDef;
        }

        private static LevelDef CreateLevelFive()
        {
            LevelDef levelDef = BaseLevel(5, "Switch lanes");
            levelDef.receiverRadius = 0.7f;
            levelDef.opponentRadius = 0.61f;
            levelDef.chargeTime = 0.72f;
            levelDef.ballSpeed = 9.1f;
            levelDef.friendlies.Add(Friendly("F1", 3, new Vector3(0f, 0f, -5f), false, ActiveBehaviorType.AutoRotate, -28f, 18f, 1.05f, -6f));
            levelDef.friendlies.Add(Friendly("F2", 6, new Vector3(-1.25f, 0f, -3.05f), false, ActiveBehaviorType.AutoRotate, 30f, 58f, 1.02f, 43f));
            levelDef.friendlies.Add(Friendly("F3", 7, new Vector3(1.25f, 0f, -1.1f), false, ActiveBehaviorType.AutoRotate, -56f, -28f, 1f, -42f));
            levelDef.friendlies.Add(Friendly("F4", 8, new Vector3(-1.15f, 0f, 1.15f), false, ActiveBehaviorType.AutoRotate, 32f, 58f, 0.94f, 44f));
            levelDef.friendlies.Add(Friendly("F5", 9, new Vector3(1.05f, 0f, 3.15f), false, ActiveBehaviorType.AutoRotate, -34f, -14f, 0.9f, -24f));
            levelDef.friendlies.Add(Friendly("F6", 10, new Vector3(0f, 0f, 5.25f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(0.8f, 0f, -3.95f)));
            levelDef.opponents.Add(Opponent(new Vector3(-0.2f, 0f, -2.0f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.2f, 0f, 0.25f)));
            return levelDef;
        }

        private static LevelDef CreateLevelSix()
        {
            LevelDef levelDef = BaseLevel(6, "Chip the wall");
            levelDef.receiverRadius = 0.68f;
            levelDef.opponentRadius = 0.62f;
            levelDef.chargeTime = 0.68f;
            levelDef.ballSpeed = 9.35f;
            FriendlyDef first = Friendly("F1", 2, new Vector3(-0.65f, 0f, -5f), false, ActiveBehaviorType.LateralMove, 0f, 0f, 0f, 35f);
            first.lateralStart = new Vector3(-1.55f, 0f, -5f);
            first.lateralEnd = new Vector3(0.75f, 0f, -5f);
            first.lateralSpeed = 0.5f;
            levelDef.friendlies.Add(first);
            levelDef.friendlies.Add(Friendly("F2", 5, new Vector3(1.15f, 0f, -2.55f), false, ActiveBehaviorType.AutoRotate, -56f, -24f, 1.08f, -39f));
            levelDef.friendlies.Add(Friendly("F3", 7, new Vector3(-1.45f, 0f, -0.2f), false, ActiveBehaviorType.AutoRotate, 35f, 64f, 1f, 50f));
            levelDef.friendlies.Add(Friendly("F4", 8, new Vector3(1.0f, 0f, 2.2f), false, ActiveBehaviorType.AutoRotate, -38f, -14f, 0.95f, -25f));
            levelDef.friendlies.Add(Friendly("F5", 10, new Vector3(0.05f, 0f, 5.25f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(-0.7f, 0f, -2.85f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.15f, 0f, 1.2f)));
            return levelDef;
        }

        private static LevelDef CreateLevelSeven()
        {
            LevelDef levelDef = BaseLevel(7, "Narrow windows");
            levelDef.receiverRadius = 0.64f;
            levelDef.opponentRadius = 0.64f;
            levelDef.chargeTime = 0.62f;
            levelDef.ballSpeed = 9.55f;
            levelDef.friendlies.Add(Friendly("F1", 4, new Vector3(-0.05f, 0f, -5.15f), false, ActiveBehaviorType.AutoRotate, -42f, -20f, 1.22f, -31f));
            levelDef.friendlies.Add(Friendly("F2", 5, new Vector3(-1.55f, 0f, -3.1f), false, ActiveBehaviorType.AutoRotate, 42f, 67f, 1.18f, 55f));
            levelDef.friendlies.Add(Friendly("F3", 6, new Vector3(1.2f, 0f, -1.25f), false, ActiveBehaviorType.AutoRotate, -57f, -35f, 1.14f, -47f));
            levelDef.friendlies.Add(Friendly("F4", 7, new Vector3(-1.25f, 0f, 0.85f), false, ActiveBehaviorType.AutoRotate, 42f, 63f, 1.08f, 53f));
            levelDef.friendlies.Add(Friendly("F5", 9, new Vector3(1.35f, 0f, 2.85f), false, ActiveBehaviorType.AutoRotate, -37f, -18f, 1.02f, -27f));
            levelDef.friendlies.Add(Friendly("F6", 10, new Vector3(0.05f, 0f, 5.25f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(-0.65f, 0f, -4.1f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.45f, 0f, -2.65f)));
            levelDef.opponents.Add(Opponent(new Vector3(-0.55f, 0f, -0.6f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.65f, 0f, 1.25f)));
            return levelDef;
        }

        private static LevelDef CreateLevelEight()
        {
            LevelDef levelDef = BaseLevel(8, "Moving traffic");
            levelDef.receiverRadius = 0.61f;
            levelDef.opponentRadius = 0.65f;
            levelDef.chargeTime = 0.58f;
            levelDef.ballSpeed = 9.75f;
            FriendlyDef first = Friendly("F1", 2, new Vector3(-1.1f, 0f, -5f), false, ActiveBehaviorType.LateralMove, 0f, 0f, 0f, 50f);
            first.lateralStart = new Vector3(-1.85f, 0f, -5f);
            first.lateralEnd = new Vector3(0.65f, 0f, -5f);
            first.lateralSpeed = 0.62f;
            levelDef.friendlies.Add(first);
            levelDef.friendlies.Add(Friendly("F2", 4, new Vector3(1.25f, 0f, -3.25f), false, ActiveBehaviorType.AutoRotate, -58f, -34f, 1.28f, -45f));
            levelDef.friendlies.Add(Friendly("F3", 6, new Vector3(-1.25f, 0f, -1.45f), false, ActiveBehaviorType.AutoRotate, 42f, 66f, 1.22f, 54f));
            levelDef.friendlies.Add(Friendly("F4", 7, new Vector3(1.45f, 0f, 0.45f), false, ActiveBehaviorType.AutoRotate, -58f, -34f, 1.16f, -45f));
            levelDef.friendlies.Add(Friendly("F5", 8, new Vector3(-1.1f, 0f, 2.35f), false, ActiveBehaviorType.AutoRotate, 34f, 56f, 1.08f, 44f));
            levelDef.friendlies.Add(Friendly("F6", 10, new Vector3(0.2f, 0f, 5.35f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(-0.2f, 0f, -3.75f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.35f, 0f, -2.0f)));
            levelDef.opponents.Add(Opponent(new Vector3(-0.6f, 0f, 0.35f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.85f, 0f, 2.15f)));
            return levelDef;
        }

        private static LevelDef CreateLevelNine()
        {
            LevelDef levelDef = BaseLevel(9, "Pick a long lane");
            levelDef.receiverRadius = 0.58f;
            levelDef.opponentRadius = 0.67f;
            levelDef.chargeTime = 0.55f;
            levelDef.ballSpeed = 9.95f;
            levelDef.friendlies.Add(Friendly("F1", 3, new Vector3(0f, 0f, -5.25f), false, ActiveBehaviorType.AutoRotate, -34f, 22f, 1.42f, 0f));
            levelDef.friendlies.Add(Friendly("F2", 5, new Vector3(1.55f, 0f, -3.55f), false, ActiveBehaviorType.AutoRotate, -62f, -38f, 1.36f, -50f));
            levelDef.friendlies.Add(Friendly("F3", 6, new Vector3(-1.55f, 0f, -2.05f), false, ActiveBehaviorType.AutoRotate, 42f, 66f, 1.3f, 54f));
            levelDef.friendlies.Add(Friendly("F4", 7, new Vector3(0.65f, 0f, 0.05f), false, ActiveBehaviorType.AutoRotate, -32f, 15f, 1.24f, -8f));
            levelDef.friendlies.Add(Friendly("F5", 8, new Vector3(-1.45f, 0f, 2.05f), false, ActiveBehaviorType.AutoRotate, 35f, 58f, 1.14f, 44f));
            levelDef.friendlies.Add(Friendly("F6", 9, new Vector3(1.25f, 0f, 3.45f), false, ActiveBehaviorType.AutoRotate, -36f, -16f, 1.08f, -25f));
            levelDef.friendlies.Add(Friendly("F7", 10, new Vector3(0f, 0f, 5.5f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(0.65f, 0f, -4.45f)));
            levelDef.opponents.Add(Opponent(new Vector3(-0.6f, 0f, -3.05f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.75f, 0f, -1.6f)));
            levelDef.opponents.Add(Opponent(new Vector3(-0.75f, 0f, -0.15f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.8f, 0f, 1.55f)));
            return levelDef;
        }

        private static LevelDef CreateLevelTen()
        {
            LevelDef levelDef = BaseLevel(10, "Final: mix it all");
            levelDef.receiverRadius = 0.55f;
            levelDef.opponentRadius = 0.68f;
            levelDef.chargeTime = 0.52f;
            levelDef.ballSpeed = 10.2f;
            levelDef.friendlies.Add(Friendly("F1", 2, new Vector3(-0.8f, 0f, -5.3f), false, ActiveBehaviorType.AutoRotate, -42f, 28f, 1.52f, -6f));
            levelDef.friendlies.Add(Friendly("F2", 3, new Vector3(1.45f, 0f, -3.65f), false, ActiveBehaviorType.AutoRotate, -62f, -42f, 1.48f, -52f));
            levelDef.friendlies.Add(Friendly("F3", 5, new Vector3(-1.65f, 0f, -2.1f), false, ActiveBehaviorType.AutoRotate, 46f, 68f, 1.42f, 58f));
            levelDef.friendlies.Add(Friendly("F4", 6, new Vector3(1.25f, 0f, -0.45f), false, ActiveBehaviorType.AutoRotate, -55f, -32f, 1.36f, -44f));
            levelDef.friendlies.Add(Friendly("F5", 7, new Vector3(-1.25f, 0f, 1.25f), false, ActiveBehaviorType.AutoRotate, 42f, 62f, 1.28f, 52f));
            levelDef.friendlies.Add(Friendly("F6", 8, new Vector3(1.25f, 0f, 2.9f), false, ActiveBehaviorType.AutoRotate, -38f, -18f, 1.18f, -28f));
            levelDef.friendlies.Add(Friendly("F7", 9, new Vector3(-0.65f, 0f, 4.0f), false, ActiveBehaviorType.AutoRotate, 14f, 34f, 1.08f, 24f));
            levelDef.friendlies.Add(Friendly("F8", 10, new Vector3(0.25f, 0f, 5.55f), true, ActiveBehaviorType.None, 0f, 0f, 0f, 0f));
            levelDef.opponents.Add(Opponent(new Vector3(-0.25f, 0f, -4.55f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.45f, 0f, -3.05f)));
            levelDef.opponents.Add(Opponent(new Vector3(-0.45f, 0f, -1.3f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.55f, 0f, 0.45f)));
            levelDef.opponents.Add(Opponent(new Vector3(-0.6f, 0f, 2.1f)));
            levelDef.opponents.Add(Opponent(new Vector3(0.6f, 0f, 3.75f)));
            return levelDef;
        }

        private static LevelDef BaseLevel(int number, string prompt)
        {
            LevelDef levelDef = new LevelDef();
            levelDef.number = number;
            levelDef.prompt = prompt;
            levelDef.fieldMin = new Vector2(-4.1f, -5.8f);
            levelDef.fieldMax = new Vector2(4.1f, 6.35f);
            levelDef.goalPosition = new Vector3(0f, 0f, 6.65f);
            levelDef.receiverRadius = 0.75f;
            levelDef.opponentRadius = 0.55f;
            levelDef.minTravelDistance = 1.15f;
            levelDef.maxTravelDistance = 7.7f;
            levelDef.ballSpeed = 8.7f;
            levelDef.chargeTime = 0.8f;
            return levelDef;
        }

        private static FriendlyDef Friendly(string id, int number, Vector3 position, bool isTen, ActiveBehaviorType behavior, float sweepMin, float sweepMax, float speed, float fixedYaw)
        {
            FriendlyDef def = new FriendlyDef();
            def.id = id;
            def.number = number;
            def.position = position;
            def.isTargetTen = isTen;
            def.behavior = behavior;
            def.sweepMinYaw = sweepMin;
            def.sweepMaxYaw = sweepMax;
            def.sweepSpeed = speed;
            def.fixedYaw = fixedYaw;
            def.lateralStart = position;
            def.lateralEnd = position;
            def.lateralSpeed = 0f;
            def.targetMovePattern = 0;
            def.targetMovePause = 1.5f;
            def.targetMovePhase = 0f;
            return def;
        }

        private static OpponentDef Opponent(Vector3 position)
        {
            OpponentDef def = new OpponentDef();
            def.position = position;
            def.lateralStart = position;
            def.lateralEnd = position;
            def.lateralSpeed = 0f;
            def.lateralPhase = 0f;
            def.isBlondeHeader = false;
            def.targetMovePattern = 0;
            def.targetMovePause = 0f;
            return def;
        }

        private static OpponentDef MovingOpponent(Vector3 position, Vector3 lateralStart, Vector3 lateralEnd, float lateralSpeed, float lateralPhase)
        {
            OpponentDef def = new OpponentDef();
            def.position = position;
            def.lateralStart = lateralStart;
            def.lateralEnd = lateralEnd;
            def.lateralSpeed = lateralSpeed;
            def.lateralPhase = lateralPhase;
            def.isBlondeHeader = false;
            def.targetMovePattern = 0;
            def.targetMovePause = 0f;
            return def;
        }
    }
}
