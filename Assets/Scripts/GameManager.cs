using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
	public enum GameEvent
	{
		PLAYER_LEFT_SCORED,
		PLAYER_RIGHT_SCORED,
		NONE
	}

	[Header("Ball options")]
	[SerializeField]
	private Transform				_ballContainer		= null;
	[SerializeField]
	private GameObject				_ballPrefab			= null;
	[SerializeField]
	private float					_initialBallSpeed	= 15.0f;

	[Header("Player options")]
	[SerializeField]
	private Transform				_playerContainer	= null;

	[Header("Game play options")]
	[SerializeField]
	private float					_scoreCooldown		= 2.0f;

	private BallController			_currentBall		= null;

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

	private void Start ()
	{
		StartCoroutine (RespawnBall ());
	}

	public void HandleGameEvent (GameEvent e)
	{
		if (e == GameEvent.PLAYER_LEFT_SCORED ||
			e == GameEvent.PLAYER_RIGHT_SCORED)
		{
			HandlePlayerScoredEvent (e);
		}
	}

	private void DestroyBall ()
	{
		// Destroy the current ball
		if (_currentBall != null)
		{
			NetworkServer.Destroy (_currentBall.gameObject);
		}
	}

	private void HandlePlayerScoredEvent (GameEvent e)
	{
		// Destroy the current ball
		DestroyBall ();

		// Start a timer to respawn a new ball
		StartCoroutine (RespawnBall ());
	}

	private IEnumerator RespawnBall ()
	{
		if (!isServer)
		{
			yield break;
		}

		// Wait for the cooldown to finish
		yield return new WaitForSeconds (_scoreCooldown);

		GameObject newBall = Instantiate (_ballPrefab, Vector3.zero, Quaternion.identity, _ballContainer) as GameObject;
		NetworkServer.Spawn (newBall);

		_currentBall = newBall.GetComponent<BallController> ();
		_currentBall.Init (_initialBallSpeed);
	}
}
