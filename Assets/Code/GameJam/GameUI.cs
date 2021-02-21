﻿using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
	public class GameUI : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField] private GameObject _gameplayRoot;
		[SerializeField] private SVGImage[] _countdownImages;
		[SerializeField] private Text _debugText;
		[Header("Victory")]
		[SerializeField] private Image _victoryRoot;
		[SerializeField] private TMP_Text _victoryText;
		[SerializeField] private TMP_Text _titleText;
		[SerializeField] public Button VictoryNextButton;
		[SerializeField] private Image _commentRoot;
		[SerializeField] public Button CommentNextButton;
		[SerializeField] private Image _retryRoot;
		[SerializeField] public Button RetryYesButton;
		[SerializeField] public Button RetryNoButton;
		[Header("Defeat")]
		[SerializeField] private Image _giveUpRoot;
		[SerializeField] private TMP_Text _giveUpText;
		[SerializeField] public Button GiveUpYesButton;
		[SerializeField] public Button GiveUpNoButton;
		[SerializeField] private Image _receiveHelpRoot;
		[SerializeField] private TMP_Text _receiveHelpText;
		[SerializeField] public Button ReceiveHelpYesButton;
		[SerializeField] public Button ReceiveHelpNoButton;
		[Header("Transitions")]
		[SerializeField] private Image _fadeToBlackImage;

		private void Awake()
		{
			HideGameplay();
			HideVictory(0f);
			HideGiveUp(0f);
			HideReceiveHelp(0f);
			HideRetry(0f);
			HideComment(0f);
			HideTitle(0f);
		}

		public void ShowGameplay() { _gameplayRoot.SetActive(true); }
		public void HideGameplay() { _gameplayRoot.SetActive(false); }

		public void SetCounter(int deathCounter)
		{
			_countdownImages[0].gameObject.SetActive(deathCounter < 1);
			_countdownImages[1].gameObject.SetActive(deathCounter < 2);
			_countdownImages[2].gameObject.SetActive(deathCounter < 3);
		}

		public void SetDebugText(string value)
		{
			_debugText.text = value;
		}

		public async UniTask ShowTitle()
		{
			await _titleText.DOFade(1f, 0.5f);
		}
		public async UniTask HideTitle(float duration = 0.5f)
		{
			await _titleText.DOFade(0f, duration);
		}

		public async UniTask ShowVictory(bool helpReceived)
		{
			if (helpReceived)
			{
				_victoryText.text = $"With the help of other players, you managed to beat the game. Well done!";
			}
			else
			{
				_victoryText.text = $"You managed to beat the game all by yourself, impressive! But remember there is no shame in accepting the help of others from time to time.";
			}

			await FadeInPanel(_victoryRoot, 0.5f);
		}
		public async UniTask HideVictory(float duration = 0.3f)
		{
			await FadeOutPanel(_victoryRoot, duration);
		}

		public async UniTask ShowComment()
		{
			await FadeInPanel(_commentRoot, 0.5f);
		}
		public async UniTask HideComment(float duration = 0.3f)
		{
			await FadeOutPanel(_commentRoot, duration);
		}

		public async UniTask ShowRetry()
		{
			await FadeInPanel(_retryRoot, 0.5f);
		}
		public async UniTask HideRetry(float duration = 0.3f)
		{
			await FadeOutPanel(_retryRoot, duration);
		}

		public async UniTask ShowGiveUp(int deathCounter)
		{
			_giveUpText.text = deathCounter switch
			{
				0 => $"So... You died. \n\nDo you want to give up here?",
				1 => $"Again? \n\nWhy don't you just give up already?!",
				_ => $"You can't do this alone... \n\nGive up?"
			};
			await FadeInPanel(_giveUpRoot, 0.5f);
		}
		public async UniTask HideGiveUp(float duration = 0.3f)
		{
			await FadeOutPanel(_giveUpRoot, duration);
		}

		public async UniTask ShowReceiveHelp(string helperName)
		{
			_receiveHelpText.text = $"Help offer received from \"{helperName}\".\n\nAccept the offer?";
			await FadeInPanel(_receiveHelpRoot, 0.5f);
		}
		public async UniTask HideReceiveHelp(float duration = 0.3f)
		{
			await FadeOutPanel(_receiveHelpRoot, duration);
		}

		public async UniTask FadeIn(Color color, float duration = 1f)
		{
			await _fadeToBlackImage.DOColor(color, duration);
		}
		public async UniTask FadeOut(float duration = 1f)
		{
			await _fadeToBlackImage.DOColor(Color.clear, duration);
		}

		private async UniTask FadeInPanel(Image panel, float duration)
		{
			panel.gameObject.SetActive(true);
			panel.DOFade(1f, duration);

			var graphics = panel.GetComponentsInChildren<Graphic>();
			foreach (var graphic in graphics)
			{
				graphic.DOFade(1f, duration);
			}

			await UniTask.Delay(TimeSpan.FromSeconds(duration));
		}

		private async UniTask FadeOutPanel(Image panel, float duration)
		{
			panel.DOFade(0f, duration);

			var graphics = panel.GetComponentsInChildren<Graphic>();
			foreach (var graphic in graphics)
			{
				graphic.DOFade(0f, duration);
			}

			await UniTask.Delay(TimeSpan.FromSeconds(duration));
			panel.gameObject.SetActive(false);
		}
	}
}
