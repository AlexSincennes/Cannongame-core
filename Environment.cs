using UnityEngine;
using System.Collections;

public class Environment : MonoBehaviour
{
	//private int seed = 12247190;
	System.Random random;
	
	public int maxWindStrength = 100;

	//private int numBasePoints = 6; 		// 6 base points at 0,4,8,12,16,20
	private int basePointDiff = 4; 		// 4 between each of these
	private int numTotalPoints = 20; 	// 21 points total (0 to 20 inclusively)

	private int timeBetweenNewVals = 2;
	private float timeCurrent = 0;

	private const float FUZZ = 0.05f;


	public bool collision_detection = true;

	public bool deformation = true;

	public bool wind =  true;


	float[] pts_old;

	public float[] pts_cur;

	float[] pts_goal;

	// Use this for initialization
	void Start ()
	{
		random =  new System.Random();

		//Perlin noise 1D, 21 values
		pts_cur = new float[numTotalPoints+1];
		pts_cur = GenerateBasePoints();
		pts_cur = GenerateAllPoints(pts_cur);
		pts_cur = Smooth(pts_cur);

		pts_old = new float[numTotalPoints+1];
		for(int i =0; i<= numTotalPoints; i++)
			pts_old[i] = pts_cur[i];

		pts_goal = new float[numTotalPoints+1];
		pts_goal = GenerateBasePoints();
		pts_goal = GenerateAllPoints(pts_goal);
		pts_goal = Smooth(pts_goal);

		//turn these values into a gui texture for user to see!
		WindTextureMaker(pts_cur);

		/*
		string debug_str = "";
		for(int i=0; i<=numTotalPoints; i++)
			debug_str += (int)pts_cur[i] + ", ";
		Debug.Log(debug_str);
		*/
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.W))
		{
			wind = !wind;
		}
		if (Input.GetKeyDown (KeyCode.C))
		{
			collision_detection = !collision_detection;
		}
		if (Input.GetKeyDown (KeyCode.D))
		{
			deformation = !deformation;
		}

		timeCurrent+= Time.deltaTime;

		//update values to interpolate between when reach the goal
		if(timeBetweenNewVals - timeCurrent < FUZZ)
		{
			timeCurrent = 0;

			for(int i =0; i<= numTotalPoints; i++)
				pts_old[i] = pts_cur[i];

			pts_goal = GenerateBasePoints();
			pts_goal = GenerateAllPoints(pts_goal);
			pts_goal = Smooth(pts_goal);
		}

		//Debug.Log(timeCurrent / timeBetweenNewVals);

		pts_cur = lerp (pts_old, pts_goal, timeCurrent / timeBetweenNewVals);

		//turn these values into a gui texture for user to see!
		WindTextureMaker(pts_cur);

	}

	//make base points as per description above. completely random
	float[] GenerateBasePoints()
	{
		float[] points = new float[numTotalPoints+1];

		for(int i = 0; i <= numTotalPoints; i += basePointDiff)
		{
			points[i] = (float)random.Next(-maxWindStrength,maxWindStrength);
			//Debug.Log(points[i]);
		}

		return points;
	}

	// interpolate a value between p1 and p2. x is [0.1]. use points before/after since cubic
	float CubicInterpolate(float before_p1, float p1, float p2, float after_p2, float x)
	{
		float a = (after_p2 - p2) - (before_p1 - p1);
		float b = (before_p1 - p1) - a;
		float c = p2 - before_p1;
		float d = p1;

		//optimization
		float x_2 = x*x;
		float x_3 = x_2*x;

		return a * x_3 + b * x_2 + c * x + d;
	}

	// generate all the other points from the base points
	// using cubic interpolation
	float[] GenerateAllPoints(float[] pts)
	{
		for(int i=0; i<= numTotalPoints; i++)
		{
			//if not one of the base points
			if(i%basePointDiff !=0)
			{
				//optimization --> if 0 or max, we're on edges, so we need "dummies" to do cubic interpolation
				int increment = i/basePointDiff;

				float x  = (float)(i%basePointDiff)/basePointDiff;

				if(increment==0) //no left value -> make dummy left point
				{
					float dummy = (float)random.Next(-maxWindStrength, maxWindStrength);
					pts[i] = CubicInterpolate(dummy, pts[0], pts[basePointDiff], pts[2*basePointDiff], x);
				}
				else
				{
					if(increment == numTotalPoints/basePointDiff - 1) // no right value -> make dummy right point
					{
						float dummy = (float)random.Next(-maxWindStrength, maxWindStrength);
						pts[i] = CubicInterpolate(pts[numTotalPoints-2*basePointDiff], pts[numTotalPoints-basePointDiff], pts[numTotalPoints], dummy, x);
					}
					else // all other values i.e. from 5 to 15
					{

						pts[i] = CubicInterpolate(pts[(increment-1) * basePointDiff], pts[increment * basePointDiff], pts[(increment+1)*basePointDiff],  pts[(increment+2)*basePointDiff], x);
					}
				}
			}
		}

		return pts;
	}


	//smooth out the random values
	float[] Smooth(float[] vals)
	{
		for(int i=0; i<=numTotalPoints; i++)
		{
			// no i-1
			if(i==0)
				vals[i] = vals[i]/2  +  vals[i+1]/2;
			else
				if(i==numTotalPoints) // no i+1
					vals[i] = vals[i]/2  +  vals[i-1]/2;
				else // normal case
					vals[i] = vals[i]/2  +  vals[i-1]/4  +  vals[i+1]/4;
		}

		return vals;
	}

	void WindTextureMaker(float[] vals)
	{
		GameObject guiTex = GameObject.Find("Wind Meter Texture") as GameObject;

		Texture2D tex =  new Texture2D(1, 21, TextureFormat.ARGB32, false);

		for(int i=0; i <= numTotalPoints; i++)
		{
			// green for RIGHT
			if(vals[i] > 0)
				tex.SetPixel(1,i+1,new Color(0,vals[i]/maxWindStrength,0));
			else // red for LEFT
				tex.SetPixel(1,i+1,new Color(-vals[i]/maxWindStrength,0,0));
		}
		tex.Apply();

		guiTex.guiTexture.texture = tex;
	}

	//linear interpolation between old and new point sets over time
	float[] lerp(float[] pts_old, float[] pts_new, float t)
	{
		float[] lerp_vals = new float[numTotalPoints+1];

		for(int i=0; i <= numTotalPoints; i++)
		{
			lerp_vals[i] =  pts_old[i]*(1-t) + pts_new[i]*t;
		}

		return lerp_vals;
	}
}

