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
    public partial class GameOverDialog : UserControl
    {
        public List<LocalizedResult> Results { get; set; }

        public GameOverDialog(List<LocalizedResult> results)
        {
            Results = results.OrderByDescending(x => x.result.Score).ToList();

            DataContext = this;

            InitializeComponent();
        }

        private void Copy_Click(object _, RoutedEventArgs __)
        {
            var copyToClipboard = "结果:\n";

            var index = 1;
            foreach (var result in Results) copyToClipboard += $"{index++}: {result.result.Player.Name} - {result.result.Score}\n";

            Clipboard.SetText(copyToClipboard);
        }
    }
}
