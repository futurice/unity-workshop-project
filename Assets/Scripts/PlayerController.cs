using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
	[SerializeField]
	private string 		_inputAxis			= "Vertical";
	[SerializeField]
	private float		_movementSpeed		= 30.0f;
	[SerializeField]
	private Color		_localPlayerColor	= Color.blue;

	private Vector3		_playerVelocity		= Vector3.zero;
	private Rigidbody	_playerRigidbody	= null;
	private Material	_playerMaterial		= null;

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

	private Material PlayerMaterial
	{
		get
		{
			if (_playerMaterial == null)
			{
				_playerMaterial = GetComponent<MeshRenderer> ().material;
			}

			return _playerMaterial;
		}
	}

	private void FixedUpdate ()
	{
		// Only the local player should process input
		if (!isLocalPlayer)
		{
			return;
		}

		// Get the movement on the vertical axis and convert that to
		// movement speed of the Rigidbody
		float v = Input.GetAxisRaw (_inputAxis);
		_playerVelocity.z = v * _movementSpeed;
		PlayerRigidbody.velocity = _playerVelocity;
	}

	public override void OnStartLocalPlayer ()
	{
		PlayerMaterial.color = _localPlayerColor;
	}
}
