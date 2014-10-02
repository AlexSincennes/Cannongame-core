using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour
{
	public float delay = 0.5f;
	public float cannonLength = 0.80f;

	public float maxImpulse = 50;
	public float minImpulse = 5;

	public float currentImpulse = 25;


	private bool canFire =  true;

	//0 -> horizontal, 90 -> vertical
	private float cannonAngle = 0;
	public float cannonRadians = 0;
	
	public int maxDegrees = 90;
	public int minDegrees = 0;


	// Use this for initialization
	void Start ()
	{
		InvokeRepeating( "AllowFire", delay, delay);
	}

	// Update is called once per frame
	void Update ()
	{
		//update GUIs
		GameObject.Find("Cannon Speed Text").GetComponent<GUIText>().text = "Cannon Muzzle Impulse: " + currentImpulse;
		GameObject.Find("Cannon Angle Text").GetComponent<GUIText>().text = "Cannon Angle Degrees : " + cannonAngle;

		if (Input.GetKeyDown (KeyCode.Space) && canFire)
    	{
			canFire = false;
			GameObject cannonball = Instantiate(Resources.Load("CannonBall")) as GameObject;
			float x = this.transform.position.x + cannonLength * Mathf.Cos(cannonRadians);
			float y = this.transform.position.y + cannonLength * Mathf.Sin(cannonRadians);
			float z = this.transform.position.z - 0.1f;

			cannonball.transform.position = new Vector3(x,y,z);
		}

		if( Input.GetKey(KeyCode.UpArrow))
   		{
			if(cannonAngle <= maxDegrees)
			{
				cannonAngle++;
				cannonRadians = Mathf.PI/180 * cannonAngle;
				transform.Rotate(-Vector3.up);

				//make wheel turn!
				foreach (Transform child in transform)
				{
					child.Rotate(2 * Vector3.up);
				}
			}
		}

		if( Input.GetKey(KeyCode.DownArrow ))
		{
			if(cannonAngle >= minDegrees)
			{
				cannonAngle--;
				cannonRadians = Mathf.PI/180 * cannonAngle;
				transform.Rotate(Vector3.up);
				foreach (Transform child in transform)
				{
					child.Rotate(-2 * Vector3.up);
				}
			}
		}

		if( Input.GetKey(KeyCode.RightArrow ))
		{
			if(currentImpulse <= maxImpulse)
			{
				currentImpulse++;
			}
		}

		if( Input.GetKey(KeyCode.LeftArrow ))
		{
			if(currentImpulse >= minImpulse)
			{
				currentImpulse--;
			}
		}

		//set to max if exceeded
		if(cannonAngle > maxDegrees)
		{
			transform.Rotate((cannonAngle-maxDegrees) * Vector3.up);
			cannonAngle = maxDegrees;
			cannonRadians = Mathf.PI/180 * cannonAngle;

		}
		
		//set to min if went below
		if(cannonAngle < minDegrees)
		{
			transform.Rotate((cannonAngle-minDegrees) * Vector3.up);
			cannonAngle = minDegrees;
			cannonRadians = Mathf.PI/180 * cannonAngle;
		}

		//set to max if exceeded
		if(currentImpulse > maxImpulse)
			currentImpulse = maxImpulse;
		
		//set to min if went below
		if(currentImpulse < minImpulse)
			currentImpulse = minImpulse;
		
	}

	void AllowFire()
	{
		canFire =  true;
	}
}

