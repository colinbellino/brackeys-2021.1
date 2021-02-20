using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
		[SerializeField] public Button RetryYesButton;
		[SerializeField] public Button RetryNoButton;
		[Header("Defeat")]
		[SerializeField] private GameObject _giveUpRoot;
		[SerializeField] private Text _giveUpText;
		[SerializeField] public Button GiveUpYesButton;
		[SerializeField] public Button GiveUpNoButton;
		[SerializeField] private GameObject _receiveHelpRoot;
		[SerializeField] private Text _receiveHelpText;
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
		}

		public void ShowGameplay() { _gameplayRoot.SetActive(true); }
		public void HideGameplay() { _gameplayRoot.SetActive(false); }

		public void ShowVictory() { _victoryRoot.SetActive(true); }
		public void HideVictory() { _victoryRoot.SetActive(false); }

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
				0 => $"Give up here?",
				1 => $"Just give up already?!",
				_ => $"You can't do it... \nGive up?"
			};
			_giveUpRoot.SetActive(true);
		}
		public void HideGiveUp() { _giveUpRoot.SetActive(false); }
		public void ShowReceiveHelp(string helperName)
		{
			_receiveHelpText.text = $"Help offer received from \"{helperName}\".\nAccept the offer?";
			_receiveHelpRoot.SetActive(true);

		}
		public void HideReceiveHelp() { _receiveHelpRoot.SetActive(false); }

		public async UniTask StartFadeToBlack(float duration = 1f) { await _fadeToBlackImage.DOColor(Color.black, duration); }
		public async UniTask EndFadeToBlack(float duration = 1f) { await _fadeToBlackImage.DOColor(Color.clear, duration); }

		public void SetDebugText(string value)
		{
			_debugText.text = value;
		}
	}
}
