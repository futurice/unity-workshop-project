﻿using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
	[SerializeField]
	private AudioSource	_audioSource;		// Reference to the audio source to play the bounce sound

	private Vector3		_currentDir;		// Current direction of the ball
	private float 		_currentSpeed;		// Current speed of the ball
	private Rigidbody	_ballRigidbody;		// Cached reference to the rigidbody of the ball

	private Rigidbody BallRigidbody
	{
		get
		{
			if (_ballRigidbody == null)
			{
				_ballRigidbody = GetComponent<Rigidbody> ();
			}

			return _ballRigidbody;
		}
	}

	public void Init (float initialSpeed)
	{
		// Set the current speed to the initial speed
		_currentSpeed = initialSpeed;

		// Get a random starting direction in the XZ plane
		_currentDir = GetRandomUnitDirection ();

		// Set the rigidbody velocity vector to reflect the starting values
		BallRigidbody.velocity = _currentDir * _currentSpeed;
	}

	private Vector3 GetRandomUnitDirection ()
	{
		// Shoot the ball either right or left randomly
		if (Random.value < 0.5f)
		{
			return Vector3.left;
		}
		else
		{
			return Vector3.right;
		}
	}

	private float GetHitZDirection (Vector3 racketPos, float racketSizeZ)
	{
		// ||  1 <- at the top of the racket
		// ||
		// ||  0 <- at the middle of the racket
		// ||
		// || -1 <- at the bottom of the racket
		return ((transform.position.z - racketPos.z) / racketSizeZ);
	}

	private void FixedUpdate ()
	{
		// Unity physics can have some weird twists to it, make sure that the
		// ball doesn't lose energy accidentally e.g. due to friction or other physical
		// properties
		if (BallRigidbody.velocity.magnitude != _currentSpeed)
		{
			BallRigidbody.velocity = _currentDir * _currentSpeed;
		}
	}

	private void OnCollisionEnter (Collision other)
	{
		//  The collision object holds the collision information.

		// If the ball collided with a player, then:
		//   other.gameObject is the player
		//   other.transform.position is the player's position
		//   other.collider is the player's collider
		if (other.gameObject.CompareTag ("Player Left") ||
			other.gameObject.CompareTag ("Player Right"))
		{
			// Get the hit Z direction - depending on where in the player
			// the ball collided
			float z = GetHitZDirection (other.transform.position, other.collider.bounds.size.z);

			// We can determine the X direction from the current position of the ball
			// if the middle of the field is assumed to be in the origin
			float x = Mathf.Clamp (-1.0f * transform.position.x, -1.0f, 1.0f);

			// Calculate normalized direction
			_currentDir = (new Vector3 (x, 0.0f, z)).normalized;

			// Play the bounce sound
			_audioSource.Play ();
		}
		// If the ball collided with a wall, then
		else if (other.gameObject.CompareTag ("Wall"))
		{
			// Invert the z direction
			_currentDir.z *= -1.0f;

			// Play the bounce sound
			_audioSource.Play ();
		}

		// Set the rigidbody velocity vector to reflect the current 
		// velocity vector
		BallRigidbody.velocity = _currentDir * _currentSpeed;
	}
}
