using System.Collections.Generic;
using TournamentAssistantShared.Models;

namespace TournamentAssistantShared.Utilities
{
    public static class Localizer
    {
        static Dictionary<User.PlayStates, string> playStateDictEnglish = new Dictionary<User.PlayStates, string> {
            {User.PlayStates.Waiting, "Waiting"},
            {User.PlayStates.InGame, "In Game"}
        };

        static Dictionary<User.PlayStates, string> playStateDictChineseSimplified = new Dictionary<User.PlayStates, string> {
            {User.PlayStates.Waiting, "等待中"},
            {User.PlayStates.InGame, "比赛中"}
        };

        static Dictionary<User.DownloadStates, string> downloadStateDictEnglish = new Dictionary<User.DownloadStates, string> {
            {User.DownloadStates.None, "None"},
            {User.DownloadStates.Downloading, "Downloading"},
            {User.DownloadStates.Downloaded, "Downloaded"},
            {User.DownloadStates.DownloadError, "Download Error"}
        };

        static Dictionary<User.DownloadStates, string> downloadStateDictChineseSimplified = new Dictionary<User.DownloadStates, string> {
            {User.DownloadStates.None, "未知"},
            {User.DownloadStates.Downloading, "下载中"},
            {User.DownloadStates.Downloaded, "已下载"},
            {User.DownloadStates.DownloadError, "下载出错"}
        };

        static Dictionary<Language, Dictionary<User.PlayStates, string>> playStateDict = new Dictionary<Language, Dictionary<User.PlayStates, string>> {
            { Language.English, playStateDictEnglish},
            { Language.Chinese_Simplified, playStateDictChineseSimplified }
        };

        static Dictionary<Language, Dictionary<User.DownloadStates, string>> downloadStateDict = new Dictionary<Language, Dictionary<User.DownloadStates, string>> {
            { Language.English, downloadStateDictEnglish},
            { Language.Chinese_Simplified, downloadStateDictChineseSimplified }
        };

        static Dictionary<Language, string> teamNameDict = new Dictionary<Language, string> {
            { Language.English, "None"},
            { Language.Chinese_Simplified, "无" }
        };

        static Dictionary<Models.Packets.SongFinished.CompletionType, string> songFinishedTypeDictEnglish = new Dictionary<Models.Packets.SongFinished.CompletionType, string> {
            {Models.Packets.SongFinished.CompletionType.Passed, "Passed"},
            {Models.Packets.SongFinished.CompletionType.Failed, "Failed"},
            {Models.Packets.SongFinished.CompletionType.Quit, "Quit"}
        };

        static Dictionary<Models.Packets.SongFinished.CompletionType, string> songFinishedTypeDictChineseSimplified = new Dictionary<Models.Packets.SongFinished.CompletionType, string> {
            {Models.Packets.SongFinished.CompletionType.Passed, "通关"},
            {Models.Packets.SongFinished.CompletionType.Failed, "失败"},
            {Models.Packets.SongFinished.CompletionType.Quit, "退出"}
        };

        static Dictionary<Language, Dictionary<Models.Packets.SongFinished.CompletionType, string>> songFinishedTypeDict = new Dictionary<Language, Dictionary<Models.Packets.SongFinished.CompletionType, string>> {
            { Language.English, songFinishedTypeDictEnglish},
            { Language.Chinese_Simplified, songFinishedTypeDictChineseSimplified }
        };

        public enum Language {
            English = 0,
            Chinese_Simplified = 1
        }

        public static string TranslatePlayState(User.PlayStates state, Language lang)
        {
            Dictionary<User.PlayStates, string> trans;
            string trans_state;
            if (playStateDict.TryGetValue(lang, out trans) && trans.TryGetValue(state, out trans_state))
            {
                return trans_state;
            }
            return state.ToString();
        }

        public static string TranslateDownloadState(User.DownloadStates state, Language lang)
        {
            Dictionary<User.DownloadStates, string> trans;
            string trans_state;
            if (downloadStateDict.TryGetValue(lang, out trans) && trans.TryGetValue(state, out trans_state))
            {
                return trans_state;
            }
            return state.ToString();
        }

        public static string TranslateTeamName(string name, Language lang)
        {
            if (name.Equals("None")) {
                string trans_name;
                if (teamNameDict.TryGetValue(lang, out trans_name))
                {
                    return trans_name;
                }
            }
            
            return name;
        }

        public static string TranslateFinishedType(Models.Packets.SongFinished.CompletionType type, Language lang)
        {
            Dictionary<Models.Packets.SongFinished.CompletionType, string> trans;
            string trans_state;
            if (songFinishedTypeDict.TryGetValue(lang, out trans) && trans.TryGetValue(type, out trans_state))
            {
                return trans_state;
            }
            return type.ToString();
        }
    }

    public class LocalizedUser {
        public User user { get; set; }
        public string translatePlayState { get; set; }
        public string translateDownloadState { get; set; }
        public string translateTeamName { get; set; }

        public void import(User user) {
            this.user = user?? new User();
            this.translateTeamName = Localizer.TranslateTeamName(user.Team.Name, Localizer.Language.Chinese_Simplified);
            this.translatePlayState = Localizer.TranslatePlayState(user.PlayState, Localizer.Language.Chinese_Simplified);
            this.translateDownloadState = Localizer.TranslateDownloadState(user.DownloadState, Localizer.Language.Chinese_Simplified);
            //return this;
        }
    }

    public class LocalizedResult {
        public Models.Packets.SongFinished result { get; set; }
        public string translateFinishedType { get; set; }

        public void import(Models.Packets.SongFinished songFinished)
        {
            this.result = songFinished ?? new Models.Packets.SongFinished();
            this.translateFinishedType = Localizer.TranslateFinishedType(result.Type, Localizer.Language.Chinese_Simplified);
        }
    }
}
