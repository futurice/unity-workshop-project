using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public enum GameState
	{
		WAITING_FOR_PLAYERS,
		RUNNING,
		NONE
	}

	public enum GameEvent
	{
		START_GAME,
		STOP_GAME,
		PLAYER_LEFT_SCORED,
		PLAYER_RIGHT_SCORED,
		NONE
	}

	[Header("GUI options")]
	[SerializeField]
	private Text					_scoreText				= null;

	[Header("Ball options")]
	[SerializeField]
	private Transform				_ballContainer			= null;
	[SerializeField]
	private GameObject				_ballPrefab				= null;
    [SerializeField]
    private Transform               _ballSpawnPoint         = null;
	[SerializeField]
	private float					_initialBallSpeed		= 15.0f;

	[Header("Game play options")]
	[SerializeField]
	private float					_scoreCooldown			= 2.0f;

	private GameState				_currentState			= GameState.NONE;
	private BallController			_currentBall			= null;

	private int						_leftPlayerScore		= 0;
	private int						_rightPlayerScore		= 0;

	public BallController CurrentBall
	{
		get
		{
			return _currentBall;
		}
	}

	#region Singleton implementation

	protected static GameManager 	_instance				= null;
	protected static object 		_lock 					= new object ();
	protected static bool 			_applicationIsQuitting 	= false;

	public static GameManager Instance
	{
		get
		{
			if (_applicationIsQuitting)
			{
				Debug.LogWarningFormat ("[GameManager] Instance {0} already destroyed on application quit. Won't create again - returning null.", typeof (GameManager));
				return null;
			}

			lock (_lock)
			{
				if (_instance == null)
				{
					_instance = (GameManager) FindObjectOfType (typeof(GameManager));

					if (FindObjectsOfType (typeof(GameManager)).Length > 1)
					{
						Debug.LogErrorFormat ("[GameManager] Multiple instances of {0} detected. Something is really wrong.", typeof (GameManager));
						return _instance;
					}

					if (_instance == null)
					{
						GameObject singleton = new GameObject ();
						_instance = singleton.AddComponent<GameManager> ();
						singleton.name = "(singleton) "+ typeof(GameManager).ToString();

						DontDestroyOnLoad (singleton);

						Debug.LogFormat ("[GameManager] An instance of {0} is needed in the scene, so {1} was created with DontDestroyOnLoad.", typeof (GameManager), singleton);
					}
				}

				return _instance;
			}
		}
	}

	#endregion

	#region Game event handling

	public void HandleGameEvent (GameEvent e)
	{
		if (e == GameEvent.START_GAME)
		{
			HandleStartGameEvent ();
		}
		else if (e == GameEvent.STOP_GAME)
		{
			HandleStopGameEvent ();
		}
		else if (e == GameEvent.PLAYER_LEFT_SCORED || e == GameEvent.PLAYER_RIGHT_SCORED)
		{
			HandlePlayerScoredEvent (e);
		}
	}

	private void HandleStartGameEvent ()
	{
		// Change the game state and spawn a new ball
		_currentState = GameState.RUNNING;
		StartCoroutine (RespawnBall ());
	}

	private void HandleStopGameEvent ()
	{
		// Change the game state and destroy the ball
		_currentState = GameState.WAITING_FOR_PLAYERS;
		DestroyBall ();
	}

	private void HandlePlayerScoredEvent (GameEvent e)
	{
		// Check which players' score should be increased and increase it
		if (e == GameEvent.PLAYER_LEFT_SCORED)
		{
			_leftPlayerScore++;
		}
		else
		{
			_rightPlayerScore++;
		}

		// Update the score text
		_scoreText.text = string.Format ("{0}  -  {1}", _leftPlayerScore, _rightPlayerScore);

		// Destroy the current ball
		DestroyBall ();

		// Start a timer to respawn a new ball
		StartCoroutine (RespawnBall ());
	}

	#endregion

	private void Start ()
	{
		// Start the game
		HandleGameEvent (GameEvent.START_GAME);
	}

	private void DestroyBall ()
	{
		// Destroy the current ball
		if (_currentBall != null)
		{
			Destroy (_currentBall.gameObject);
		}

		_currentBall = null;
	}

	private void CreateBall ()
	{
		// Create a new ball
		GameObject newBall = Instantiate (_ballPrefab, _ballSpawnPoint.position, Quaternion.identity, _ballContainer) as GameObject;

		// Store the reference to the ball and initialize the ball
		_currentBall = newBall.GetComponent<BallController> ();
		_currentBall.Init (_initialBallSpeed);
	}

	private IEnumerator RespawnBall ()
	{
		// Wait for the cooldown to finish
		yield return new WaitForSeconds (_scoreCooldown);

		CreateBall ();
	}
}
