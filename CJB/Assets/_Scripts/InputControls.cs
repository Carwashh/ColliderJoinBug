using UnityEngine;

public class InputControls : MonoBehaviour
{
	[SerializeField] private Movement movement;

	[Header("Controls")]
	[SerializeField] private KeyCode accelerate = KeyCode.W;
	[SerializeField] private KeyCode brake = KeyCode.S;
	[SerializeField] private KeyCode steerLeft = KeyCode.A;
	[SerializeField] private KeyCode steerRight = KeyCode.D;
	
	public Movement Movement
	{
		get { return movement; }
		set { movement = value; }
	}

	private void Update()
	{
		// Acceleration
		if (Input.GetKey(accelerate)) { Movement.ControlAcceleration(); }
		if (Input.GetKey(brake)) { Movement.ControlBrake(); }

		// Steering
		if (Input.GetKey(steerLeft)) { Movement.ControlTurning(-1f); }
		if (Input.GetKey(steerRight)) { Movement.ControlTurning(1f); }
	}
}