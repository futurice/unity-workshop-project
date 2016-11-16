using UnityEngine;
using System.Collections;

public class GoalController : MonoBehaviour
{
	[SerializeField]
	private GameManager.GameEvent _triggeredEvent = GameManager.GameEvent.NONE;

	private void OnTriggerEnter (Collider other)
	{
		// Send the associated game event to the GameManager
		GameManager.Instance.HandleGameEvent (_triggeredEvent);
	}
}
