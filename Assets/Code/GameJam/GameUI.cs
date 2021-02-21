using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace GameJam
{
	public class GameUI : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField] private GameObject _gameplayRoot;
		[SerializeField] private SVGImage[] _countdownImages;
		[SerializeField] private Text _debugText;
		[Header("Victory")]
		[SerializeField] private GameObject _victoryRoot;
		[SerializeField] private TMP_Text _victoryText;
		[SerializeField] private TMP_Text _titleText;
		[SerializeField] public Button VictoryNextButton;
		[SerializeField] private GameObject _commentRoot;
		[SerializeField] public Button CommentNextButton;
		[SerializeField] private GameObject _retryRoot;
		[SerializeField] public Button RetryYesButton;
		[SerializeField] public Button RetryNoButton;
		[Header("Defeat")]
		[SerializeField] private GameObject _giveUpRoot;
		[SerializeField] private TMP_Text _giveUpText;
		[SerializeField] public Button GiveUpYesButton;
		[SerializeField] public Button GiveUpNoButton;
		[SerializeField] private GameObject _receiveHelpRoot;
		[SerializeField] private TMP_Text _receiveHelpText;
		[SerializeField] public Button ReceiveHelpYesButton;
		[SerializeField] public Button ReceiveHelpNoButton;
		[Header("Transitions")]
		[SerializeField] private Image _fadeToBlackImage;

		private void Awake()
		{
			HideGameplay();
			HideVictory();
			HideGiveUp();
			HideReceiveHelp();
			HideRetry();
			HideComment();
			HideTitle(0f);
		}

		public void ShowGameplay() { _gameplayRoot.SetActive(true); }
		public void HideGameplay() { _gameplayRoot.SetActive(false); }

		public async UniTask ShowTitle() => await _titleText.DOFade(1f, 0.5f);
		public async UniTask HideTitle(float duration = 0.5f) => await _titleText.DOFade(0f, duration);

		public void ShowVictory(bool helpReceived)
		{
			if (helpReceived)
			{
				_victoryText.text = $"With the help of other players, you managed to beat the game. Well done!";
			}
			else
			{
				_victoryText.text = $"You managed to beat the game all by yourself, impressive! But remember there is no shame in accepting the help of others from time to time.";
			}
			_victoryRoot.SetActive(true);
		}
		public void HideVictory() { _victoryRoot.SetActive(false); }

		public void ShowComment()
		{
			_commentRoot.SetActive(true);
		}
		public void HideComment()
		{
			_commentRoot.SetActive(false);
		}

		public void ShowRetry()
		{
			_retryRoot.SetActive(true);
		}
		public void HideRetry()
		{
			_retryRoot.SetActive(false);
		}

		public void SetCounter(int deathCounter)
		{
			_countdownImages[0].gameObject.SetActive(deathCounter < 1);
			_countdownImages[1].gameObject.SetActive(deathCounter < 2);
			_countdownImages[2].gameObject.SetActive(deathCounter < 3);
		}

		public void ShowGiveUp(int deathCounter)
		{
			_giveUpText.text = deathCounter switch
			{
				0 => $"So... You died. \n\nDo you want to give up here?",
				1 => $"Again? \n\nWhy don't you just give up already?!",
				_ => $"You can't do this alone... \n\nGive up?"
			};
			_giveUpRoot.SetActive(true);
		}
		public void HideGiveUp() { _giveUpRoot.SetActive(false); }

		public void ShowReceiveHelp(string helperName)
		{
			_receiveHelpText.text = $"Help offer received from \"{helperName}\".\n\nAccept the offer?";
			_receiveHelpRoot.SetActive(true);

		}
		public void HideReceiveHelp() { _receiveHelpRoot.SetActive(false); }

		public async UniTask FadeIn(Color color, float duration = 1f) { await _fadeToBlackImage.DOColor(color, duration); }
		public async UniTask FadeOut(float duration = 1f) { await _fadeToBlackImage.DOColor(Color.clear, duration); }

		public void SetDebugText(string value)
		{
			_debugText.text = value;
		}
	}
}
