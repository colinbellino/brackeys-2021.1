using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using static GameJam.Utils;

namespace GameJam
{
	public class GameUI : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField] private GameObject _gameplayRoot;
		[SerializeField] private Text _debugText;
		[SerializeField] private SpriteRenderer _selectionRectangle;
		[SerializeField] private Transform _cursor;
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

		public void SetSelectedUnits(List<EntityComponent> units)
		{
			_debugText.text = "Selected characters:\n";
			foreach (var unit in units)
			{
				_debugText.text += $"- {unit.name}\n";
			}
		}

		public void SetSelectionRectangle(Vector3 start, Vector3 end)
		{
			var (origin, size) = GetSelectionBox(start, end);
			_selectionRectangle.size = size;
			_selectionRectangle.transform.position = origin;
		}

		public void ClearSelectionRectangle()
		{
			_selectionRectangle.size = Vector2.zero;
		}

		public void MoveCursor(Vector3 position)
		{
			_cursor.position = position;
		}
	}
}
