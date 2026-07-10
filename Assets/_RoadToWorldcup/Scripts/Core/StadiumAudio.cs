using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RoadToWorldcup
{
    /// <summary>Runtime-generated football soundscape. It keeps the build self-contained while preserving the Music/SFX settings.</summary>
    public sealed class StadiumAudio : MonoBehaviour
    {
        private const int SampleRate = 22050;
        // These two placeholders stay muted until their final assets are supplied.
        private static readonly bool BallBounceAudioEnabled = false;
        private static readonly bool StadiumAmbienceEnabled = false;
        private static readonly bool CrowdCheerAudioEnabled = false;
        private static StadiumAudio instance;

        private AudioSource musicSource;
        private AudioSource stadiumSource;
        private AudioSource sfxSource;
        private AudioSource stingerSource;
        private AudioClip menuTheme;
        private AudioClip stadiumAmbience;
        private AudioClip passKick;
        private AudioClip ballBounce;
        private AudioClip teammateReceive;
        private AudioClip opponentReceive;
        private AudioClip netHit;
        private AudioClip refereeWhistle;
        private AudioClip crowdCheer;
        private AudioClip victoryFanfare;
        private AudioClip loseTheme;
        private AudioClip buttonTap;
        private bool mainMenuActive;
        private bool gameplayActive;
        private float lastButtonTime = -1f;

        public static void EnsureInstalled(GameObject root)
        {
            if (instance != null)
            {
                return;
            }

            instance = root.AddComponent<StadiumAudio>();
        }

        public static void SetMainMenuAmbience()
        {
            if (instance == null) return;
            instance.mainMenuActive = true;
            instance.gameplayActive = false;
            instance.RefreshBackground();
        }

        public static void SetGameplayAmbience()
        {
            if (instance == null) return;
            instance.mainMenuActive = false;
            instance.gameplayActive = true;
            instance.RefreshBackground();
        }

        public static void StopBackground()
        {
            if (instance == null) return;
            instance.mainMenuActive = false;
            instance.gameplayActive = false;
            instance.RefreshBackground();
        }

        public static void PlayButton()
        {
            if (instance == null || !GameSession.SfxEnabled || Time.unscaledTime - instance.lastButtonTime < 0.055f) return;
            instance.lastButtonTime = Time.unscaledTime;
            instance.sfxSource.PlayOneShot(instance.buttonTap, 0.42f);
        }

        public static void PlayPassKick(float strength = 1f)
        {
            if (instance == null || !GameSession.SfxEnabled) return;
            instance.sfxSource.PlayOneShot(instance.passKick, Mathf.Clamp(strength, 0.7f, 1.2f));
        }

        public static void PlayBallBounce(float impactSpeed)
        {
            if (!BallBounceAudioEnabled || instance == null || !GameSession.SfxEnabled) return;
            instance.sfxSource.PlayOneShot(instance.ballBounce, Mathf.Clamp(impactSpeed / 8f, 0.2f, 0.65f));
        }

        public static void PlayTeammateReceive()
        {
            if (instance == null || !GameSession.SfxEnabled) return;
            instance.sfxSource.PlayOneShot(instance.teammateReceive, 0.7f);
        }

        public static void PlayOpponentReceive()
        {
            if (instance == null || !GameSession.SfxEnabled) return;
            instance.sfxSource.PlayOneShot(instance.opponentReceive, 0.78f);
        }

        public static void PlayGoal()
        {
            if (instance == null || !GameSession.SfxEnabled) return;
            instance.StartCoroutine(instance.PlayGoalRoutine());
        }

        public static void PlayLose()
        {
            if (instance == null || !GameSession.MusicEnabled) return;
            instance.stingerSource.PlayOneShot(instance.loseTheme, 0.52f);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = CreateSource("Menu_Music", true, 0.24f);
            stadiumSource = CreateSource("Stadium_Ambience", true, 0.15f);
            sfxSource = CreateSource("Gameplay_SFX", false, 1f);
            stingerSource = CreateSource("Result_Stingers", false, 1f);
            CreateClips();
        }

        private void Update()
        {
            RefreshBackground();
            if (!GameSession.SfxEnabled && sfxSource.isPlaying)
            {
                sfxSource.Stop();
            }
            if (!GameSession.MusicEnabled && stingerSource.isPlaying)
            {
                stingerSource.Stop();
            }
        }

        private AudioSource CreateSource(string sourceName, bool loop, float volume)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = volume;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            return source;
        }

        private void RefreshBackground()
        {
            bool playMenu = mainMenuActive && GameSession.MusicEnabled;
            if (playMenu && !musicSource.isPlaying)
            {
                musicSource.clip = menuTheme;
                musicSource.Play();
            }
            else if (!playMenu && musicSource.isPlaying)
            {
                musicSource.Stop();
            }

            bool playStadium = StadiumAmbienceEnabled && gameplayActive && GameSession.MusicEnabled;
            if (playStadium && !stadiumSource.isPlaying)
            {
                stadiumSource.clip = stadiumAmbience;
                stadiumSource.Play();
            }
            else if (!playStadium && stadiumSource.isPlaying)
            {
                stadiumSource.Stop();
            }
        }

        private IEnumerator PlayGoalRoutine()
        {
            sfxSource.PlayOneShot(netHit, 0.9f);
            yield return new WaitForSecondsRealtime(0.08f);
            if (!GameSession.SfxEnabled) yield break;
            sfxSource.PlayOneShot(refereeWhistle, 0.82f);
            yield return new WaitForSecondsRealtime(0.16f);
            if (!GameSession.SfxEnabled) yield break;
            if (CrowdCheerAudioEnabled)
            {
                stingerSource.PlayOneShot(crowdCheer, 0.75f);
            }
            stingerSource.PlayOneShot(victoryFanfare, 0.58f);
        }

        private void CreateClips()
        {
            menuTheme = CreateMenuTheme();
            stadiumAmbience = CreateStadiumAmbience();
            passKick = CreatePassKick();
            ballBounce = CreateBallBounce();
            teammateReceive = CreateTeammateReceive();
            opponentReceive = CreateOpponentReceive();
            netHit = CreateNetHit();
            refereeWhistle = CreateWhistle();
            crowdCheer = CreateCrowdCheer();
            victoryFanfare = CreateVictoryFanfare();
            loseTheme = CreateLoseTheme();
            buttonTap = CreateButtonTap();
        }

        private static AudioClip CreateMenuTheme()
        {
            const float duration = 16f;
            return CreateClip("WC_Menu_Theme", duration, delegate(float[] data, float time, int sample, ref uint seed)
            {
                int beat = Mathf.FloorToInt(time * 2f);
                int[] roots = { 48, 53, 45, 50, 48, 55, 50, 53 };
                int root = roots[(beat / 4) % roots.Length];
                float local = time % 0.5f;
                float pad = 0.035f * (Mathf.Sin(time * Mathf.PI * 2f * Midi(root - 12)) + 0.65f * Mathf.Sin(time * Mathf.PI * 2f * Midi(root - 5)) + 0.45f * Mathf.Sin(time * Mathf.PI * 2f * Midi(root + 3)));
                int melodyStep = Mathf.FloorToInt(time * 4f);
                int[] melody = { 72, 76, 79, 76, 74, 77, 81, 77, 69, 72, 76, 72, 71, 74, 77, 74 };
                float noteAge = time % 0.25f;
                float melodyEnvelope = Mathf.Clamp01(noteAge / 0.018f) * Mathf.Clamp01((0.23f - noteAge) / 0.08f);
                float lead = 0.075f * melodyEnvelope * (Mathf.Sin(time * Mathf.PI * 2f * Midi(melody[melodyStep % melody.Length])) + 0.22f * Mathf.Sin(time * Mathf.PI * 4f * Midi(melody[melodyStep % melody.Length])));
                float kickAge = time % 0.5f;
                float kick = kickAge < 0.08f ? Mathf.Sin(kickAge * Mathf.PI * 2f * (92f - kickAge * 700f)) * Mathf.Exp(-kickAge * 33f) * 0.12f : 0f;
                data[sample] = pad + lead + kick;
            });
        }

        private static AudioClip CreateStadiumAmbience()
        {
            return CreateClip("WC_Stadium_Ambience", 9f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float noise = Noise(ref seed);
                float rumble = Mathf.Sin(time * Mathf.PI * 2f * 86f) * 0.014f + Mathf.Sin(time * Mathf.PI * 2f * 121f) * 0.009f;
                float sway = 0.027f + 0.014f * Mathf.Sin(time * Mathf.PI * 2f * 0.19f) + 0.009f * Mathf.Sin(time * Mathf.PI * 2f * 0.41f);
                float chant = Mathf.Max(0f, Mathf.Sin(time * Mathf.PI * 2f * 1.17f)) * 0.012f * (Mathf.Sin(time * Mathf.PI * 2f * 380f) + Mathf.Sin(time * Mathf.PI * 2f * 510f));
                data[sample] = noise * sway + rumble + chant;
            });
        }

        private static AudioClip CreatePassKick()
        {
            return CreateClip("SFX_Pass_Kick", 0.19f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float envelope = Mathf.Exp(-time * 22f);
                float tone = Mathf.Sin(time * Mathf.PI * 2f * (170f - time * 340f));
                data[sample] = envelope * (tone * 0.55f + Noise(ref seed) * 0.38f);
            });
        }

        private static AudioClip CreateBallBounce()
        {
            return CreateClip("SFX_Ball_Bounce", 0.14f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float envelope = Mathf.Exp(-time * 32f);
                data[sample] = envelope * (Mathf.Sin(time * Mathf.PI * 2f * (115f - time * 210f)) * 0.45f + Noise(ref seed) * 0.2f);
            });
        }

        private static AudioClip CreateTeammateReceive()
        {
            return CreateClip("SFX_Teammate_Receive", 0.27f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float envelope = Mathf.Exp(-time * 11f);
                data[sample] = envelope * (0.34f * Mathf.Sin(time * Mathf.PI * 2f * 520f) + 0.22f * Mathf.Sin(time * Mathf.PI * 2f * 780f));
            });
        }

        private static AudioClip CreateOpponentReceive()
        {
            return CreateClip("SFX_Opponent_Receive", 0.34f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float envelope = Mathf.Exp(-time * 8f);
                data[sample] = envelope * (0.31f * Mathf.Sin(time * Mathf.PI * 2f * (190f - time * 90f)) + Noise(ref seed) * 0.2f);
            });
        }

        private static AudioClip CreateNetHit()
        {
            return CreateClip("SFX_Goal_Net", 0.36f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float envelope = Mathf.Exp(-time * 9f);
                data[sample] = envelope * (Noise(ref seed) * 0.38f + Mathf.Sin(time * Mathf.PI * 2f * 220f) * 0.16f);
            });
        }

        private static AudioClip CreateWhistle()
        {
            return CreateClip("SFX_Referee_Whistle", 0.48f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float envelope = Mathf.Clamp01(time / 0.025f) * Mathf.Clamp01((0.45f - time) / 0.1f);
                float vibrato = Mathf.Sin(time * Mathf.PI * 2f * 5f) * 18f;
                data[sample] = envelope * (Mathf.Sin(time * Mathf.PI * 2f * (2180f + vibrato)) * 0.2f + Mathf.Sin(time * Mathf.PI * 2f * (4360f + vibrato)) * 0.06f);
            });
        }

        private static AudioClip CreateCrowdCheer()
        {
            return CreateClip("SFX_Goal_Crowd", 2.7f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float rise = Mathf.Clamp01(time / 0.32f) * Mathf.Clamp01((2.65f - time) / 0.7f);
                float noise = Noise(ref seed) * 0.11f;
                float voices = (Mathf.Sin(time * Mathf.PI * 2f * 330f) + Mathf.Sin(time * Mathf.PI * 2f * 415f) + Mathf.Sin(time * Mathf.PI * 2f * 523f)) * 0.024f;
                data[sample] = rise * (noise + voices);
            });
        }

        private static AudioClip CreateVictoryFanfare()
        {
            return CreateClip("SFX_Victory_Fanfare", 1.9f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                int step = Mathf.Min(4, Mathf.FloorToInt(time / 0.29f));
                int[] notes = { 72, 76, 79, 84, 88 };
                float age = time - step * 0.29f;
                float envelope = Mathf.Clamp01(age / 0.018f) * Mathf.Clamp01((0.32f - age) / 0.12f);
                float frequency = Midi(notes[step]);
                data[sample] = envelope * (Mathf.Sin(time * Mathf.PI * 2f * frequency) * 0.16f + Mathf.Sin(time * Mathf.PI * 2f * frequency * 1.5f) * 0.05f);
            });
        }

        private static AudioClip CreateLoseTheme()
        {
            return CreateClip("Music_Lose", 1.75f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                int step = Mathf.Min(3, Mathf.FloorToInt(time / 0.42f));
                int[] notes = { 64, 60, 57, 52 };
                float age = time - step * 0.42f;
                float envelope = Mathf.Clamp01(age / 0.035f) * Mathf.Clamp01((0.47f - age) / 0.14f);
                float frequency = Midi(notes[step]);
                data[sample] = envelope * (Mathf.Sin(time * Mathf.PI * 2f * frequency) * 0.14f + Mathf.Sin(time * Mathf.PI * 2f * frequency * 0.5f) * 0.09f);
            });
        }

        private static AudioClip CreateButtonTap()
        {
            return CreateClip("SFX_Button_Tap", 0.12f, delegate(float[] data, float time, int sample, ref uint seed)
            {
                float envelope = Mathf.Exp(-time * 25f);
                data[sample] = envelope * (Mathf.Sin(time * Mathf.PI * 2f * (740f - time * 1100f)) * 0.16f + Mathf.Sin(time * Mathf.PI * 2f * 1240f) * 0.05f);
            });
        }

        private delegate void SampleGenerator(float[] data, float time, int sample, ref uint seed);

        private static AudioClip CreateClip(string clipName, float duration, SampleGenerator generator)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];
            uint seed = 0xA341316Cu;
            for (int i = 0; i < samples; i++)
            {
                generator(data, i / (float)SampleRate, i, ref seed);
            }
            AudioClip clip = AudioClip.Create(clipName, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Midi(int note) { return 440f * Mathf.Pow(2f, (note - 69) / 12f); }

        private static float Noise(ref uint seed)
        {
            seed = seed * 1664525u + 1013904223u;
            return ((seed >> 8) & 0x00FFFFFF) / 8388607.5f - 1f;
        }
    }

    /// <summary>Added to every generated UI button so new UI automatically gets the same soft tap feedback.</summary>
    public sealed class UiClickSound : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                StadiumAudio.PlayButton();
            }
        }
    }
}
