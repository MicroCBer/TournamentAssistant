using System.Windows.Controls;
using System.Windows.Input;
using TournamentAssistantShared.Models;
using TournamentAssistantShared.Utilities;

namespace TournamentAssistantUI.UI.UserControls
{
    /// <summary>
    /// Interaction logic for UserDialog.xaml
    /// </summary>
    public partial class UserDialog : UserControl
    {
        public User User { get; set; }

        public string translatePlayState {
            get { return Localizer.TranslatePlayState(this.User.PlayState, Localizer.Language.Chinese_Simplified) ?? ""; }
            set { }
        }

        public string translateDownloadState
        {
            get { return Localizer.TranslateDownloadState(this.User.DownloadState, Localizer.Language.Chinese_Simplified) ?? ""; }
            set { }
        }

        public string translateTeamName
        {
            get { return Localizer.TranslateTeamName(this.User.Team.Name, Localizer.Language.Chinese_Simplified) ?? ""; }
            set { }
        }

        public ICommand KickPlayer { get; set; }

        public UserDialog(User user, ICommand kickPlayer)
        {
            User = user;
            KickPlayer = kickPlayer;

            DataContext = this;

            InitializeComponent();
        }
    }
}
