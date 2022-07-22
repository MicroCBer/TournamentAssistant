#pragma warning disable IDE0052
using BeatSaberMarkupLanguage;
using HMUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using TournamentAssistant.Misc;
using TournamentAssistant.UI.ViewControllers;
using TournamentAssistant.Utilities;
using TournamentAssistantShared;
using TournamentAssistantShared.Models;
using TournamentAssistantShared.Models.Packets;
using UnityEngine;
using static TournamentAssistantShared.Models.GameplayModifiers;
using static TournamentAssistantShared.Models.PlayerSpecificSettings;

namespace TournamentAssistant.UI.FlowCoordinators
{
    class QualifierCoordinator : FlowCoordinator
    {
        public event Action DidFinishEvent;

        public QualifierEvent Event { get; set; }
        public CoreServer EventHost { get; set; }

        private SongSelection _songSelection;
        private SongDetail _songDetail;

        private GameplayParameters _currentParameters;
        private IBeatmapLevel _lastPlayedBeatmapLevel;
        private BeatmapCharacteristicSO _lastPlayedCharacteristic;
        private BeatmapDifficulty _lastPlayedDifficulty;

        private PlayerDataModel _playerDataModel;
        private MenuLightsManager _menuLightsManager;
        private SoloFreePlayFlowCoordinator _soloFreePlayFlowCoordinator;
        private CampaignFlowCoordinator _campaignFlowCoordinator;

        private PlatformLeaderboardViewController _globalLeaderboard;
        private CustomLeaderboard _customLeaderboard;
        private ResultsViewController _resultsViewController;
        private MenuLightsPresetSO _scoreLights;
        private MenuLightsPresetSO _redLights;
        private MenuLightsPresetSO _defaultLights;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("资格赛房间", ViewController.AnimationType.None);
                showBackButton = true;

                _playerDataModel = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First();
                _menuLightsManager = Resources.FindObjectsOfTypeAll<MenuLightsManager>().First();
                _soloFreePlayFlowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
                _campaignFlowCoordinator = Resources.FindObjectsOfTypeAll<CampaignFlowCoordinator>().First();
                _resultsViewController = Resources.FindObjectsOfTypeAll<ResultsViewController>().First();
                _scoreLights = _soloFreePlayFlowCoordinator.GetField<MenuLightsPresetSO>("_resultsClearedLightsPreset");
                _redLights = _soloFreePlayFlowCoordinator.GetField<MenuLightsPresetSO>("_resultsFailedLightsPreset");
                _defaultLights = _soloFreePlayFlowCoordinator.GetField<MenuLightsPresetSO>("_defaultLightsPreset");

                _songSelection = BeatSaberUI.CreateViewController<SongSelection>();
                _songSelection.SongSelected += SongSelection_SongSelected;

                _songDetail = BeatSaberUI.CreateViewController<SongDetail>();
                _songDetail.PlayPressed += SongDetail_didPressPlayButtonEvent;
                _songDetail.DisableCharacteristicControl = true;
                _songDetail.DisableDifficultyControl = true;
                _songDetail.DisablePlayButton = false;

                _customLeaderboard = BeatSaberUI.CreateViewController<CustomLeaderboard>();
            }
            if (addedToHierarchy)
            {
                _songSelection.SetSongs(Event.QualifierMaps.ToList());
                ProvideInitialViewControllers(_songSelection);
            }
        }

        private void SongDetail_didPressPlayButtonEvent(IBeatmapLevel level, BeatmapCharacteristicSO characteristic, BeatmapDifficulty difficulty)
        {
            _lastPlayedBeatmapLevel = level;
            _lastPlayedCharacteristic = characteristic;
            _lastPlayedDifficulty = difficulty;

            var playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
            var playerSettings = playerData.playerSpecificSettings;

            //Override defaults if we have forced options enabled
            if (_currentParameters.PlayerSettings.Options != PlayerOptions.None)
            {
                playerSettings = new PlayerSpecificSettings(
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.LeftHanded),
                        _currentParameters.PlayerSettings.PlayerHeight,
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.AutoPlayerHeight),
                        _currentParameters.PlayerSettings.SfxVolume,
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.ReduceDebris),
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.NoHud),
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.NoFailEffects),
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.AdvancedHud),
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.AutoRestart),
                        _currentParameters.PlayerSettings.SaberTrailIntensity,
                        (NoteJumpDurationTypeSettings)_currentParameters.PlayerSettings.note_jump_duration_type_settings,
                        _currentParameters.PlayerSettings.NoteJumpFixedDuration,
                        _currentParameters.PlayerSettings.NoteJumpStartBeatOffset,
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.HideNoteSpawnEffect),
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.AdaptiveSfx),
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.StaticLights) ? EnvironmentEffectsFilterPreset.NoEffects : EnvironmentEffectsFilterPreset.AllEffects,
                        _currentParameters.PlayerSettings.Options.HasFlag(PlayerOptions.StaticLights) ? EnvironmentEffectsFilterPreset.NoEffects : EnvironmentEffectsFilterPreset.AllEffects
                    );
            }

            var songSpeed = GameplayModifiers.SongSpeed.Normal;
            if (_currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.SlowSong)) songSpeed = GameplayModifiers.SongSpeed.Slower;
            if (_currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.FastSong)) songSpeed = GameplayModifiers.SongSpeed.Faster;
            if (_currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.SuperFastSong)) songSpeed = GameplayModifiers.SongSpeed.SuperFast;

            var gameplayModifiers = new GameplayModifiers(
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.BatteryEnergy) ? GameplayModifiers.EnergyType.Battery : GameplayModifiers.EnergyType.Bar,
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.NoFail),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.InstaFail),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.FailOnClash),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.NoObstacles) ? GameplayModifiers.EnabledObstacleType.NoObstacles : GameplayModifiers.EnabledObstacleType.All,
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.NoBombs),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.FastNotes),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.StrictAngles),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.DisappearingArrows),
                songSpeed,
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.NoArrows),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.GhostNotes),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.ProMode),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.ZenMode),
                _currentParameters.GameplayModifiers.Options.HasFlag(GameOptions.SmallCubes)
            );

            var colorScheme = playerData.colorSchemesSettings.overrideDefaultColors ? playerData.colorSchemesSettings.GetSelectedColorScheme() : null;

            //Disable scores if we need to
            if (((QualifierEvent.EventSettings)Event.Flags).HasFlag(QualifierEvent.EventSettings.DisableScoresaberSubmission)) BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Constants.NAME);

            SongUtils.PlaySong(level, characteristic, difficulty, playerData.overrideEnvironmentSettings, colorScheme, gameplayModifiers, playerSettings, SongFinished);
        }

        private void SongSelection_SongSelected(GameplayParameters parameters)
        {
            _currentParameters = parameters;

            SongUtils.LoadSong(parameters.Beatmap.LevelId, (loadedLevel) =>
            {
                PresentViewController(_songDetail, () =>
                {
                    _songDetail.SetSelectedSong(loadedLevel);
                    _songDetail.SetSelectedDifficulty((int)parameters.Beatmap.Difficulty);
                    _songDetail.SetSelectedCharacteristic(parameters.Beatmap.Characteristic.SerializedName);

                    if (_globalLeaderboard == null)
                    {
                        _globalLeaderboard = Resources.FindObjectsOfTypeAll<PlatformLeaderboardViewController>().First();
                        _globalLeaderboard.name = "总排行榜";
                    }

                    _globalLeaderboard.SetData(SongUtils.GetClosestDifficultyPreferLower(loadedLevel, (BeatmapDifficulty)(int)parameters.Beatmap.Difficulty, parameters.Beatmap.Characteristic.SerializedName));
                    SetRightScreenViewController(_globalLeaderboard, ViewController.AnimationType.In);

                    //TODO: Review whether this could cause issues. Probably need debouncing or something similar
                    foreach (var set in loadedLevel.beatmapLevelData.difficultyBeatmapSets)
                    {
                       TournamentAssistantShared.Logger.Warning($"{set.beatmapCharacteristic.serializedName},{parameters.Beatmap.Characteristic.SerializedName}");
                        if(set.beatmapCharacteristic.serializedName.Equals(parameters.Beatmap.Characteristic.SerializedName))
                        {
                            TournamentAssistantShared.Logger.Warning(set.difficultyBeatmaps);
                            TournamentAssistantShared.Logger.Warning(parameters.Beatmap.Difficulty);
                            foreach(var difficulty in set.difficultyBeatmaps)
                            {
                                if (difficulty.difficulty.Equals((BeatmapDifficulty)(int)parameters.Beatmap.Difficulty))
                                {
                                    Task.Run(() => PlayerUtils.GetPlatformUserData(RequestLeaderboardWhenResolved, difficulty));
                                    SetLeftScreenViewController(_customLeaderboard, ViewController.AnimationType.In);
                                    return;
                                }
                            }
                           
                        }
                    }
                    
                });
            });
        }

        private void ResultsViewController_continueButtonPressedEvent(ResultsViewController results)
        {
            _resultsViewController.continueButtonPressedEvent -= ResultsViewController_continueButtonPressedEvent;
            _menuLightsManager.SetColorPreset(_defaultLights, true);
            DismissViewController(_resultsViewController);
        }

        private void ResultsViewController_restartButtonPressedEvent(ResultsViewController results)
        {
            _resultsViewController.continueButtonPressedEvent -= ResultsViewController_continueButtonPressedEvent;
            _menuLightsManager.SetColorPreset(_defaultLights, true);
            DismissViewController(_resultsViewController, finishedCallback: () => SongDetail_didPressPlayButtonEvent(_lastPlayedBeatmapLevel, _lastPlayedCharacteristic, _lastPlayedDifficulty));
        }

        public void SongFinished(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData, LevelCompletionResults results)
        {
            standardLevelScenesTransitionSetupData.didFinishEvent -= SongFinished;

            var map = standardLevelScenesTransitionSetupData.difficultyBeatmap;
            var transformedMap = standardLevelScenesTransitionSetupData.transformedBeatmapData;
            var localPlayer = _playerDataModel.playerData;
            var localResults = localPlayer.GetPlayerLevelStatsData(map.level.levelID, map.difficulty, map.parentDifficultyBeatmapSet.beatmapCharacteristic);
            var highScore = localResults.highScore < results.modifiedScore;

            if (results.levelEndAction == LevelCompletionResults.LevelEndAction.Restart) SongDetail_didPressPlayButtonEvent(_lastPlayedBeatmapLevel, _lastPlayedCharacteristic, _lastPlayedDifficulty);
            else if (results.levelEndStateType != LevelCompletionResults.LevelEndStateType.Incomplete)
            {
                if (results.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
                {
                    Task.Run(() => PlayerUtils.GetPlatformUserData((username, userId) => SubmitScoreWhenResolved(username, userId, results, map)));

                    _menuLightsManager.SetColorPreset(_scoreLights, true);
                    _resultsViewController.Init(results, transformedMap, map, false, highScore);
                    _resultsViewController.continueButtonPressedEvent += ResultsViewController_continueButtonPressedEvent;
                    _resultsViewController.restartButtonPressedEvent += ResultsViewController_restartButtonPressedEvent;
                }
                else if (results.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed)
                {
                    _menuLightsManager.SetColorPreset(_redLights, true);
                    _resultsViewController.Init(results, transformedMap, map, false, highScore);
                    _resultsViewController.continueButtonPressedEvent += ResultsViewController_continueButtonPressedEvent;
                    _resultsViewController.restartButtonPressedEvent += ResultsViewController_restartButtonPressedEvent;
                }

                PresentViewController(_resultsViewController, immediately: true);
            }
        }

        private Task SubmitScoreWhenResolved(string username, ulong userId, LevelCompletionResults results, IDifficultyBeatmap map)
        {
            Task.Run(async () =>
            {
                var scores = ((await HostScraper.RequestResponse(EventHost, new Packet
                {
                    SubmitScore = new SubmitScore
                    {
                        Score = new Score
                        {
                            EventId = Event.EventId,
                            Parameters = _currentParameters,
                            UserId = userId.ToString(),
                            Username = username,
                            FullCombo = results.fullCombo,
                            score = results.modifiedScore,
                            Color = "#ffffff"
                        }
                    }
                },
                Packet.packetOneofCase.ScoreRequestResponse,
                username, userId)).ScoreRequestResponse).Scores.Take(10).ToArray();
                var info = await map.GetBeatmapDataBasicInfoAsync();

                UnityMainThreadDispatcher.Instance().Enqueue(() => SetCustomLeaderboardScores(scores, userId,info.cuttableNotesCount * 920 - 7245));
            });
            return Task.CompletedTask;
        }

        private Task RequestLeaderboardWhenResolved(string username, ulong userId, IDifficultyBeatmap level)
        {
            //Don't scrape on main thread
            Task.Run(async () =>
            {
                var scores = (await HostScraper.RequestResponse(EventHost, new Packet
                {
                    ScoreRequest = new ScoreRequest
                    {
                        EventId = Event.EventId,
                        Parameters = _currentParameters
                    }
                },
                Packet.packetOneofCase.ScoreRequestResponse,
                username, userId)).ScoreRequestResponse.Scores.Take(10).ToArray();

                var info=await level.GetBeatmapDataBasicInfoAsync();
                

                UnityMainThreadDispatcher.Instance().Enqueue(() => SetCustomLeaderboardScores(scores, userId, info.cuttableNotesCount * 920 - 7245));
            });
            return Task.CompletedTask;
        }

        public void SetCustomLeaderboardScores(Score[] scores, ulong userId, int maxScore)
        {
            var place = 1;
            var indexOfme = -1;
            _customLeaderboard.SetScores(scores.Select(x =>
            {
                if (x.UserId == userId.ToString()) indexOfme = place - 1;
                TournamentAssistantShared.Logger.Info($"Score: {x.score} MaxScore:{maxScore}");
                return new LeaderboardTableView.ScoreData(x.score, x.Username+"\t\t\t" + ((double)x.score / maxScore * 100).ToString("N3")+"%", place++, x.FullCombo);
            }).ToList(), indexOfme);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            if (_resultsViewController.isInViewControllerHierarchy)
            {
                _menuLightsManager.SetColorPreset(_defaultLights, false);
                DismissViewController(_resultsViewController);
            }
            else if (_songDetail.isInViewControllerHierarchy)
            {
                SetLeftScreenViewController(null, ViewController.AnimationType.Out);
                SetRightScreenViewController(null, ViewController.AnimationType.Out);
                DismissViewController(_songDetail);
            }
            else DidFinishEvent?.Invoke();
        }
    }
}
