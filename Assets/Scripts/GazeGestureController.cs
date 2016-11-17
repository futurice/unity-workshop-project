using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GazeGestureController : MonoBehaviour {

    [SerializeField]
    private Transform _cursorTransform;
    [SerializeField]
    private MeshRenderer _cursorRender;

    private GameObject _focusedObject;
    private GameObject _oldFocus;

    private Vector3 _headPos;
    private Vector3 _viewDir;

    private GestureRecognizer _recognizer;

	// Use this for initialization
	void Awake ()
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
	
	// Update is called once per frame
	void Update ()
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
}
