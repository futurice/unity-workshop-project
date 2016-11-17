#define MODE_DESKTOP

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

#if MODE_VIVE
using Valve.VR;
#endif

#if MODE_HOLOLENS
using UnityEngine.VR.WSA.Input;
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
        // Handle input from both of the vive controllers - select the maximum between the controllers
        float leftControllerMovement = GetViveControllerDeviceInput (LeftControllerDevice);
        float rightControllerMovement = GetViveControllerDeviceInput (RightControllerDevice);
		MovePlayer (Mathf.Max (leftControllerMovement, rightControllerMovement));	
#elif MODE_HOLOLENS
        // Handle the gaze based input from the Hololens
        HandleHololensControls ();
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

    private float GetViveControllerDeviceInput (SteamVR_Controller.Device device)
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
            return touchpad.y;
        }

		// If the finger isn't on the touch pad, stop the movement
		return 0.0f
    }

#endif
#endregion

#region Hololens
#if MODE_HOLOLENS

    [SerializeField]
    private Transform _cursorTransform;
    [SerializeField]
    private MeshRenderer _cursorRender;

    private GameObject _focusedObject;
    private GameObject _oldFocus;

    private Vector3 _headPos;
    private Vector3 _viewDir;

    private GestureRecognizer _recognizer;

    void Awake()
    {
        _recognizer = new GestureRecognizer();
        _recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            if (_focusedObject != null)
            {
                _focusedObject.SendMessageUpwards("OnSelect");
            }
        };

        _recognizer.StartCapturingGestures();
    }

    void HandleHololensControls()
    {
        GameObject oldFocus = _focusedObject;

        // First update the head position and view direction
        _headPos = Camera.main.transform.position;
        _viewDir = Camera.main.transform.forward;

        // Create a variable to store a potential hit from the raytracer
        RaycastHit hitInfo;

        // Fire a ray from the position of the head in the direction we are viewing, and see if it hits anything
        bool hit = Physics.Raycast(_headPos, _viewDir, out hitInfo);

        // If we hit an object, we want to turn on the rendering of the cursor at the impact point
        if (hit)
        {
            _cursorRender.enabled = true;
            // Set the cursor at the hit position
            _cursorTransform.position = hitInfo.point;
            _cursorTransform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

            _focusedObject = hitInfo.collider.gameObject;
        }
        // Not hit so we want to turn off the rendering of the cursor
        else
        {
            _cursorRender.enabled = false;
            _focusedObject = null;
        }

        if (_focusedObject != _oldFocus)
        {
            _recognizer.CancelGestures();
            _recognizer.StartCapturingGestures();
        }
    }

#endif
#endregion
}
