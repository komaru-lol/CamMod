using BepInEx;
using GorillaLocomotion;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System;
using GorillaNetworking;
using System.IO;
using System.Reflection;
using TMPro;
using Debug = UnityEngine.Debug;

namespace CamMod
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private static GameObject _tpcObject;
        private static GameObject _spec;

        private static GUIStyle _savedTimerStyle;
        private static GUIStyle _windowStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _textFieldStyle;

        private static readonly List<float> SavedTimes = [];
        private static List<VRRig> _playerList = [];
        private static readonly List<float> Speeds = [];
        
        private static string SelectedConfigPath => Path.Combine(ConfigFolder, _selectedConfigName + ".cfg");
        private static string[] _availableConfigs;
        private static string _selectedConfigName = "default";
        private static readonly string ConfigFolder = Path.Combine(Paths.ConfigPath, "KomaruCam");
        private static string _roomCode = "";
        private static string _team1Name = "Team 1";
        private static string _team2Name = "Team 2";
        private static string _tempTeam1Name;
        private static string _tempTeam2Name;
        private static string _nameChange = "";
        private static string _configName = "";

        public static Camera Tpc;
        private static Camera _minimapCamera;

        private static Vector3 _specOffset = new Vector3(0f, 0.1f, -1.5f);
        private static Vector3 _velocity;
        
        private static float _rigLerp = 0.5f;
        private static float _smoothing = 0.5f;
        private static float _fov = 90f;
        private static float _stopWatchTime = -10f;
        private static float _dist;
        private static float _switchCooldown = 2.0f;
        private static float _lastSwitchTime = -Mathf.Infinity;
        private static float _dayTime;
        private static float _maxSpeed;

        private static bool _isEditingTeam1;
        private static bool _isEditingTeam2;
        private static bool _listener;
        private static bool _timing;
        private static bool _distanceDisplay;
        private static bool _settingsDisplay;
        private static bool _scoreDisplay;
        private static bool _isAutoCast;
        private static bool _timer;
        private static bool _menuUI;
        private static bool _spectatorList;
        private static bool _casterMods;
        private static bool _movement;
        private static bool _fpc = true;
        private static bool _spectating;
        private static bool _deletedCamera;
        private static bool _minimap;
        private static bool _isHeadTracking;
        public static bool IsNameTags;
        public static bool IsFpsTags;
        
        private static readonly Action DistanceAction = () => CloseMenu(ref _distanceDisplay);
        private static readonly Action ScoreAction = () => CloseMenu(ref _scoreDisplay);
        private static readonly Action TimerAction = () => CloseMenu(ref _timer);
        private static readonly Action SpectateAction = () => CloseMenu(ref _spectatorList);
        private static readonly Action CastingAction = () => CloseMenu(ref _casterMods);
        private static readonly Action SettingsAction = () => CloseMenu(ref _settingsDisplay);
        private static readonly Action Toggle = () => SwitchBool(ref _movement);
        private static readonly Action MinimapAction = () => CloseMenu(ref _minimap);

        private static int _teamScore1;
        private static int _teamScore2;
        private static int _selectedConfigIndex;
        private static int _weatherTypeIndex;
        private static int _smoothingType = 1;
        private const int CornerRadius = 8;
        private const int MiniMapEspLayer = 25;
        
        private static Color _windowColor;
        private static Color _buttonColor;
        private static Color _textFieldColor;
        private static Color _labelTextColor;
        private static Color _sliderColor;
        private static Color _sliderThumbColor;
        
        private static Texture2D _sliderBackground;
        private static Texture2D _sliderFill;
        private static Texture2D _buttonNormalTex;
        private static Texture2D _buttonHoverTex;
        private static Texture2D _buttonActiveTex;
        private static Texture2D _buttonFocusedTex;
        private static Texture2D _textFieldNormalTex;
        private static Texture2D _textFieldFocusedTex;
        private static Texture2D _textFieldActiveTex;
        private static Texture2D _textFieldHoverTex;

        private enum Theme { Dark, VeryDark, Space, Purple, Solarized, Forest }
        private static Theme _currentTheme = Theme.Dark;
        private static Theme _lastAppliedTheme = (Theme)(-1);

        private static Vector2 _lastMousePosition;

        public static TMP_FontAsset NameTagFont;

        private static AudioListener _camListener;

        private static RenderTexture _minimapRenderTexture;
        
        private static VRRig _specRig;
        
        private static float _defaultClipping = 0.01f;
        private static float _editClipPlane = 0.03f;
        private static float _targetClipping = _defaultClipping;
        private static bool IsSpecNull => _spec == null;
        private static float DesiredClipPlane;
        
        public static void Setup()
        {
            for (int i = 0; i < 6; i++)
            {
                Speeds.Add(0);
            }
            
            EnsureDefaultConfig();
            LoadSettings();
            RpcManager.Init();
        }
        
        void Start()
        { 
            Setup();
        }

        /*void OnEnable()
        {
            HarmonyPatches.ApplyPatches();
        }

        void OnDisable()
        {
            HarmonyPatches.Unpatch();
        }*/
        
        public static void ChangeWeather(BetterDayNightManager.WeatherType weathertype)
        {
            BetterDayNightManager.instance.weatherCycle[BetterDayNightManager.instance.currentWeatherIndex + 1] = weathertype;
            BetterDayNightManager.instance.currentWeatherIndex++;
            BetterDayNightManager.instance.CurrentWeather();
        }
        
        private static readonly string Prefix = "Time Of Day: ";
        private static string _timeStr = Prefix;
        private static int _timeIndex;

        public static void ChangeTime()
        {
            string timeLabel = "Unknown";

            switch (_timeIndex)
            {
                case 0:
                    timeLabel = "Day";
                    BetterDayNightManager.instance.SetTimeOfDay(3);
                    break;
                case 1:
                    timeLabel = "Dawn";
                    BetterDayNightManager.instance.SetTimeOfDay(1);
                    break;
                case 2:
                    timeLabel = "Night";
                    BetterDayNightManager.instance.SetTimeOfDay(0);
                    break;
                case 3:
                    timeLabel = "Night Fall";
                    BetterDayNightManager.instance.SetTimeOfDay(6);
                    break;
                case 4:
                    timeLabel = "Mid Night";
                    BetterDayNightManager.instance.SetTimeOfDay(8);
                    break;
            }

            _timeStr = Prefix + timeLabel;

            _timeIndex = (_timeIndex + 1) % 5;
        }

        
        private void LateUpdate()
        {
            SetupCamera();
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _menuUI = !_menuUI;
                Debug.Log($"Menu Toggled: {_menuUI}");
            }

            SpecBackground();
            NoSmoothRigs();
            MiniMap();

            if (_timer)
            {
                if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
                {
                    _timing = !_timing;
                }
            }

            if (_timing)
            {
                _stopWatchTime += Time.deltaTime;
            }

            if (_scoreDisplay)
            {
                ScoreUpdater();
            }

            if (_isAutoCast)
            {
                AutoCast();
            }

            if (_movement)
            {
                Wasd();
            }

            if (_playerList == null)
                _playerList = new List<VRRig>();

            if (PhotonNetwork.InRoom)
            {
                if (_playerList != null && _playerList.Count > 0)
                {
                    NumberSpectate();
                }
                else
                {
                    _spec = null;
                    _specRig = null;
                }
            }

            NameTags.EnableNameTags();
            
            if (NameTagFont == null)
            {
                NameTagFont = TMP_FontAsset.CreateFontAsset(CreateFont("CamMod.Assets.nametagfont.ttf"));
            }
        }

        private static void EnsureDefaultConfig()
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            string defaultPath = Path.Combine(ConfigFolder, "default.cfg");

            if (!File.Exists(defaultPath))
            {
                File.WriteAllLines(defaultPath, new List<string>
                {
                    "FOV=95",
                    "Smoothing=0.5",
                    "OffsetX=0",
                    "OffsetY=0.1",
                    "OffsetZ=-1.5",
                    "IsAutoCast=0",
                    "CoolDown=0.1",
                    "Clipping=0.01",
                    "DayTime=0",
                    "SmoothingType=1",
                    "RigLerp=0.5",
                    "Tracking=0",
                    "NameTags=0",
                    "FpsTags=0",
                    "WASD=0",
                    "Theme=Dark"
                });
            }
            RefreshAvailableConfigs();
        }

        private static void LoadSettings()
        {
            if (!File.Exists(SelectedConfigPath))
                return;

            var lines = File.ReadAllLines(SelectedConfigPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var split = line.Split('=');
                if (split.Length != 2)
                    continue;

                string key = split[0].Trim();
                string value = split[1].Trim();

                switch (key)
                {
                    case "FOV": _fov = float.Parse(value); break;
                    case "Smoothing": _smoothing = float.Parse(value); break;
                    case "OffsetX": _specOffset.x = float.Parse(value); break;
                    case "OffsetY": _specOffset.y = float.Parse(value); break;
                    case "OffsetZ": _specOffset.z = float.Parse(value); break;
                    case "IsAutoCast": _isAutoCast = value == "1"; break;
                    case "CoolDown": _switchCooldown = float.Parse(value); break;
                    case "Clipping": _defaultClipping = float.Parse(value); break;
                    case "DayTime": _dayTime = float.Parse(value); break;
                    case "SmoothingType": _smoothingType = int.Parse(value); break;
                    case "RigLerp": _rigLerp = float.Parse(value); break;
                    case "Tracking": _isHeadTracking = value == "1"; break;
                    case "NameTags": IsNameTags = value == "1"; break;
                    case "FpsTags": IsFpsTags = value == "1"; break;
                    case "WASD": _movement = value == "1"; break;
                    case "Theme":
                        if (Enum.TryParse(value, out Theme parsedTheme))
                            _currentTheme = parsedTheme;
                        break;
                }
            }
        }

        private static void SaveSettings()
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            var lines = new List<string>
            {
                $"FOV={_fov}",
                $"Smoothing={_smoothing}",
                $"OffsetX={_specOffset.x}",
                $"OffsetY={_specOffset.y}",
                $"OffsetZ={_specOffset.z}",
                $"IsAutoCast={(_isAutoCast ? "1" : "0")}",
                $"CoolDown={_switchCooldown}",
                $"Clipping={_defaultClipping}",
                $"DayTime={_dayTime}",
                $"SmoothingType={_smoothingType}",
                $"RigLerp={_rigLerp}",
                $"Tracking={(_isHeadTracking ? "1" : "0")}",
                $"NameTags={(IsNameTags ? "1" : "0")}",
                $"FpsTags={(IsFpsTags ? "1" : "0")}",
                $"WASD={(_movement ? "1" : "0")}",
                $"Theme={_currentTheme}"
            };

            File.WriteAllLines(SelectedConfigPath, lines);
            RefreshAvailableConfigs();
        }
        
        private static void RefreshAvailableConfigs()
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            _availableConfigs = Directory.GetFiles(ConfigFolder, "*.cfg")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();

            if (_availableConfigs.Length == 0)
                _availableConfigs = new[] { "default" };

            _selectedConfigIndex = Array.IndexOf(_availableConfigs, _selectedConfigName);
            if (_selectedConfigIndex < 0)
                _selectedConfigIndex = 0;
            _selectedConfigName = _availableConfigs[_selectedConfigIndex];
        }
    
        private static Font CreateFont(string path)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            using var stream = executingAssembly.GetManifestResourceStream(path);
            if (stream != null)
            {
                var bytes = new byte[stream.Length];
                _ = stream.Read(bytes, 0, bytes.Length);
                File.WriteAllBytes(Path.Combine(Application.temporaryCachePath, "tempfont.ttf"), bytes);
            }

            var result = new Font(Path.Combine(Application.temporaryCachePath, "tempfont.ttf"));
            return result;
        }
        
        private void ApplyTheme()
        {
            if (_currentTheme == _lastAppliedTheme)
                return;
            
            switch (_currentTheme)
            {
                case Theme.Dark:
                    _windowColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
                    _buttonColor = new Color(0.15f, 0.15f, 0.15f);
                    _textFieldColor = new Color(0.18f, 0.18f, 0.18f);
                    _sliderColor = new Color(0.2f, 0.2f, 0.2f);
                    _sliderThumbColor = new Color(0.3f, 0.3f, 0.3f);
                    _labelTextColor = Color.white;
                    break;

                case Theme.VeryDark:
                    _windowColor = new Color(0.05f, 0.05f, 0.05f, 0.7f);
                    _buttonColor = new Color(0.1f, 0.1f, 0.1f);
                    _textFieldColor = new Color(0.12f, 0.12f, 0.12f);
                    _sliderColor = new Color(0.15f, 0.15f, 0.15f);
                    _sliderThumbColor = new Color(0.2f, 0.2f, 0.2f);
                    _labelTextColor = Color.white;
                    break;

                case Theme.Space:
                    _windowColor = new Color(0.0f, 0.0f, 0.1f, 0.7f);
                    _buttonColor = new Color(0.0f, 0.0f, 0.2f);
                    _textFieldColor = new Color(0.05f, 0.05f, 0.25f);
                    _sliderColor = new Color(0.08f, 0.08f, 0.3f);
                    _sliderThumbColor = new Color(0.2f, 0.4f, 1f);
                    _labelTextColor = new Color(0.6f, 0.8f, 1f);
                    break;

                case Theme.Purple:
                    _windowColor = new Color(0.12f, 0.0f, 0.2f, 0.7f);
                    _buttonColor = new Color(0.2f, 0.0f, 0.3f);
                    _textFieldColor = new Color(0.25f, 0.0f, 0.4f);
                    _sliderColor = new Color(0.3f, 0.0f, 0.5f);
                    _sliderThumbColor = new Color(0.6f, 0.4f, 1f);
                    _labelTextColor = new Color(0.9f, 0.8f, 1f);
                    break;
                
                case Theme.Solarized:
                    _windowColor = new Color(0.0f, 0.17f, 0.21f, 0.7f);
                    _buttonColor = new Color(0.01f, 0.26f, 0.31f);
                    _textFieldColor = new Color(0.02f, 0.36f, 0.41f);
                    _sliderColor = new Color(0.0f, 0.5f, 0.55f);
                    _sliderThumbColor = new Color(0.13f, 0.58f, 0.60f);
                    _labelTextColor = new Color(0.92f, 0.91f, 0.85f);
                    break;

                case Theme.Forest:
                    _windowColor = new Color(0.1f, 0.15f, 0.1f, 0.7f);
                    _buttonColor = new Color(0.15f, 0.2f, 0.15f);
                    _textFieldColor = new Color(0.2f, 0.25f, 0.2f);
                    _sliderColor = new Color(0.2f, 0.3f, 0.2f);
                    _sliderThumbColor = new Color(0.4f, 0.6f, 0.4f);
                    _labelTextColor = new Color(0.8f, 1f, 0.8f);
                    break;

                default:
                    _windowColor = new Color(0.05f, 0.05f, 0.05f, 0.7f);
                    _buttonColor = new Color(0.1f, 0.1f, 0.1f);
                    _textFieldColor = new Color(0.1f, 0.1f, 0.1f);
                    _sliderColor = new Color(0.1f, 0.1f, 0.1f);
                    _sliderThumbColor = new Color(0.3f, 0.3f, 0.3f);
                    _labelTextColor = Color.white;
                    break;
            }

            _buttonNormalTex = MakeRoundedTexture(12, _buttonColor);
            _buttonHoverTex = MakeRoundedTexture(12, _buttonColor * 1.1f);
            _buttonActiveTex = MakeRoundedTexture(12, _buttonColor * 0.9f);
            _buttonFocusedTex = MakeRoundedTexture(12,_buttonColor * 1.2f);

            _textFieldNormalTex = MakeRoundedTexture(8, _textFieldColor);
            _textFieldFocusedTex = MakeRoundedTexture(8, _textFieldColor * 1.2f);
            _textFieldHoverTex = MakeRoundedTexture(8, _textFieldColor * 1.05f);
            _textFieldActiveTex = MakeRoundedTexture(8, _textFieldColor * 0.95f);
            
            _sliderBackground = MakeTexture(_sliderColor);
            _sliderFill = MakeTexture(_sliderThumbColor);
            
            _windowStyle = _buttonStyle = _labelStyle = _textFieldStyle = null;
            _lastAppliedTheme = _currentTheme;
        }
        
        public void CycleTheme()
        {
            _currentTheme = (Theme)(((int)_currentTheme + 1) % Enum.GetValues(typeof(Theme)).Length);
            ApplyTheme();
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture2D = new Texture2D(30, 30);
            Color[] colors = new Color[900];
            for (int index = 0; index < colors.Length; ++index)
                colors[index] = color;
            texture2D.SetPixels(colors);
            texture2D.Apply();
            return texture2D;
        }

        private static Texture2D MakeRoundedTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            for (int i = 0; i < size * size; i++)
            {
                int x = i % size;
                int y = i / size;
                if (Mathf.Sqrt((x - size / 2) * (x - size / 2) + (y - size / 2) * (y - size / 2)) <= size / 2)
                {
                    colors[i] = color;
                }
                else
                {
                    colors[i] = Color.clear;
                }
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
        
        private void SetupGUIStyles()
        {
            if (_windowStyle == null)
            {
                var bg = MakeRoundedTexture(12, _windowColor);
                _windowStyle = new GUIStyle(GUI.skin.window)
                {
                    normal = { background = bg, textColor = _labelTextColor },
                    padding = new RectOffset(0, 0, 30, 0),
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(CornerRadius, CornerRadius, CornerRadius, CornerRadius)
                };
            }
            
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { background = _buttonNormalTex, textColor = _labelTextColor },
                    hover = { background = _buttonHoverTex, textColor = _labelTextColor },
                    active = { background = _buttonActiveTex, textColor = _labelTextColor },
                    focused = { background = _buttonFocusedTex, textColor = _labelTextColor },
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(CornerRadius, CornerRadius, CornerRadius, CornerRadius)
                };
            }

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = _labelTextColor },
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                };
            }

            if (_textFieldStyle == null)
            {
                _textFieldStyle = new GUIStyle(GUI.skin.textField)
                {
                    normal = { background = _textFieldNormalTex, textColor = _labelTextColor },
                    hover = { background = _textFieldHoverTex, textColor = _labelTextColor },
                    active = { background = _textFieldActiveTex, textColor = _labelTextColor },
                    focused = { background = _textFieldFocusedTex, textColor = _labelTextColor },
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
        
        private static Rect _r = new Rect(Screen.width - 270f, 10f, 260f, 340f);
        private static Rect _menuForm = new Rect((Screen.width - 900f) / 2f, (Screen.height - 700f) / 2f, 900f, 700f);
        private static Rect _casterModForm = new Rect(10f, 10f, 250f, 360f);
        private static Rect _timerForm = new Rect(20f, Screen.height - 220f, 320f, 200f);
        private static Rect _settingsForm = new Rect(Screen.width - 280f, 330f, 260f, 390f);
        private static Rect _scoreForm = new Rect(20f, 440f, 300f, 220f);

        private static void BeginMargin(ref Rect oldRect, Rect newRect)
        {
            oldRect.x = newRect.x;
            oldRect.y = newRect.y;
            oldRect.width = newRect.width;
            oldRect.height = newRect.height;
        }

        private static void EndMargin(ref Rect rect)
        {
            rect.x += 10;
            rect.y += 25;
            rect.width -= 20;
            rect.height -= 35;
        }
        
        private void OnGUI()
        {
            SetupGUIStyles();
            ApplyTheme();
            if (Plugin._menuUI)
            {
                Rect rect = new Rect(
                    (Screen.width - _menuForm.width) / 2.0f,
                    (Screen.height - _menuForm.height) / 2.0f,
                    1000f,
                    700f
                );
                
                rect.y = (Screen.height - 665) / 2;
                GUI.Box(rect, "Komaru Camera Mod | ESC", _windowStyle);
                rect.y += 40f;
                rect.x = (Screen.width - 780) / 2f;
                GUILayout.BeginArea(rect);
                GUILayout.Space(10f);
                string[] themes = Enum.GetNames(typeof(Theme));
                int nextIndex = ((int)_currentTheme + 1) % themes.Length;
                Plugin.ColorButton($"Theme: {_currentTheme} → {themes[nextIndex]}", Color.white, CycleTheme, Plugin._menuForm.width - 20f, 30f);
                GUILayout.Space(5f);
                Plugin.ColorButton("Spectate Others", Plugin._spectatorList ? Color.green : Color.white, Plugin.SpectateAction, Plugin._menuForm.width - 20f, 30f);
                GUILayout.Space(5f);
                Plugin.ColorButton("Mini Map", Plugin._minimap ? Color.green : Color.white, Plugin.MinimapAction, Plugin._menuForm.width - 20f, 30f);
                GUILayout.Space(5f);
                Plugin.ColorButton("Casting Mods", Plugin._casterMods ? Color.green : Color.white, Plugin.CastingAction, Plugin._menuForm.width - 20f, 30f);
                GUILayout.Space(5f);
                Plugin.ColorButton("Distance Display", Plugin._distanceDisplay ? Color.green : Color.white, Plugin.DistanceAction, Plugin._menuForm.width - 20f, 30f);
                GUILayout.Space(5f);
                Plugin.ColorButton("Stopwatch", Plugin._timer ? Color.green : Color.white, Plugin.TimerAction, Plugin._menuForm.width - 20f, 30f);
                GUILayout.Space(5f);
                Plugin.ColorButton("Settings", Plugin._settingsDisplay ? Color.green : Color.white, Plugin.SettingsAction, Plugin._menuForm.width - 20f, 30f);
                GUILayout.Space(5f);
                Plugin.ColorButton("Score", Plugin._scoreDisplay ? Color.green : Color.white, Plugin.ScoreAction, Plugin._menuForm.width - 20f, 30f);

                GUILayout.EndArea();

                if (Plugin._spec == null)
                {
                    Plugin._listener = false;

                    if (GUI.Button(new Rect(1320f, 850f, 125f, 30f), Plugin._fpc ? "Third Person" : "First Person", _buttonStyle))
                        Plugin._fpc = !Plugin._fpc;
                }
            }

            if (Plugin._minimap)
            {
                int size = 340;
                int padding = 10;

                Rect mapRect = new Rect(
                    Screen.width - size - padding,
                    Screen.height - size - padding,
                    size,
                    size
                );

                GUI.DrawTexture(mapRect, Plugin._minimapRenderTexture, ScaleMode.ScaleToFit, false);
                SkeletonEsp();
            }
            else {
                TurnOffSkeletonEsp();
            }
            
            if (Plugin._spectatorList)
                Plugin.SpectatorMenu();

            if (Plugin._scoreDisplay)
                Plugin.ScoreDisplay();

            if (Plugin._casterMods)
                Plugin.CasterModsMenu();

            if (Plugin._timer)
                Plugin.TimerGUI();

            if (Plugin._distanceDisplay)
                Plugin.DistanceText();

            if (Plugin._settingsDisplay)
                Plugin.SettingsDisplay();
        }

        private static void SpectatorMenu()
        {
            _r = new Rect(Screen.width - 270f, 10f, 260f, 320f);
            GUI.Box(_r, "Spectator List", _windowStyle);
    
            Rect paddedRect = new Rect(_r.x + 10f, _r.y + 40f, _r.width - 20f, _r.height - 60f);
            GUILayout.BeginArea(paddedRect);
    
            if (PhotonNetwork.InRoom)
            {
                _playerList = new List<VRRig>();
                foreach (var vrrig in GorillaParent.instance.vrrigs)
                {
                    if (!vrrig.isOfflineVRRig)
                        _playerList.Add(vrrig);
                }
        
                int index = 1;
                foreach (VRRig player in _playerList)
                {
                    GUILayout.BeginHorizontal();
                    string name = RigManager.ReachForName(player).NickName.ToUpper();

                    bool isTagged = player.mainSkin.material.name.Contains("fected") || player.mainSkin.material.name.Contains("It");
                    GUI.contentColor = isTagged ? new Color(1f, 0.1f, 0f) : Color.white;

                    GUILayout.Label($"{index}. {name}", _labelStyle);
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Spectate", _buttonStyle, GUILayout.Width(80f)))
                    {
                        _spec = player.gameObject;
                        _specRig = player;
                    }

                    GUILayout.EndHorizontal();
                    index++;
                    GUI.contentColor = Color.white;
                }

                if (_spec != null)
                {
                    GUILayout.Space(10f);
                    if (GUILayout.Button("Stop Spectating", _buttonStyle))
                    {
                        _spec = null;
                        _specRig = null;
                    }
                }
            }
            else
            {
                GUILayout.Space(30f);
                GUILayout.Label("Join a Room\nTo Spectate Others", _labelStyle);
            }

            GUILayout.EndArea();
        }
        
        private static void CasterModsMenu()
        {
            GUI.Box(_casterModForm, "Casting Mods", _windowStyle);
    
            Rect paddedRect = new Rect(_casterModForm.x + 10f, _casterModForm.y + 40f, _casterModForm.width - 20f, _casterModForm.height - 60f);
            GUILayout.BeginArea(paddedRect);
    
            ColorButton("WASD", _movement ? Color.green : Color.white, Toggle);
            GUILayout.Space(8f);
    
            GUILayout.Label("Room Code", _labelStyle);
            _roomCode = GUILayout.TextField(_roomCode.ToUpper(), 10, _textFieldStyle);
            GUILayout.Space(4f);

            string joinBtnText = PhotonNetwork.InRoom ? "Disconnect" : "Join";
            if (GUILayout.Button(joinBtnText, _buttonStyle))
            {
                if (PhotonNetwork.InRoom)
                {
                    PhotonNetwork.Disconnect();   
                    _spec = null;
                    _specRig = null;
                }
                else
                {
                    PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(_roomCode.ToUpper(), JoinType.Solo);
                }
            }

            GUILayout.Space(5f);
            string tagLabel = IsNameTags ? "<color=green>NameTags: ON</color>" : "NameTags: OFF";
            if (GUILayout.Button(tagLabel, _buttonStyle))
                IsNameTags = !IsNameTags;
            if (IsNameTags)
            {
                string fpsLabel = IsFpsTags ? "<color=green>FPS Tags: ON</color>" : "FPS Tags: OFF";
                if (GUILayout.Button(fpsLabel, _buttonStyle))
                {
                    IsFpsTags = !IsFpsTags;
                }
            }
            GUILayout.Space(5f);
            GUILayout.Label("New Name", _labelStyle);
            _nameChange = GUILayout.TextField(_nameChange.ToUpper(), 12, _textFieldStyle);
            GUILayout.Space(4f);

            if (GUILayout.Button("Set Name", _buttonStyle))
                ChangeName(_nameChange);

            if (GUILayout.Button($"Change {_timeStr}", _buttonStyle))
            {
                ChangeTime();
            }

            var weatherEnumType = typeof(BetterDayNightManager.WeatherType);
            int totalWeathers = Enum.GetValues(weatherEnumType).Length;

            var currentWeather = (BetterDayNightManager.WeatherType)Enum.GetValues(weatherEnumType).GetValue(_weatherTypeIndex);

            if (GUILayout.Button("Current Weather: " + currentWeather, _buttonStyle))
            {
                _weatherTypeIndex = (_weatherTypeIndex + 1) % totalWeathers;
                var nextWeather = (BetterDayNightManager.WeatherType)Enum.GetValues(weatherEnumType).GetValue(_weatherTypeIndex);
                ChangeWeather(nextWeather);
            }
            
            GUILayout.EndArea();
        }
        
        private static void ScoreDisplay()
        {
            BeginMargin(ref _scoreForm, new Rect(20, 440, 300, 240));
            GUILayout.BeginArea(_scoreForm);

            GUI.Box(new Rect(0, 0, _scoreForm.width, _scoreForm.height), GUIContent.none, _windowStyle);

            GUILayout.Label("Scoreboard", new GUIStyle(_labelStyle)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            }, GUILayout.Width(_scoreForm.width));

            GUILayout.Label("(F) Add, (G) Subtract for Team 1 | (H) Add, (J) Subtract for Team 2",
                new GUIStyle(_labelStyle)
                {
                    richText = true,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                },
                GUILayout.Width(_scoreForm.width - 10));

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(_scoreForm.width / 2), GUILayout.Height(100));

            GUILayout.Label(_team1Name, new GUIStyle(_labelStyle)
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            });

            GUILayout.Space(5);

            if (_isEditingTeam1)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                _tempTeam1Name = GUILayout.TextField(_tempTeam1Name, _textFieldStyle,GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("✔", _buttonStyle, GUILayout.Width(30)))
                {
                    _team1Name = _tempTeam1Name;
                    _isEditingTeam1 = false;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            if (_spec == null && !_isEditingTeam1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Edit", _buttonStyle, GUILayout.Width(50)))
                {
                    _tempTeam1Name = _team1Name;
                    _isEditingTeam1 = true;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Label(_teamScore1.ToString(), new GUIStyle(_labelStyle)
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            });

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(_scoreForm.width / 2), GUILayout.Height(100));

            GUILayout.Label(_team2Name, new GUIStyle(_labelStyle)
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            });

            GUILayout.Space(5);

            if (_isEditingTeam2)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                _tempTeam2Name = GUILayout.TextField(_tempTeam2Name, _textFieldStyle, GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("✔", _buttonStyle, GUILayout.Width(30)))
                {
                    _team2Name = _tempTeam2Name;
                    _isEditingTeam2 = false;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            if (_spec == null && !_isEditingTeam2)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Edit", _buttonStyle, GUILayout.Width(50)))
                {
                    _tempTeam2Name = _team2Name;
                    _isEditingTeam2 = true;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Label(_teamScore2.ToString(), new GUIStyle(_labelStyle)
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            });

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea(); 
            EndMargin(ref _scoreForm);
        }

        private static void TimerGUI()
        {
            _savedTimerStyle = new GUIStyle(_labelStyle) { fontSize = 25, alignment = TextAnchor.MiddleCenter };

            if (!_timer) return;
            BeginMargin(ref _timerForm, new Rect(20, 860, 320, 200));

            GUILayout.BeginArea(_timerForm);
            GUILayout.Box("Stopwatch", _windowStyle, GUILayout.Width(_timerForm.width), GUILayout.Height(_timerForm.height));
            GUILayout.EndArea();
            EndMargin(ref _timerForm);

            GUILayout.BeginArea(_timerForm);
            GUILayout.BeginHorizontal();

            if (SavedTimes.Count != 0)
            {
                if (_stopWatchTime < SavedTimes.First())
                {
                    GUI.contentColor = Color.red;
                }
                else if (_stopWatchTime > SavedTimes.First())
                {
                    GUI.contentColor = Color.green;
                }
            }
            else
            {
                GUI.contentColor = Color.white;
            }

            GUILayout.Label(Watchtime(_stopWatchTime), new GUIStyle(_labelStyle) { fontSize = 25 }, GUILayout.Height(46));

            GUI.contentColor = Color.white;

            GUILayout.BeginVertical();

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            if (GUILayout.Button("Save", _buttonStyle, GUILayout.Height(20)) || Keyboard.current.mKey.wasPressedThisFrame)
            {
                if (SavedTimes.Count < 1)
                {
                    SavedTimes.Add(_stopWatchTime);
                }
                else
                {
                    SavedTimes.Clear();
                    SavedTimes.Add(_stopWatchTime);
                }
            }

            if (GUILayout.Button("Reset", _buttonStyle,GUILayout.Height(20)) || Keyboard.current.nKey.wasPressedThisFrame)
            {
                _timing = false;
                _stopWatchTime = -10f;
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start", _buttonStyle,GUILayout.Height(23)) || Keyboard.current.vKey.wasPressedThisFrame)
            {
                _timing = true;
            }

            if (GUILayout.Button("Stop",  _buttonStyle,GUILayout.Height(23)) || Keyboard.current.bKey.wasPressedThisFrame)
            {
                _timing = false;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            foreach (float time in SavedTimes)
            {
                GUILayout.Label(Watchtime(time), _savedTimerStyle, GUILayout.Height(26));
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear Saved Time", _buttonStyle))
            {
                SavedTimes.Clear();
            }

            GUILayout.EndArea();
        }

        private static Vector2 _settingsScrollPos; 

        private static void SettingsDisplay()
        {
            _settingsForm = new Rect(Screen.width - 270f, 335f, 260f, 390f);

            // Draw window box first
            GUI.Box(_settingsForm, "Settings", _windowStyle);

            // Padding inside the window
            Rect paddedRect = new Rect(_settingsForm.x + 10f, _settingsForm.y + 40f, _settingsForm.width - 20f, _settingsForm.height - 50f);

            GUILayout.BeginArea(paddedRect);
            
            var oldWidth = GUI.skin.verticalScrollbar.fixedWidth;
            GUI.skin.verticalScrollbar.fixedWidth = 0;

            _settingsScrollPos = GUILayout.BeginScrollView(_settingsScrollPos, false, true, GUILayout.Width(paddedRect.width), GUILayout.Height(paddedRect.height));

            // Your scrollable content:
            if (GUILayout.Button("Config: " + _availableConfigs[_selectedConfigIndex], _buttonStyle))
            {
                _selectedConfigIndex = (_selectedConfigIndex + 1) % _availableConfigs.Length;
                _selectedConfigName = _availableConfigs[_selectedConfigIndex];
                LoadSettings();
            }

            GUILayout.Label("Config Name:", _labelStyle);
            _configName = GUILayout.TextField(_configName, 10, _textFieldStyle);

            if (GUILayout.Button("Save Settings", _buttonStyle))
            {
                if (!string.IsNullOrWhiteSpace(_configName))
                {
                    _selectedConfigName = _configName;
                    SaveSettings();
                }
            }

            LabelSlider("FOV", ref _fov, 60f, 115f);
            LabelSlider("Smoothing", ref _smoothing, 0f, 1.5f);
            LabelSlider("Rig Lerp", ref _rigLerp, 0f, 0.5f);
            LabelSlider("X Offset", ref _specOffset.x, -10f, 10f);
            LabelSlider("Y Offset", ref _specOffset.y, -10f, 10f);
            LabelSlider("Z Offset", ref _specOffset.z, -10f, 10f);

            GUILayout.Space(3f);

            string labelerpp = _isHeadTracking ? "Head" : "Body";
            if (GUILayout.Button($"Tracking: {labelerpp}", _buttonStyle))
                _isHeadTracking = !_isHeadTracking;

            if (GUILayout.Button($"AutoCast: {_isAutoCast}", _buttonStyle))
                _isAutoCast = !_isAutoCast;

            if (_isAutoCast)
            {
                LabelSlider("CoolDown", ref _switchCooldown, 0.1f, 3f);
            }
            
            GUILayout.Space(3f);

            if (GUILayout.Button("Smoothing Type: " + _smoothingType, _buttonStyle))
                _smoothingType = (_smoothingType % 4) + 1;

            GUILayout.EndScrollView();

            // Restore vertical scrollbar width
            GUI.skin.verticalScrollbar.fixedWidth = oldWidth;

            GUILayout.EndArea();
        }
       
        private static void DistanceText()
        {
            if (_dist != 0 && _distanceDisplay && _spec != null)
            {
                GUI.Label(new Rect((float)Screen.width / 2 - 100f, Screen.height - 50f, 200, 50),
                    $"Closest Lava: {_dist.ToString("F0")}ft", new GUIStyle(_labelStyle) { fontSize = 18 });
            }
        }
        
        private static void ColorButton(string text, Color color, Action action)
        {
            GUI.contentColor = color;
            if (GUILayout.Button(text, _buttonStyle)) 
                action.Invoke();
            GUI.contentColor = Color.white;
        }

        private static void ColorButton(string text, Color color, Action action, float width, float height)
        {
            GUI.contentColor = color;
            if (GUILayout.Button(text, _buttonStyle, GUILayout.Width(width), GUILayout.Height(height))) 
                action.Invoke();
            GUI.contentColor = Color.white;
        }

        private static void LabelSlider(string label, ref float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value:F2}", new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = _labelTextColor },
                fontStyle = FontStyle.Bold,
            }, GUILayout.Width(120f));
            GUILayout.BeginVertical();
            GUILayout.Space(5f);
            value = GUIUtils.RoundedSlider(value, min, max, _sliderBackground, _sliderFill);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }


        private static void ChangeName(string name)
        {
            GorillaComputer.instance.currentName = name;
            PhotonNetwork.LocalPlayer.NickName = name;
            GorillaComputer.instance.savedName = name;
            PlayerPrefs.SetString("playerName", name);
            PlayerPrefs.Save();
        }

        private static void ScoreUpdater()
        {
            if (Keyboard.current.fKey.wasPressedThisFrame && _teamScore1 < 5) _teamScore1++;
            if (Keyboard.current.gKey.wasPressedThisFrame && _teamScore1 > 0) _teamScore1--;
            if (Keyboard.current.hKey.wasPressedThisFrame && _teamScore2 < 5) _teamScore2++;
            if (Keyboard.current.jKey.wasPressedThisFrame && _teamScore2 > 0) _teamScore2--;
        }
        
        private static void SetupCamera()
        {
            if (GTPlayer.Instance == null)
                return;

            GameObject existingShoulderCam = GameObject.Find("Shoulder Camera");

            if (_tpcObject == null)
            {
                _tpcObject = new GameObject("TPCamera");
                Tpc = _tpcObject.AddComponent<Camera>();

                SetupCamListener();
                
                _editClipPlane = Mathf.Lerp(_editClipPlane, DesiredClipPlane, 0.075f);
                Tpc.nearClipPlane = _editClipPlane;

                Tpc.cameraType = CameraType.Preview;

                PhotonNetworkController.Instance.disableAFKKick = true;

                Plugin.Tpc.cullingMask &= ~(1 << Plugin.MiniMapEspLayer);
                if (Camera.main != null)
                    Camera.main.cullingMask &= ~(1 << Plugin.MiniMapEspLayer);
            }
            else
            {
                Tpc.nearClipPlane = _editClipPlane;
            }
            
            if (!_deletedCamera && existingShoulderCam != null)
            {
                existingShoulderCam.SetActive(false);
                _deletedCamera = true;
            }
        }

        private static void SetupCamListener()
        {
            if (_tpcObject.GetComponent<AudioListener>() == null)
            {
                _camListener = _tpcObject.AddComponent<AudioListener>();
            }
            _camListener.enabled = _listener;
        }
        
        private static void NoSmoothRigs()
        {
            foreach (VRRig r in GorillaParent.instance.vrrigs)
            {
                if (r != null) {
                    if (r != GorillaTagger.Instance.offlineVRRig) {
                        r.lerpValueBody = _rigLerp;
                        r.lerpValueFingers = _rigLerp;
                    }

                    else if (_spec == null && _specRig == null) {
                        if (r == GorillaTagger.Instance.offlineVRRig) {
                            r.lerpValueBody = _rigLerp;
                            r.lerpValueFingers = _rigLerp;
                        }
                    }
                }
            }
        }

        private static void SkeletonEsp()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            Shader skeletonShader = Shader.Find("GUI/Text Shader");

            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig == null || rig.isOfflineVRRig || rig.skeleton == null || rig.skeleton.renderer == null)
                    continue;

                Color highlightColor = (rig.mainSkin.material.name.Contains("fected") || rig.mainSkin.material.name.Contains("It"))
                    ? Color.red
                    : new Color(0.1f, 1f, 0f);

                float t = Mathf.PingPong(Time.time, 1f);
                Color rigColor = Color.Lerp(highlightColor, Color.black, t);

                rig.skeleton.UpdateColor(rigColor);
                rig.skeleton.renderer.sharedMaterial.color = rigColor;
                rig.skeleton.renderer.sharedMaterial.shader = skeletonShader;
                
                rig.skeleton.gameObject.layer = MiniMapEspLayer;
                rig.skeleton.renderer.gameObject.layer = MiniMapEspLayer;

                rig.skeleton.enabled = true;
                rig.skeleton.renderer.enabled = true;
            }
        }

        private static void TurnOffSkeletonEsp()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            
            Shader defaultShader = Shader.Find("GorillaTag/UberShader");

            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig == null || rig.isOfflineVRRig || rig.skeleton == null || rig.skeleton.renderer == null)
                    continue;

                rig.skeleton.enabled = false;
                rig.skeleton.renderer.enabled = false;

                rig.skeleton.renderer.sharedMaterial.shader = defaultShader;
            }
        }
        
        private static void MiniMap()
        {
            if (_minimapRenderTexture == null)
            {
                _minimapRenderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                _minimapRenderTexture.Create();
            }

            if (_minimapCamera == null)
            {
                var minimapCameraObj = GameObject.Find("MinimapCamera") ?? new GameObject("MinimapCamera");
                _minimapCamera = minimapCameraObj.GetComponent<Camera>() ?? minimapCameraObj.AddComponent<Camera>();
                _minimapCamera.targetTexture = _minimapRenderTexture;
                Plugin._minimapCamera.clearFlags = CameraClearFlags.Color;
                Plugin._minimapCamera.backgroundColor = Color.clear;
                Plugin._minimapCamera.cullingMask = -1;
                _minimapCamera.enabled = false;
            }

            if (_spec == null)
            {
                var player = GTPlayer.Instance;
                if (player == null || player.headCollider == null)
                    return;

                var headTransform = player.headCollider.transform;
                if (headTransform == null)
                    return;

                _minimapCamera.transform.position = headTransform.position + Vector3.up * 12f;
                _minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                _minimapCamera.transform.parent = headTransform.parent;
            }
            else
            {
                _minimapCamera.transform.parent = _spec.transform.parent;
                _minimapCamera.transform.position = _spec.transform.position + Vector3.up * 15f;
            }

            _minimapCamera.Render();
        }

        private static Vector3 SmoothPosition(Vector3 current, Vector3 target)
        {
            float speed = Mathf.Max(_smoothing, 0.01f);
            float t = Mathf.Clamp01(Time.deltaTime * 6f / speed); // faster convergence
            _velocity = IsSpecNull ? GTPlayer.Instance.RigidbodyVelocity : _specRig.LatestVelocity();

            switch (_smoothingType)
            {
                case 1: // Basic Lerp
                    return Vector3.Lerp(current, target, t);

                case 2: // Exponential decay
                    return Vector3.Lerp(current, target, 1f - Mathf.Exp(-Time.deltaTime * 6f / speed));

                case 3: // SmoothDamp with time constant
                    return Vector3.SmoothDamp(current, target, ref _velocity, speed * 0.1f, Mathf.Infinity, Time.deltaTime);

                case 4: // Double Lerp for soft easing
                    Vector3 mid = Vector3.Lerp(current, (current + target) * 0.5f, t);
                    return Vector3.Lerp(mid, target, t);

                default:
                    return target;
            }
        }

        private static Quaternion SmoothRotation(Quaternion current, Quaternion target)
        {
            float speed = Mathf.Max(_smoothing, 0.01f);
            float t = Mathf.Clamp01(Time.deltaTime * 6f / speed);

            switch (_smoothingType)
            {
                case 1:
                    return Quaternion.Lerp(current, target, t);

                case 2:
                    return Quaternion.Lerp(current, target, 1f - Mathf.Exp(-Time.deltaTime * 6f / speed));

                case 3:
                    return Quaternion.Slerp(current, target, t);

                case 4:
                    Quaternion mid = Quaternion.Slerp(current, target, t * 0.5f);
                    return Quaternion.Slerp(mid, target, t);

                default:
                    return target;
            }
        }
        
        private static void SpecBackground()
        {
            
            if (_tpcObject == null || _minimapCamera == null || GTPlayer.Instance == null || GTPlayer.Instance.headCollider == null)
                return;
            
            DesiredClipPlane = IsSpecNull ? _defaultClipping : _targetClipping;
            if (_spec != null)
            {
                _targetClipping = _defaultClipping * _specRig.transform.localScale.y;
                _listener = true;
                GetDistToFected(ref _dist);

                Vector3 targetPos;

                if (_isHeadTracking)
                {
                    targetPos = _specRig.headMesh.transform.TransformPoint(_specOffset);
                }
                else
                {
                    targetPos = _spec.transform.TransformPoint(_specOffset);
                }

                if ((targetPos - _tpcObject.transform.position).sqrMagnitude > 10f)
                {
                    _tpcObject.transform.position = Vector3.Lerp(_tpcObject.transform.position, targetPos, 0.3f);
                }
                else
                {
                    _tpcObject.transform.position = SmoothPosition(_tpcObject.transform.position, targetPos);
                }

                var lookRot = Quaternion.LookRotation((_spec.transform.position - _tpcObject.transform.position).normalized);


                _tpcObject.transform.rotation = SmoothRotation(_tpcObject.transform.rotation, lookRot);
                _tpcObject.transform.parent = null;

                _minimapCamera.transform.parent = _spec.transform.parent;
                _minimapCamera.transform.position = _spec.transform.position + Vector3.up * 15f;
            }
            else
            {
                _targetClipping = _defaultClipping * GorillaTagger.Instance.offlineVRRig.transform.localScale.y;
                _listener = false;
                if (_spectating)
                {
                    _tpcObject.transform.position = GTPlayer.Instance.headCollider.transform.position +
                                                    GTPlayer.Instance.headCollider.transform.forward;
                    _tpcObject.transform.parent = GTPlayer.Instance.headCollider.transform;
                    _spectating = false;
                }
                else
                {
                    _minimapCamera.transform.position = GTPlayer.Instance.headCollider.transform.position + Vector3.up * 15f;
                    _minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    _minimapCamera.transform.parent = GTPlayer.Instance.headCollider.transform.parent;

                    if (_fpc)
                    { 
                        Vector3 testTarget;
                        if (_isHeadTracking)
                        {
                            testTarget = GTPlayer.Instance.headCollider.transform.TransformPoint(_specOffset);
                        }
                        else
                        {
                            testTarget = GTPlayer.Instance.bodyCollider.transform.TransformPoint(_specOffset);
                        }
                        _tpcObject.transform.position = SmoothPosition(_tpcObject.transform.position, testTarget);

                        var lookRotation= Quaternion.LookRotation(GTPlayer.Instance.headCollider.transform.position - _tpcObject.transform.position);
                        _tpcObject.transform.rotation = SmoothRotation(_tpcObject.transform.rotation, lookRotation);
                        _tpcObject.transform.parent = null;
                    }
                    else
                    {
                        Transform head = GTPlayer.Instance.headCollider.transform;
                        _tpcObject.transform.position = head.position;
                        _tpcObject.transform.parent = head;
                        _tpcObject.transform.rotation = SmoothRotation(_tpcObject.transform.rotation, head.rotation);
                    }
                }
            }

            var tpc = _tpcObject.GetComponent<Camera>();
            if (tpc is not null)
            {
                tpc.fieldOfView = _fov;
            }
        }

        
        private static void NumberSpectate()
        {
            bool[] keys = 
            {
                Keyboard.current.digit1Key.wasPressedThisFrame,
                Keyboard.current.digit2Key.wasPressedThisFrame,
                Keyboard.current.digit3Key.wasPressedThisFrame,
                Keyboard.current.digit4Key.wasPressedThisFrame,
                Keyboard.current.digit5Key.wasPressedThisFrame,
                Keyboard.current.digit6Key.wasPressedThisFrame,
                Keyboard.current.digit7Key.wasPressedThisFrame,
                Keyboard.current.digit8Key.wasPressedThisFrame,
                Keyboard.current.digit9Key.wasPressedThisFrame
            };
            bool wasPressedThisFrame = Keyboard.current.digit0Key.wasPressedThisFrame;
            for (int i = 0; i < keys.Length; i++) {
                if (keys[i] &&  i < _playerList.Count) {
                    if (_playerList != null)
                    {
                        _spec = _playerList[i].gameObject;
                        _specRig = _playerList[i];   
                    }
                    break;
                }
            }

            if (wasPressedThisFrame && _spec != null)
            {
                _spec = null;
                _specRig = null;
            }
        }

        private static void AutoCast()
        {
            float minDistance = float.MaxValue;
            float currentTime = Time.time;

            if (currentTime - _lastSwitchTime < _switchCooldown)
            {
                return;
            }

            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != null && !vrrig.mainSkin.material.name.Contains("fected") && !vrrig.mainSkin.material.name.Contains("It"))
                {
                    foreach (VRRig fectedVrrig in GorillaParent.instance.vrrigs)
                    {
                        if (fectedVrrig != null && fectedVrrig.mainSkin.material.name.Contains("fected") || fectedVrrig.mainSkin.material.name.Contains("It"))
                        {
                            float distance = Vector3.Distance(vrrig.transform.position, fectedVrrig.transform.position);

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                _specRig = vrrig;
                                _spec = vrrig.gameObject;

                                _lastSwitchTime = currentTime;
                            }
                        }
                    }
                }
            }
        }
        
        private static void Wasd()
        {
	        Transform transform = GTPlayer.Instance.headCollider.transform;
	        bool[] array =
            [
                Keyboard.current.wKey.isPressed,
		        Keyboard.current.aKey.isPressed,
		        Keyboard.current.sKey.isPressed,
		        Keyboard.current.dKey.isPressed,
		        Keyboard.current.spaceKey.isPressed,
		        Keyboard.current.leftShiftKey.isPressed,
		        Keyboard.current.leftCtrlKey.isPressed
            ];
	        _maxSpeed = (array[6] ? 10f : 5f);
	        for (var i = 0; i < 6; i++)
	        {
		        var list = Speeds;
                list[i] += (array[i] ? 1 : (-1)) * Time.deltaTime * 15f;
		        Speeds[i] = Mathf.Clamp(Speeds[i], 0f, _maxSpeed);
		        bool flag = Speeds[i] != 0f;
		        if (flag)
		        {
			        switch (i)
			        {
			        case 0:
				        transform.position += transform.forward * (Speeds[i] * Time.deltaTime);
				        break;
			        case 1:
				        transform.position -= transform.right * (Speeds[i] * Time.deltaTime);
				        break;
			        case 2:
				        transform.position -= transform.forward * (Speeds[i] * Time.deltaTime);
				        break;
			        case 3:
				        transform.position += transform.right * (Speeds[i] * Time.deltaTime);
				        break;
			        case 4:
				        transform.position += transform.up * (Speeds[i] * Time.deltaTime);
				        break;
			        case 5:
				        transform.position -= transform.up * (Speeds[i] * Time.deltaTime);
				        break;
			        }
		        }
	        }
	        bool isPressed = Mouse.current.rightButton.isPressed;
	        if (isPressed)
	        {
		        Vector2 currentMousePos = Mouse.current.position.ReadValue();
		        if (_lastMousePosition != Vector2.zero)
		        {
			        Vector2 vector2 = currentMousePos - _lastMousePosition;
			        float num2 = vector2.x * 0.3f;
			        GTPlayer.Instance.Turn(num2);
		        }
		        _lastMousePosition = currentMousePos;
	        }
	        else
	        {
		        _lastMousePosition = Vector2.zero;
	        }
	        if (Mouse.current.leftButton.isPressed && !_fpc)
	        {
                LayerMask layerMask = LayerMask.GetMask(new[] { "Gorilla Trigger", "Zone", "Gorilla Body" });
                if (Camera.main != null)
                {
                    Ray ray = (GorillaTagger.Instance.thirdPersonCamera.activeInHierarchy ? GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue()) : Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
                    RaycastHit raycastHit;
                    if (Physics.Raycast(ray, out raycastHit, 20f, ~layerMask))
                    {
                        GorillaTagger.Instance.rightHandTriggerCollider.transform.position = raycastHit.point;
                    }
                }
            }
	        else
	        {
		        GorillaTagger.Instance.rightHandTransform.position = GorillaTagger.Instance.bodyCollider.transform.position;
	        }
	        GorillaTagger.Instance.leftHandTransform.position = GorillaTagger.Instance.bodyCollider.transform.position;
	        GTPlayer.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
        }

        private static string Watchtime(float time)
        {
            bool isNegative = time < 0;
            time = Mathf.Abs(time);

            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            float milliseconds = (time * 1000) % 1000;

            string formattedTime = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);

            if (isNegative)
            {
                formattedTime = "-" + formattedTime;
            }

            return formattedTime;
        }

        private static void GetDistToFected(ref float output)
        {
            if (output < 0) throw new ArgumentOutOfRangeException(nameof(output));
            float maxDist = float.MaxValue;
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != null && (vrrig.mainSkin.material.name.Contains("fected") || vrrig.mainSkin.material.name.Contains("It")))
                {
                    float dist = Vector3.Distance(_spec.transform.position, vrrig.transform.position);
                    if (dist < maxDist)
                    {
                        maxDist = dist;
                    }
                }
            }

            output = maxDist;
        }

        private static void SwitchBool(ref bool flip)
        {
            flip = !flip;
        }

        private static void CloseMenu(ref bool wow)
        {
            wow = !wow;
        }
    }
}
