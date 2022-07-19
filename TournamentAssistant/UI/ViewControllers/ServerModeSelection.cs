#pragma warning disable CS0649
#pragma warning disable IDE0044
#pragma warning disable IDE0051
#pragma warning disable IDE0052
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using TMPro;
using UnityEngine.UI;

namespace TournamentAssistant.UI.ViewControllers
{
    internal class ServerModeSelection : BSMLResourceViewController
    {
        // For this method of setting the ResourceName, this class must be the first class in the file.
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        public event Action TournamentButtonPressed;
        public event Action QualifierButtonPressed;
        public event Action BattleSaberButtonPressed;

        [UIComponent("status-text")]
        private TextMeshProUGUI statusText;

        [UIComponent("tournament-button")]
        private Button _tournamentRoomButton;

        [UIComponent("battlesaber-button")]
        private Button _battleSaberButton;

        [UIComponent("bottom-text-panel")]
        private TextMeshProUGUI _bottomTextPanel;

        [UIValue("bottom-text")]
        private string quote = QuoteRandomizer();

        //We need to keep track of the text like this because it is very possible
        //that we'll want to update it before the list is actually displayed.
        //This way, we can handle that situation and avoid null exceptions / missing data
        private string _statusText;
        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                if (statusText != null) statusText.text = _statusText;
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (addedToHierarchy)
            {
                if (_statusText != null) statusText.text = _statusText;
            }
        }

        [UIAction("tournament-button-pressed")]
        private void TournamentButtonPress()
        {
            _bottomTextPanel.text = QuoteRandomizer();
            TournamentButtonPressed?.Invoke();
        }

        [UIAction("qualifier-button-pressed")]
        private void QualifierButtonPress()
        {
            _bottomTextPanel.text = QuoteRandomizer();
            QualifierButtonPressed?.Invoke();
        }

        [UIAction("battlesaber-button-pressed")]
        private void BattleSaberButtonPress()
        {
            _bottomTextPanel.text = QuoteRandomizer();
            BattleSaberButtonPressed?.Invoke();
        }

        public void EnableButtons()
        {
            _tournamentRoomButton.interactable = true;
            _battleSaberButton.interactable = true;
        }

        public void DisableButtons()
        {
            _tournamentRoomButton.interactable = false;
            _battleSaberButton.interactable = false;
        }

        public static string QuoteRandomizer()
        {
            Random rnd = new();
            string[] quotes =
            {
                "要打败一个永不放弃的人会很难",
                "你要前往一个好地方的话，今天就启程吧。",
                "赢家并不总意味着成为第一。",
                "在成事之前，它看起来总是不可能完成。",
                "你唯一失败的时候，就是你跌倒了并不再起身。",
                "在乎的不是你是否会被击倒，而是你是否会站起来。",
                "平凡与非凡的差异仅在于那一点点额外的努力。",
                "成功是一点一点的积累,一天又一天的坚持。",
                "如果你是一名真正的战勇士，竞技不会吓退你，而会让你变得更强。",
                "成为第一比保持第一更容易。",
                "一场比赛总是有赢家和输家。虽然每个人都争着去赢，但也为所有人带来乐趣。",

                //I guess thats enough for now, need to find more later, remind me if I forget - Arimodu#6469

                "天行健，君子以自強不息，地势坤，君子以厚德载物。",
                "如果放弃太早，你永远都不知道自己会错过什么。",
                "你看看你现在的样子?还是我爱的那个你么?",
                "你的选择是做或不做，但不做就永远不会有机会。",
                "你必须成功，因为你不能失败。",
                "人生有两出悲剧：一是万念俱灰，另一是踌躇满志。",
                "心灵纯洁的人，生活充满甜蜜和喜悦。",
                "遇到困难时不要抱怨，既然改变不了过去，那么就努力改变未来。",
                "只要功夫深，铁杵磨成针。",
                "用理想去成就人生，不要蹉跎了岁月。",
                "永不言败是追究者的最佳品格。",
                "目标的实现建立在我要成功的强烈愿望上。",
                "保持激情;只有激情，你才有动力，才能感染自己和其他人。",
                "别人能做到的事，自己也可以做到。",
                "努力了不一定能够成功，但是放弃了肯定是失败。",
                "人活着就要快乐。",
                "有努力就会成功!",
                "人不一定要生得漂亮，但却一定要活得漂亮。",
                "世事总是难以意料，一个人的命运往往在一瞬间会发生转变。",
                "活在当下，别在怀念过去或者憧憬未来中浪费掉你现在的生活。",
                "一份耕耘，一份收获，努力越大，收获越多。",
                "一切事无法追求完美，唯有追求尽力而为。这样心无压力，出来的结果反而会更好。",
                "进则安居以行其志，退则安居以修其所未能，则进亦有为，退亦有为也。",
                "有智者立长志，无志者长立志。",
                "坚强并不只是在大是大非中不屈服，而也是在挫折前不改变自己。",
                "希望是厄运的忠实的姐妹。",
                "梦想不抛弃苦心追求的人，只要不停止追求，你们会沐浴在梦想的光辉之中。",
                "不管现在有多么艰辛，我们也要做个生活的舞者。",
                "要成功，先发疯，头脑简单向前冲。",
                "无论什么时候，做什么事情，要思考。",
                "让我们将事前的忧虑，换为事前的思考和计划吧!",
                "永远对生活充满希望，对于困境与磨难，微笑面对。",
                "生活中的许多事，并不是我们不能做到，而是我们不相信能够做到。",
                "不要说你不会做!你是个人你就会做!",
                "学习这件事，不是缺乏时间，而是缺乏努力。",
                "胜利女神不一定眷顾所有的人，但曾经尝试过，努力过的人，他们的人生总会留下痕迹!",
                "勤奋是学习的枝叶，当然很苦，智慧是学习的花朵，当然香郁。",
                "人不能创造时机，但是它可以抓住那些已经出现的时机。",
                "没有斗狼的胆量，就不要牧羊。",
                "有时候，垃圾只是放错位置的人才。",
                "人的生命，似洪水奔流，不遇着岛屿和暗礁，难以激起美丽的浪花。",
                "与积极的人在一起，可以让我们心情高昂。",
                "向日葵看不到太阳也会开放，生活看不到希望也要坚持。",
                "才华是血汗的结晶。才华是刀刃，辛苦是磨刀石。",
                "一个人至少拥有一个梦想，有一个理由去坚强。",
                // I add some Chinese quotes - baoziii#3234

            };
            return quotes[rnd.Next(0, quotes.Length)];
        }
    }
}
