﻿using TournamentAssistantShared.Models;
using TournamentAssistantShared.Models.Packets;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using TournamentAssistantShared.Utilities;

namespace TournamentAssistantUI.UI.UserControls
{
    /// <summary>
    /// Interaction logic for UserDialog.xaml
    /// </summary>
    public partial class GameOverDialogTeams : UserControl
    {
        public class TeamResult
        {
            public Team Team { get; set; }
            public List<User> Players { get; set; }
            public int TotalScore { get; set; } = 0;
            public string IndividualScores
            {
                get
                {
                    var rankIndex = 1;
                    var totalScoreText = string.Empty;
                    Players.OrderByDescending(x => x.Score).ToList().ForEach(x => totalScoreText += $"{rankIndex++}: {x.Name} - {x.Score}\n");
                    return totalScoreText;
                }
            }
        }

        public List<TeamResult> TeamResults { get; set; }

        public GameOverDialogTeams(List<LocalizedResult> results)
        {
            TeamResults = new List<TeamResult>();

            results.ForEach(r =>
            {
                var x = r.result as SongFinished;
                var teamResult = TeamResults.FirstOrDefault(y => y.Team.Id == x.Player.Team.Id);

                //If there's no team in the results list for the current player
                if (teamResult == null)
                {
                    teamResult = new TeamResult()
                    {
                        Team = x.Player.Team,
                        Players = new List<User>()
                    };
                    TeamResults.Add(teamResult);
                }

                x.Player.Score = x.Score;
                teamResult.Players.Add(x.Player);
                teamResult.TotalScore += x.Score;
            });

            TeamResults = TeamResults.OrderByDescending(x => x.TotalScore).ToList();

            DataContext = this;

            InitializeComponent();
        }

        private void Copy_Click(object _, RoutedEventArgs __)
        {
            var copyToClipboard = "结果:\n";
            var index = 1;

            foreach (var result in TeamResults)
            {
                copyToClipboard += $"{index}: {result.Team.Name} - {result.TotalScore}\n";
                foreach (var player in result.Players)
                {
                    copyToClipboard += $"\t\t{player.Name} - {player.Score}\n";
                }
                copyToClipboard += "\n";
            }

            Clipboard.SetText(copyToClipboard);
        }
    }
}
