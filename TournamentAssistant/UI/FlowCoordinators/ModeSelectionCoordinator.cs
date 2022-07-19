﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using System;
using System.Threading.Tasks;
using TournamentAssistant.Misc;
using TournamentAssistant.UI.ViewControllers;
using TournamentAssistantShared;
using UnityEngine;

namespace TournamentAssistant.UI.FlowCoordinators
{
    class ModeSelectionCoordinator : FlowCoordinator, IFinishableFlowCoordinator
    {
        public event Action DidFinishEvent;

        private EventSelectionCoordinator _eventSelectionCoordinator;
        private ServerSelectionCoordinator _serverSelectionCoordinator;
        private ServerModeSelection _serverModeSelectionViewController;
        private PatchNotes _patchNotesViewController;
        private ServerMessage _serverMessage;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (addedToHierarchy)
            {
                SetTitle($"比赛助手 v{Constants.VERSION}");
                showBackButton = true;

                _patchNotesViewController = BeatSaberUI.CreateViewController<PatchNotes>();
                _serverModeSelectionViewController = BeatSaberUI.CreateViewController<ServerModeSelection>();
                _serverModeSelectionViewController.BattleSaberButtonPressed += ServerModeSelectionViewController_BattleSaberButtonPressed;
                _serverModeSelectionViewController.QualifierButtonPressed += ServerModeSelectionViewController_QualifierButtonPressed;
                _serverModeSelectionViewController.TournamentButtonPressed += ServerModeSelectionViewController_TournamentButtonPressed;

                ProvideInitialViewControllers(_serverModeSelectionViewController, null, _patchNotesViewController);

                //Check for updates before contacting a server
                Task.Run(CheckForUpdate);
            }
        }

        private async void CheckForUpdate()
        {
            var newVersion = await Update.GetLatestRelease();
            if (Version.Parse(Constants.VERSION) < newVersion)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    var message = new TournamentAssistantShared.Models.Packets.Message()
                    {
                        MessageTitle = "发现新版本",
                        MessageText = $"需要更新插件! 当前版本 \'{Constants.VERSION}\', 新版本 \'{newVersion}\'\n" +
                            $"请访问 https://bsc.beatsaberchina.com/getTA 或者联系主办方\n" +
                            $"获取最新版本"
                    };
                    _serverMessage = BeatSaberUI.CreateViewController<ServerMessage>();
                    _serverMessage.SetMessage(message);

                    FloatingScreen screen = FloatingScreen.CreateFloatingScreen(new Vector2(100, 50), false, new Vector3(0f, 0.9f, 2.4f), Quaternion.Euler(30f, 0f, 0f));
                    screen.SetRootViewController(_serverMessage, ViewController.AnimationType.None);
                });
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            if (_serverMessage?.screen) Destroy(_serverMessage.screen.gameObject);

            DidFinishEvent?.Invoke();
        }

        private void ServerModeSelectionViewController_BattleSaberButtonPressed()
        {
            _serverSelectionCoordinator = BeatSaberUI.CreateFlowCoordinator<ServerSelectionCoordinator>();
            _serverSelectionCoordinator.DestinationCoordinator = BeatSaberUI.CreateFlowCoordinator<RoomSelectionCoordinator>();
            _serverSelectionCoordinator.DidFinishEvent += ServerSelectionCoordinator_DidFinishEvent;
            PresentFlowCoordinator(_serverSelectionCoordinator);
        }

        private void ServerModeSelectionViewController_QualifierButtonPressed()
        {
            _eventSelectionCoordinator = BeatSaberUI.CreateFlowCoordinator<EventSelectionCoordinator>();
            _eventSelectionCoordinator.RescrapeForSecondaryEvents = true;
            _eventSelectionCoordinator.DidFinishEvent += EventSelectionCoordinator_DidFinishEvent;
            PresentFlowCoordinator(_eventSelectionCoordinator);
        }

        private void ServerModeSelectionViewController_TournamentButtonPressed()
        {
            _serverSelectionCoordinator = BeatSaberUI.CreateFlowCoordinator<ServerSelectionCoordinator>();
            _serverSelectionCoordinator.DestinationCoordinator = BeatSaberUI.CreateFlowCoordinator<RoomCoordinator>();
            _serverSelectionCoordinator.DidFinishEvent += ServerSelectionCoordinator_DidFinishEvent;
            PresentFlowCoordinator(_serverSelectionCoordinator);
        }

        private void ServerSelectionCoordinator_DidFinishEvent()
        {
            _serverSelectionCoordinator.DidFinishEvent -= ServerSelectionCoordinator_DidFinishEvent;
            DismissFlowCoordinator(_serverSelectionCoordinator);
        }

        private void EventSelectionCoordinator_DidFinishEvent()
        {
            _eventSelectionCoordinator.DidFinishEvent -= EventSelectionCoordinator_DidFinishEvent;
            DismissFlowCoordinator(_eventSelectionCoordinator);
        }
    }
}
