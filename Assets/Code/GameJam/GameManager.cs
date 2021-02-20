using UnityEngine;

namespace GameJam
{
	public class GameManager : MonoBehaviour
    {
        private GameStateMachine _machine;

        public Game Game { get; private set; }

        private void Awake()
        {
	        var _musicAudioSource = GameObject.Find("Music Audio Source").GetComponent<AudioSource>();

	        Game = new Game();
            Game.Config = Resources.Load<GameConfig>("Game Config");
            Game.Controls = new GameControls();
            Game.Camera = Camera.main;
            Game.UI = FindObjectOfType<GameUI>();
            Game.State = new GameState();
            Game.ProjectileSpawner = new ProjectileSpawner();
            Game.AudioPlayer = new AudioPlayer(Game.Config, _musicAudioSource);

	        _machine = new GameStateMachine(false, Game);
        }

        private void Start()
        {
	        _machine.Start();
        }

        private void Update()
        {
			_machine?.Tick();
        }
    }
}
