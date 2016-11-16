using UnityEngine;
using System.Collections;

public class AIPlayer : MonoBehaviour
{
	[SerializeField]
	private float 			_movementSpeed 		= 1.5f;
	[SerializeField]
	private float 			_lerpSpeed 			= 1f;

	private Rigidbody 		_playerRigidbody	= null;
	private Transform 		_ballTransform		= null;

	private Vector3			_up					= new Vector3 (0.0f, 0.0f, 1.0f);
	private Vector3			_down				= new Vector3 (0.0f, 0.0f, -1.0f);

	private Rigidbody PlayerRigidbody
	{
		get
		{
			if (_playerRigidbody == null)
			{
				_playerRigidbody = GetComponent<Rigidbody> ();
			}

			return _playerRigidbody;
		}
	}

	private Transform BallTransform
	{
		get
		{
			if (_ballTransform == null)
			{
				if (GameManager.Instance.CurrentBall != null)
				{
					_ballTransform = GameManager.Instance.CurrentBall.transform;
				}
			}

			return _ballTransform;
		}
	}

	private void FixedUpdate ()
	{
		// If there is no ball in the game - return
		if (BallTransform == null)
		{
			return;
		}

		// If the ball position is higher than our own position
		if (BallTransform.position.z > transform.position.z)
		{
			// If we are moving down - stop
			if (PlayerRigidbody.velocity.z < 0)
			{
				PlayerRigidbody.velocity = Vector3.zero;
			}

			// Move up towards the ball
			PlayerRigidbody.velocity = Vector3.Lerp (PlayerRigidbody.velocity, _up * _movementSpeed, _lerpSpeed * Time.fixedDeltaTime);
		}
		// If the ball position is lower than our own position
		else if (BallTransform.position.z < transform.position.z)
		{
			// If we are moving up - stop
			if (PlayerRigidbody.velocity.z > 0)
			{
				PlayerRigidbody.velocity = Vector3.zero;
			}

			// Move down towards the ball
			PlayerRigidbody.velocity = Vector3.Lerp (PlayerRigidbody.velocity, _down * _movementSpeed, _lerpSpeed * Time.deltaTime);
		}
		// If the ball is aligned with us - slow down
		else
		{
			PlayerRigidbody.velocity = Vector3.Lerp (PlayerRigidbody.velocity, Vector3.zero * _movementSpeed, _lerpSpeed * Time.deltaTime);
		}
	}
}
