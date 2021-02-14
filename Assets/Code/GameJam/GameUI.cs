using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace GameJam
{
	public class GameUI : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField] private GameObject _gameplayRoot;
		[Header("Victory")]
		[SerializeField] private GameObject _victoryRoot;
		[SerializeField] private Button _victoryRetryButton;
		[Header("Defeat")]
		[SerializeField] private GameObject _defeatRoot;
		[SerializeField] private Button _defeatRetryButton;

		public event Action RetryClicked;

		private void Awake()
		{
			HideGameplay();
			HideVictory();
			HideDefeat();

			_victoryRetryButton.onClick.AddListener(OnRetryClicked);
			_defeatRetryButton.onClick.AddListener(OnRetryClicked);
		}

		private void OnRetryClicked()
		{
			RetryClicked?.Invoke();
		}

		public void ShowGameplay() { _gameplayRoot.SetActive(true); }
		public void HideGameplay() { _gameplayRoot.SetActive(false); }
		public void ShowVictory() { _victoryRoot.SetActive(true); }
		public void HideVictory() { _victoryRoot.SetActive(false); }
		public void ShowDefeat() { _defeatRoot.SetActive(true); }
		public void HideDefeat() { _defeatRoot.SetActive(false); }
	}
}
