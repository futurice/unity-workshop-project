#define MODE_VIVE

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
	private float						_movementSpeed			= 1.5f;			// How fast should the player move

	private Vector3						_playerVelocity			= Vector3.zero;	// Current player velocity
	private Rigidbody					_playerRigidbody		= null;			// Cached reference to the rigidbody of the player
	private Material					_playerMaterial			= null;			// Cached reference to the player material


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
	private SteamVR_TrackedObject		_leftController 		= null;		// The VR tracked object of the left Vive controller
	[SerializeField]
	private SteamVR_TrackedObject 		_rightController 		= null;		// The VR tracked object of the right Vive controller

	private SteamVR_Controller.Device 	_leftControllerDevice 	= null;		// The controller object of the left controller
	private SteamVR_Controller.Device 	_rightControllerDevice 	= null;		// The controller object of the right controller

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
            return 0.0f;
        }

        // Detect whether the finger is on touchpad
        if (device.GetTouch (SteamVR_Controller.ButtonMask.Touchpad))
        {
            // Read the touchpad values
           	Vector2 touchpad = device.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad);

            // Convert the touchpad y value to movement
            return touchpad.y;
        }

		// If the finger isn't on the touch pad, stop the movement
		return 0.0f;
    }

#endif
#endregion

#region Hololens
#if MODE_HOLOLENS
	[SerializeField]
	private Color               _inactiveColor		= Color.blue;	// Color of the player when the player is not selected
	[SerializeField]
	private Color 				_activeColor		= Color.green;	// Color of the player when the player is selected
    [SerializeField]
    private Transform       	_cursorTransform;   				// The transform of the cursor contains its position, rotation, and scale
    [SerializeField]
    private MeshRenderer    	_cursorRender;   			  	 	// The component that actually draws the cursor. We can use it to turn on and off the drawing of the cursor

    private GameObject      	_focusedObject;     				// The object we are currently targeting with our gaze

    private Vector3         	_headPos;           				// The current position of the Hololens, i.e. our head, in 3D space
    private Vector3         	_viewDir;           				// The direction of our gaze in 3D space

    private GestureRecognizer 	_recognizer;      					// Object that handles our gestures for the hololens

    private bool 				selected 			= false;    	// If we have our paddle selected or not

    private void Awake ()
    {
        _recognizer = new GestureRecognizer ();

        _recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            // If we are looking at the paddle, select/unselect it and change its color
            if (_focusedObject != null && _focusedObject == gameObject)
            {
                selected = !selected;
                PlayerMaterial.color = selected ? _activeColor : _inactiveColor;
            }
        };

        _recognizer.StartCapturingGestures ();
    }

    private void HandleHololensControls ()
    {
        // First update the head position and view direction
        _headPos = Camera.main.transform.position;
        _viewDir = Camera.main.transform.forward;

        // If we have our paddle selected, move it toward where you're looking
        if (selected)
        {
            // We calculate the unit vector pointing from the head to the paddle
            Vector3 headToPaddle = (transform.position - _headPos).normalized;

            // If the above direction is not the same as the view direction, move the paddle
            if (_viewDir.z != headToPaddle.z)
            {
                // This isn't 100% exact, but works as a rough approximation
                // If the z component of the view direction is larger than the direction to the paddle,
                // move the paddle along the positive z-axis, reduce this difference
                MovePlayer(_viewDir.z > headToPaddle.z ? 1 : -1);
            }
        }
        else
        {
            // Make sure the paddle is still, since it's not selected
            MovePlayer(0);
        }

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
    }

#endif
#endregion
}
