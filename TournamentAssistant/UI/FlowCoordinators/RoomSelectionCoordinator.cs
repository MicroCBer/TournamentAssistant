﻿using BeatSaberMarkupLanguage;
using HMUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using TournamentAssistant.Misc;
using TournamentAssistant.UI.ViewControllers;
using TournamentAssistantShared;
using TournamentAssistantShared.Models;
using TournamentAssistantShared.Models.Packets;
using TournamentAssistantShared.Utilities;

namespace TournamentAssistant.UI.FlowCoordinators
{
    class RoomSelectionCoordinator : FlowCoordinatorWithClient
    {
        private SplashScreen _splashScreen;
        private RoomSelection _roomSelection;
        private RoomCoordinator _roomCoordinator;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (addedToHierarchy)
            {
                //Set up UI
                SetTitle("大厅", ViewController.AnimationType.None);
                showBackButton = true;

                _roomSelection = BeatSaberUI.CreateViewController<RoomSelection>();
                _roomSelection.MatchSelected += RoomSelection_MatchSelected;
                _roomSelection.CreateMatchPressed += RoomSelection_MatchCreated;

                _splashScreen = BeatSaberUI.CreateViewController<SplashScreen>();
                _splashScreen.StatusText = $"正在连接 \"{Host.Name}\"...";

                ProvideInitialViewControllers(_splashScreen);
            }

            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        public override void Dismiss()
        {
            if (_roomCoordinator != null && IsFlowCoordinatorInHierarchy(_roomCoordinator)) _roomCoordinator.Dismiss();
            if (topViewController is RoomSelection) DismissViewController(topViewController, immediately: true);

            base.Dismiss();
        }

        protected override async Task Client_ConnectedToServer(ConnectResponse response)
        {
            await base.Client_ConnectedToServer(response);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _roomSelection.SetMatches(Plugin.client.State.Matches.ToList());
                PresentViewController(_roomSelection, immediately: true);
            });
        }

        protected override async Task Client_FailedToConnectToServer(ConnectResponse response)
        {
            await base.Client_FailedToConnectToServer(response);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _splashScreen.StatusText = !string.IsNullOrEmpty(response?.Response.Message) ? response.Response.Message : "初始化连接失败，正在重试...";
            });
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            Dismiss();
        }

        protected override async Task Client_MatchCreated(Match match)
        {
            await base.Client_MatchCreated(match);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _roomSelection.SetMatches(Plugin.client.State.Matches.ToList());
            });
        }

        //The match list item is used in click events, so it needs to be up to date as possible
        //(ie: have the most recent player lists) This shouln't get score updates as they're only directed to match players
        //We could potentially refresh the view here too if we ever include updatable data in the texts
        protected override async Task Client_MatchInfoUpdated(Match natch)
        {
            await base.Client_MatchInfoUpdated(natch);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _roomSelection.SetMatches(Plugin.client.State.Matches.ToList());
            });
        }

        protected override async Task Client_MatchDeleted(Match match)
        {
            await base.Client_MatchDeleted(match);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _roomSelection.SetMatches(Plugin.client.State.Matches.ToList());
            });
        }

        private void RoomSelection_MatchCreated()
        {
            var player = Plugin.client.State.Users.FirstOrDefault(x => x.UserEquals(Plugin.client.Self));
            var match = new Match()
            {
                Guid = Guid.NewGuid().ToString(),
                Leader = Plugin.client.Self,
            };
            match.AssociatedUsers.Add(player);

            //As of the async refactoring, this *shouldn't* cause problems to not await. It would be very hard to properly use async from a UI event so I'm leaving it like this for now
            Task.Run(() => Plugin.client.CreateMatch(match));

            _roomCoordinator = BeatSaberUI.CreateFlowCoordinator<RoomCoordinator>();
            _roomCoordinator.DidFinishEvent += RoomCoordinator_DidFinishEvent;
            _roomCoordinator.Match = match;
            PresentFlowCoordinator(_roomCoordinator);
        }

        private void RoomCoordinator_DidFinishEvent()
        {
            _roomCoordinator.DidFinishEvent -= RoomCoordinator_DidFinishEvent;

            //If we're marked to dismiss ourself, we should do so as soon as our child coordinator returns to us
            void onComplete()
            {
                if (ShouldDismissOnReturnToMenu) Dismiss();
            }

            DismissFlowCoordinator(_roomCoordinator, finishedCallback: onComplete);
        }

        private void RoomSelection_MatchSelected(Match match)
        {
            //Add ourself to the match and send the update
            var player = Plugin.client.State.Users.FirstOrDefault(x => x.UserEquals(Plugin.client.Self));
            match.AssociatedUsers.Add(player);

            //As of the async refactoring, this *shouldn't* cause problems to not await. It would be very hard to properly use async from a UI event so I'm leaving it like this for now
            Task.Run(() => Plugin.client.UpdateMatch(match));

            _roomCoordinator = BeatSaberUI.CreateFlowCoordinator<RoomCoordinator>();
            _roomCoordinator.DidFinishEvent += RoomCoordinator_DidFinishEvent;

            _roomCoordinator.Match = match;
            PresentFlowCoordinator(_roomCoordinator);
        }
    }
}
