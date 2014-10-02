using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class TerrainGenerator : MonoBehaviour 
{

	private float scale = 0.06f;

	public System.Random random = new System.Random();

	private Point2d mountainOneLow = new Point2d(0,0);
	private Point2d mountainOnePeak = new Point2d(5,15);
	private Point2d mountainOneLow2 = new Point2d(15,0);

	//for physics to know when to start calculating collision
	public static int mountain2lowX = 20;
	public static int mountain2PeakX = 30;

	private Point2d mountainTwoLow = new Point2d(mountain2lowX,0);
	private Point2d mountainTwoPeak = new Point2d(mountain2PeakX,15);
	private Point2d mountainTwoLow2 = new Point2d(40,0);

	private int maxRecursiveDepth = 2;


	private const float cannonLocRatio = 0.66f;
	private float cannonLoc;
	private float peakX;
	private bool cannonCreated = false;
		
	private const float FUZZ = 0.05f;

	//for iterating through for physics
	public ArrayList mountain2points;
	
	void Start () 
	{
		//init
		mountain2points = new ArrayList();

		//place starting points
		GameObject gmountainOneLow = Instantiate(Resources.Load("Line")) as GameObject;
		gmountainOneLow.name = "Point "+mountainOneLow.X + " " + mountainOneLow.Y;
		gmountainOneLow.GetComponent<PointScript>().x = mountainOneLow.X;
		gmountainOneLow.GetComponent<PointScript>().y = mountainOneLow.Y;
		mountain2points.Add(mountainOneLow);

		GameObject gmountainOnePeak = Instantiate(Resources.Load("Line")) as GameObject;
		gmountainOnePeak.name = "Point "+mountainOnePeak.X + " " + mountainOnePeak.Y;
		gmountainOnePeak.GetComponent<PointScript>().x = mountainOnePeak.X;
		gmountainOnePeak.GetComponent<PointScript>().y = mountainOnePeak.Y;
		mountain2points.Add(mountainOnePeak);

		GameObject gmountainOneLow2 = Instantiate(Resources.Load("Line")) as GameObject;
		gmountainOneLow2.name = "Point "+mountainOneLow2.X + " " + mountainOneLow2.Y;
		gmountainOneLow2.GetComponent<PointScript>().x = mountainOneLow2.X;
		gmountainOneLow2.GetComponent<PointScript>().y = mountainOneLow2.Y;
		mountain2points.Add(mountainOneLow2);

		//***************************************

		GameObject gmountainTwoLow = Instantiate(Resources.Load("Line")) as GameObject;
		gmountainTwoLow.name = "Point "+mountainTwoLow.X + " " + mountainTwoLow.Y;
		gmountainTwoLow.GetComponent<PointScript>().x = mountainTwoLow.X;
		gmountainTwoLow.GetComponent<PointScript>().y = mountainTwoLow.Y;
		mountain2points.Add(mountainTwoLow);

		GameObject gmountainTwoPeak = Instantiate(Resources.Load("Line")) as GameObject;
		gmountainTwoPeak.name = "Point "+mountainTwoPeak.X + " " + mountainTwoPeak.Y;
		gmountainTwoPeak.GetComponent<PointScript>().x = mountainTwoPeak.X;
		gmountainTwoPeak.GetComponent<PointScript>().y = mountainTwoPeak.Y;
		mountain2points.Add(mountainTwoPeak);

		GameObject gmountainTwoLow2 = Instantiate(Resources.Load("Line")) as GameObject;
		gmountainTwoLow2.name = "Point "+mountainTwoLow2.X + " " + mountainTwoLow2.Y;
		gmountainTwoLow2.GetComponent<PointScript>().x = mountainTwoLow2.X;
		gmountainTwoLow2.GetComponent<PointScript>().y = mountainTwoLow2.Y;
		mountain2points.Add(mountainTwoLow2);




		cannonLoc = mountainOnePeak.Y * cannonLocRatio;
		peakX = mountainOnePeak.X;
		MipointBisectAlg(mountainOneLow, mountainOnePeak, 0);
		MipointBisectAlg(mountainOnePeak, mountainOneLow2, 0);

		MipointBisectAlg(mountainTwoLow, mountainTwoPeak, 0);
		MipointBisectAlg(mountainTwoPeak, mountainTwoLow2, 0);

		// create cannon near mountain one peak's next if we didn't find a place to put it
		// which would not happen in a more fine-grain mountain 
		if(!cannonCreated)
		{
			Point2d next = gmountainOnePeak.GetComponent<PointScript>().next;
			GameObject cannon = GameObject.Find("Cannon") as GameObject;
			cannon.transform.position = new Vector3(next.X+0.2f, next.Y, -0.2f);
		}


		//signal we're done loading
		GameObject.Find("Loading Text").GetComponent<GUIText>().text = "Loading... Done!\nPress W to toggle wind.\nC to toggle collisions.\nD to toggle deformation.";
	}

	void Update () 
	{
	
	}

	//returns the new point closest to highPt
	void MipointBisectAlg(Point2d leftPt, Point2d rightPt, int recCount)
	{
		// do midpoint bisect order depending on which is highest
		Point2d newPoint;
		if(leftPt.Y > rightPt.Y)
			newPoint= MidpointBisect(leftPt, rightPt);
		else
			newPoint = MidpointBisect(rightPt, leftPt);

		// collect points of mountain2 for collision detection
		if(newPoint.X >= mountain2lowX)
			mountain2points.Add(newPoint);

		GameObject g = Instantiate(Resources.Load("Line")) as GameObject;
		g.name = "Point "+newPoint.X + " " + newPoint.Y;
		g.GetComponent<PointScript>().x = newPoint.X;
		g.GetComponent<PointScript>().y = newPoint.Y;
		

		//if we're 2/3rds of the way up the hill (and on the side facing the other mountain), place the cannon
		//this is in the general algorithm, but really only applies to the first mountain
		if(!cannonCreated && Mathf.Abs(newPoint.Y - cannonLoc) < FUZZ && newPoint.X > peakX && newPoint.X < mountain2lowX)
		{
			cannonCreated = true;

			GameObject cannon = GameObject.Find("Cannon") as GameObject;
			//slight x-offset to position it appropriately
			cannon.transform.position = new Vector3(newPoint.X+0.2f, newPoint.Y, -0.2f);
		}

		if ( recCount == maxRecursiveDepth )
		{
			//at max depth, trace lines

			// left -> new
			GameObject g1 = GameObject.Find("Point " + leftPt.X + " " + leftPt.Y);
			LineRenderer lr1 = g1.GetComponent<LineRenderer>();
			lr1.SetPosition(0,new Vector3(leftPt.X, leftPt.Y, 0));
			lr1.SetPosition(1,new Vector3(newPoint.X, newPoint.Y, 0));

			// set next for collision detection
			g1.GetComponent<PointScript>().SetNext(newPoint.X, newPoint.Y);

			// new -> right line
			LineRenderer lr2 = g.GetComponent<LineRenderer>();
			lr2.SetPosition(0,new Vector3(newPoint.X, newPoint.Y, 0));
			lr2.SetPosition(1,new Vector3(rightPt.X, rightPt.Y, 0));

			// set next for collision detection
			g.GetComponent<PointScript>().SetNext(rightPt.X, rightPt.Y);

		}
		else
		{
			recCount++;
			MipointBisectAlg(leftPt, newPoint, recCount);
			MipointBisectAlg(newPoint, rightPt, recCount);
		}
	}


	// wherein the high point is the one with highest Y
	Point2d MidpointBisect(Point2d highPt, Point2d lowPt)
	{
		// find midpoint on line
		float mx = (highPt.X + lowPt.X) / 2;
		float my = (highPt.Y + lowPt.Y) / 2;

		float dx = highPt.X - lowPt.X;
		float dy = highPt.Y - lowPt.Y;

		//find normal (the one pointing out)
		float nx = -dx;
		float ny = dy;

		//line length for scale
		float lineLength = Mathf.Sqrt(dx*dx + dy*dy);

		//scale factor --> depends on line length
		int sign = random.Next(0,2) == 0 ? -1 : 1;
		float scaleFactor = scale * Mathf.Sqrt(lineLength) * (float)random.NextDouble() * sign;


		float newX = mx + scaleFactor * nx;
		float newY = my + scaleFactor * ny;

		Point2d newPoint = new Point2d(newX, newY);

		return newPoint;

	}
		
}
