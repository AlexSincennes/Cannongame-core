using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class PointScript : MonoBehaviour
{
	public float x;
	public float y;

	public Point2d next;

	public void SetNext(float xx, float yy)
	{
		next = new Point2d(xx,yy);
	}
}

