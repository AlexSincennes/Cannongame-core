using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class CannonBall : MonoBehaviour
{
	public float gravity = 9.8f;

	private float velocityX;
	private float velocityY;

	public float mass = 2.0f;

	//coefficient of fricition
	public float mu = 0.01f;

	public float windResistance = 0.0001f;

	//so as to not make wind too strong
	private float windSpeedNormalizer = 0.002f;

	// timeout time for cannonball at rest	
	private int timeout = 5;
	private float timeIdle = 0;
	
	private const float FUZZ = 0.05f;

	private bool collided = false;

	private bool airborne = true;

	//scale of destruction of terrain
	private float scale = 0.006f;

	// Use this for initialization
	void Start ()
	{
		CannonController cannon = GameObject.Find ("Cannon").GetComponent<CannonController>();

		velocityX = Mathf.Cos (cannon.cannonRadians) * cannon.currentImpulse / mass;
		velocityY = Mathf.Sin (cannon.cannonRadians) * cannon.currentImpulse / mass;

	}

	void Update()
	{
		//destroy if projectile went below Y=0
		if(transform.position.y < 0)
			Destroy(this.gameObject);

		//destroy if went out of bounds in x direction
		if(transform.position.x >= 40)
			Destroy (this.gameObject);

		//destroy if idle i.e. not moving for too long
		if(velocityX < FUZZ && velocityY < FUZZ)
		{
			if(timeIdle > timeout)
				Destroy(this.gameObject);
			else
				timeIdle += Time.deltaTime;
		}
		else
		   timeIdle = 0;
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		Environment env = GameObject.Find("Main Camera").GetComponent<Environment>();

		//update position and velocity

		float x = transform.position.x + velocityX*Time.deltaTime;
		float y = transform.position.y + velocityY*Time.deltaTime - gravity*(Time.deltaTime*Time.deltaTime)/2;
	

		transform.position = new Vector3(x,y,transform.position.z);


		if(airborne)
		{
			//calculate wind resistance
			velocityX -= windResistance*velocityX/mass * Time.deltaTime;

			if(env.wind)
				// add wind speed (positve or negative) --> wind speed at y-th altitude
				if(transform.position.y <= 20 && transform.position.y >= 0)
					velocityX += windSpeedNormalizer * env.GetComponent<Environment>().pts_cur[(int)transform.position.y];
				else // if above 20, just take the speed at 20
					velocityX += windSpeedNormalizer * env.GetComponent<Environment>().pts_cur[20];

			//account for gravity
			velocityY -= gravity * Time.deltaTime;
		}



		//****************************
		if(env.collision_detection)
			CollisionDetection();

		//****************************

	}

	void CollisionDetection()
	{	
		TerrainGenerator tgen = GameObject.Find ("Main Camera").GetComponent<TerrainGenerator>();
		
		// calculate if colliding with second mountain
		// only if intersection possible i.e. x >= mountain2's lowest x
		if(transform.position.x > TerrainGenerator.mountain2lowX)
		{
			// find closest point x-wise from the LEFT (because lines go from left to right in this program)
			// find minimum POSITIVE difference of cannonball.x - point.x
			
			float min = float.MaxValue;
			Point2d minPt = null;
			foreach( Point2d pt in tgen.mountain2points)
			{
				float diff = this.transform.position.x - pt.X;
				
				if( diff > 0)
				{
					if(min > diff)
					{
						min = diff;
						minPt = new Point2d(pt.X, pt.Y);
					}
				}
			}
			
			if(minPt != null && minPt.X < 40) // if such a point exists and is not the very last one (i.e. no NEXT)
			{
				
				// see if cannonbal is on line from this point to its next point
				// i.e. if y = mx + b for cannonball's x

				// get next of minPt
				GameObject minPtasGO = GameObject.Find("Point " + minPt.X + " " + minPt.Y);
				Point2d next = minPtasGO.GetComponent<PointScript>().next;
				
				// get slope
				float m = (next.Y - minPt.Y)/(next.X - minPt.X);
				
				// get Y-intercept
				float b = minPt.Y - minPt.X * m;
				
				// find y of line at cannonball's x
				float y = m*transform.position.x + b;
				
				// look for collision : they're close to each other
				if(transform.position.y - y <= FUZZ)
				{
					//wind should not affect this
					airborne = false;

					//if negative -> force on the correct side of hill
					if(transform.position.y - y < -FUZZ)
						transform.position= new Vector3(transform.position.x, y + FUZZ, transform.position.z);

					//put at rest if collision, also DESTROY some mountain terrain
					if(!collided )
					{
						collided = true;

						//*************************************************
						// destroy local mountain terrain
						if(GameObject.Find("Main Camera").GetComponent<Environment>().deformation)
						{

							GameObject nextAsGO = GameObject.Find ("Point " + next.X + " " + next.Y) as GameObject;

							//line length for scale
							float dx = next.X - minPt.X;
							float dy = next.Y - minPt.Y;
							float lineLength = Mathf.Sqrt(dx*dx + dy*dy);

							//find normal (the one pointing out)
							int sign = dy > 0 ? -1 : 1;
							float nx = sign * dx;
							float ny = dy;

							float scaleFactor = - scale * Mathf.Sqrt(lineLength) * (float)tgen.random.NextDouble() * (Mathf.Abs(velocityX) + Mathf.Abs(velocityY));

							//shift the point
							float newX = next.X + scaleFactor * nx;
							float newY = next.Y + scaleFactor * ny;


							// if new X is beyond its peak's X, prevent change (it would break mountain)
							// add a little bit of margin to prevent perfectly vertical slope (which is VERY BAD)
							// also if too close to next's next it's bad
							if ((next.X < TerrainGenerator.mountain2PeakX && newX + FUZZ < TerrainGenerator.mountain2PeakX && nextAsGO.GetComponent<PointScript>().next.X - next.X > FUZZ) || 
							    (next.X > TerrainGenerator.mountain2PeakX && newX - FUZZ > TerrainGenerator.mountain2PeakX && nextAsGO.GetComponent<PointScript>().next.X - next.X < FUZZ))
							{

								//update references to this point if it's okay
								minPtasGO.GetComponent<PointScript>().SetNext(newX, newY);
								minPtasGO.GetComponent<LineRenderer>().SetPosition(1,new Vector3(newX, newY, 0));
								nextAsGO.GetComponent<LineRenderer>().SetPosition(0,new Vector3(newX, newY, 0));
								nextAsGO.name = "Point " + newX + " " + newY;
								nextAsGO.GetComponent<PointScript>().x = newX;
								nextAsGO.GetComponent<PointScript>().x = newY;

								//update point in list
								foreach( Point2d pt in tgen.mountain2points)
								{
									if(Mathf.Abs(pt.X - next.X) < FUZZ && Mathf.Abs(pt.Y - next.Y) < FUZZ)
									{
										pt.X = newX;
										pt.Y = newY;
									}
								}

								//indicate hit textually
								GameObject.Find("Loading Text").GetComponent<GUIText>().text = "Hit successfully \ndeformed mountain!";
							}
							else // display warning to explain this
							{
								GameObject.Find("Loading Text").GetComponent<GUIText>().text = "Warning! Can't deform \nthat vertex any more!";
							}
						}

						//********************************
						// kill velocity (we needed it to calculate destruction)
						velocityX = 0;
						velocityY = 0;

					}
					else // already collided: sliding down slope
					{
						// have velocity relate to hill slope
						float slopeAngle = Mathf.Atan (m);

						velocityX += mu * -m/mass * gravity * Mathf.Cos (slopeAngle);

						velocityY += mu * -m/mass * gravity * Mathf.Sin (slopeAngle);
					}
				}
				else //back in the air: bring back wind resistance and wind speed
					airborne = true;
			}
		}
	}
}

