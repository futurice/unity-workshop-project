using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
	private enum InputType
	{
		DESKTOP,
		VIVE,
		HOLOLENS
	}

	[Header("General options")]
	[SerializeField]
	private InputType					_inputType				= InputType.DESKTOP;
	[SerializeField]
	private float						_movementSpeed			= 30.0f;

	private Vector3						_playerVelocity			= Vector3.zero;
	private Rigidbody					_playerRigidbody		= null;
	private Material					_playerMaterial			= null;

	[Header("Vive options")]
	[SerializeField]
	private SteamVR_TrackedObject 		_leftController			= null;
	[SerializeField]
	private SteamVR_TrackedObject		_rightController		= null;

	private SteamVR_Controller.Device 	_leftControllerDevice	= null;
	private SteamVR_Controller.Device 	_rightControllerDevice	= null;

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

	private SteamVR_Controller.Device LeftControllerDevice
	{
		get
		{
			if (_leftControllerDevice == null)
			{
				if (_leftController != null && _leftController.index != SteamVR_TrackedObject.EIndex.None)
				{
					_leftControllerDevice = SteamVR_Controller.Input ((int)_leftController.index);
				}
			}

			return _leftControllerDevice;
		}
	}

	private SteamVR_Controller.Device RightControllerDevice
	{
		get
		{
			if (_rightControllerDevice == null)
			{
				if (_rightController != null && _rightController.index != SteamVR_TrackedObject.EIndex.None)
				{
					_rightControllerDevice = SteamVR_Controller.Input ((int)_rightController.index);
				}
			}

			return _rightControllerDevice;
		}
	}

	private void FixedUpdate ()
	{
		if (_inputType == InputType.DESKTOP)
		{
			// Get the movement on the vertical axis and convert that to
			// movement speed of the Rigidbody
			float v = Input.GetAxisRaw ("Vertical");
			MovePlayer (v);
		}
		else if (_inputType == InputType.VIVE)
		{
			HandleViveControllerDeviceInput (LeftControllerDevice);
			HandleViveControllerDeviceInput (RightControllerDevice);
		}
		else if (_inputType == InputType.HOLOLENS)
		{
			// TODO
		}
	}

	private void MovePlayer (float v)
	{
		_playerVelocity.z = v * _movementSpeed;
		PlayerRigidbody.velocity = _playerVelocity;
	}

	private void HandleViveControllerDeviceInput (SteamVR_Controller.Device device)
	{
		if (device == null)
		{
			return;
		}

		// Detect whether the finger is on touchpad
		if (device.GetTouch (SteamVR_Controller.ButtonMask.Touchpad))
		{
			// Read the touchpad values
			Vector2 touchpad = device.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad);

			// Convert the touchpad y value to movement
			MovePlayer (touchpad.y);
		}
	}
}
