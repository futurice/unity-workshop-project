#define MODE_DESKTOP

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

#if MODE_VIVE
using Valve.VR;
#endif

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
	private float						_movementSpeed			= 30.0f;

	private Vector3						_playerVelocity			= Vector3.zero;
	private Rigidbody					_playerRigidbody		= null;
	private Material					_playerMaterial			= null;

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
		// Handle different types of user input
		#if MODE_DESKTOP
			// Read the vertical axis - typically up/down arrows or w/s letters
			float v = Input.GetAxisRaw ("Vertical");
			MovePlayer (v);
		#elif MODE_VIVE
            // Handle input from both of the vive controllers - right controller overrides the left
            HandleViveControllerDeviceInput (LeftControllerDevice);
            HandleViveControllerDeviceInput (RightControllerDevice);
		#elif MODE_HOLOLENS
			// TODO
		#endif
	}

	private void MovePlayer (float v)
	{
		// Convert the press value to velocity
		_playerVelocity.z = v * _movementSpeed;
		PlayerRigidbody.velocity = _playerVelocity;
	}
	
	#region Vive
	#if MODE_VIVE

	[Header("Vive options")]
	[SerializeField]
	private SteamVR_TrackedObject		_leftController 		= null;
	[SerializeField]
	private SteamVR_TrackedObject 		_rightController 		= null;

	private SteamVR_Controller.Device 	_leftControllerDevice 	= null;
	private SteamVR_Controller.Device 	_rightControllerDevice 	= null;

	private SteamVR_Controller.Device LeftControllerDevice
	{
		get
		{
			if (_leftControllerDevice == null)
			{
				if (_leftController != null && _leftController.index != SteamVR_TrackedObject.EIndex.None)
				{
					_leftControllerDevice = SteamVR_Controller.Input((int)_leftController.index);
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
					_rightControllerDevice = SteamVR_Controller.Input((int)_rightController.index);
				}
			}

			return _rightControllerDevice;
		}
	}

    private void HandleViveControllerDeviceInput(SteamVR_Controller.Device device)
    {
        // If the device is null we can't do anything - return
        if (device == null)
        {
            return;
        }

        // Detect whether the finger is on touchpad
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            // Read the touchpad values
           	Vector2 touchpad = device.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad);

            // Convert the touchpad y value to movement
            MovePlayer (touchpad.y);
        }
    }

	#endif
	#endregion
}
