using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RectTouchEdit : MonoBehaviour {

	public RectTransform loadedRect;

	[HideInInspector] 
	public int screenWidth,screenHeight,lastTouchCount;

	public float rotateMultiplier,zoomMultiplier,currentTouchDistance;

	/// <summary>
	/// Values for maximum positions the object can move to.
	/// </summary>
	public float maxYposition,maxXPosition;

	[HideInInspector] 
	public Vector2 currentPosition,touch1First,touch2First,touch1Current,touch2Current,lastPosition;

	private float distanceTreshold, angleTreshold;

	private float pinchAmount,firstDistance,currentDistance,lastDistance,firstAngle,currentAngle,lastAngle;

	//To check if first touch position has been saved.
	private bool firstTouchChecked;

	//To prevent position jumping when a finger is lifted.
	private bool anyTouchLifted;

	Touch touch1,touch2;

	private bool[] firstTouchTaken;

	void Start()
	{
		//Initialization
		rotateMultiplier = 1;
		zoomMultiplier = 2;

		distanceTreshold = 0.01f;
		angleTreshold = 0.5f;
	
		loadedRect = GetComponent<RectTransform>();

		screenWidth = Screen.width;
		screenHeight = Screen.height;

		maxYposition = screenHeight*0.2f;
		maxXPosition = screenWidth*0.35f;

		firstTouchTaken = new bool[2];

	}

	//Scale rotate and move the RectTransform
	void LateUpdate() 
	{
		CalculateTouches();

		// Scale
		if (Mathf.Abs(currentDistance) > 0) 
		{ 
			
			pinchAmount = currentDistance*zoomMultiplier;

			//Constraint the max scale value of the rect.
			if(loadedRect.localScale.x > 1.5f)
			{
				loadedRect.localScale = new Vector2(1.5f,1.5f);
			}
				
			if(loadedRect.localScale.x + pinchAmount > 0.1f && loadedRect.localScale.y + pinchAmount > 0.1f )
			{
				
				loadedRect.localScale = new Vector3(loadedRect.localScale.x + pinchAmount,loadedRect.localScale.y + pinchAmount,1);
			}
		}

		// Rotate
		if (Mathf.Abs(currentAngle) > 0)
		{ 
			
			Quaternion desiredRotation = transform.rotation;
			Vector3 rotationDeg = Vector3.zero;
			rotationDeg.z = currentAngle*rotateMultiplier;
			desiredRotation *= Quaternion.Euler(rotationDeg);

			transform.localRotation = desiredRotation;
		}

		//Get the delta distance of touch position from last frame
		Vector2 deltaVec = currentPosition - lastPosition;
		currentTouchDistance = deltaVec.magnitude;

		//If any touch hasn't been lifted since last frame ...
		if(anyTouchLifted == false)
		{
			//... if current distance is greater than treshold ...
			if(currentTouchDistance > 0.01f)
			{
				//... move the object according to the delta position.
				VectorHVMovement(deltaVec);
			}
		}
		else
		{
			//... set bool to false for next frame calculation.
			anyTouchLifted = false;
		}

		lastPosition = currentPosition;

	}

	/// <summary>
	/// Calculate the touches for pinch, rotate and movement actions.
	/// </summary>
	private void CalculateTouches()
	{
		
		if(Input.touchCount == 1)
		{
			touch1 = Input.touches[0];
			currentPosition = new Vector2(touch1.position.x/screenWidth , touch1.position.y/screenHeight);

			//If the first position of the touch hasn't been checked ...
			if(firstTouchChecked == false)
			{
				/*
				 * Get the first touch position and assign it to the last position
				 * so that when user first touches the screen
				 * image doesnt jump to the finger position.
				*/
				Debug.Log("Touch 0 began!");
				lastPosition = currentPosition;
				firstTouchChecked = true;
			}


		}

		//If there are 2 touches...
		if (Input.touchCount == 2) 
		{
			lastTouchCount = 2;
			touch1 = Input.touches[0];
			touch2 = Input.touches[1];

			//... get the middle point of the touch delta positions.
			currentPosition = (touch1.position + touch2.position) / 2;
			currentPosition = new Vector2(currentPosition.x/screenWidth , currentPosition.y/screenHeight);


			//Get the percentage position of the first touch according to the screen
			if(firstTouchTaken[0] == false)
			{
				Debug.Log("Touch 1 Began");
				touch1First = new Vector2(touch1.position.x/screenWidth , touch1.position.y/screenHeight);
				firstTouchTaken[0] = true;
			}



			//Get the percentage position of the second touch according to the screen
			if(firstTouchTaken[1] == false)
			{
				Debug.Log("Touch 2 Began");
				touch2First = new Vector2(touch2.position.x/screenWidth , touch2.position.y/screenHeight);

			
				lastPosition = currentPosition;

				//... get the initial distance of the touches.
				firstDistance = Vector2.Distance(touch1First,touch2First);
				lastDistance = firstDistance;
				currentDistance = 0;

				//... get the initial angle of the touches.
				firstAngle = AngleBetween2Vectors(touch1First,touch2First);
				lastAngle = firstAngle;
				currentAngle = 0;

				firstTouchTaken[1] = true;
			}
				
			if(touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
			{
				//Get the current position of the touch values as float (0-1).
				touch1Current = new Vector2(touch1.position.x/screenWidth , touch1.position.y/screenHeight);
				touch2Current = new Vector2(touch2.position.x/screenWidth , touch2.position.y/screenHeight);

				float distance = Vector2.Distance(touch1Current , touch2Current);
				float distanceDelta = distance - lastDistance;

				//If distance is greater than treshold save distance value for scaling.
				if(Mathf.Abs(distanceDelta) > distanceTreshold && Mathf.Abs(distanceDelta) < 0.1f)
				{
					currentDistance = distanceDelta;
				}
				else
				{
					currentDistance = 0;
				}

				//Save the last distance value
				lastDistance = distance;

				//Get the current angle of the touches ...
				float angle = AngleBetween2Vectors(touch1Current , touch2Current);
				float angleDelta = Mathf.DeltaAngle (angle , lastAngle);

				//If distance is greater than treshold save angle value for rotating.
				if(Mathf.Abs(angleDelta) > angleTreshold && Mathf.Abs(angleDelta) < 30)
				{
					currentAngle = angleDelta;
				}
				else
				{
					currentAngle = 0;
				}

				lastAngle = angle;
			}

		}

		//If  there are more or less than 2 touches, reset the values to stop unnecessary movement.
		if(Input.touchCount != 2)
		{
			
			firstTouchTaken[0] = false;
			firstTouchTaken[1] = false;
			currentAngle = 0;
			currentDistance = 0;
			lastAngle = 0;
			lastDistance = 0;

		}

		if(Input.touchCount == 0)
		{
			firstTouchChecked = false;
		}

		//If any touch has been lifted ...
		if(Input.touchCount < lastTouchCount)
		{
			//... set bool for not calculating next frame.
			Debug.Log("A touch has been lifted!");
			anyTouchLifted = true;
		}

		//Get the last touch count and save it.
		lastTouchCount = Input.touchCount;

	}

	/// <summary>
	/// Get the angle between two vectors relative to Up vector (1,0)
	/// </summary>
	/// <returns>The between2 vectors.</returns>
	/// <param name="vec1">Vec1.</param>
	/// <param name="vec2">Vec2.</param>
	private float AngleBetween2Vectors(Vector2 vec1, Vector2 vec2)
	{
		Vector2 diference = vec2 - vec1;
		float sign = (vec2.x < vec1.x)? -1.0f : 1.0f;
		return Vector2.Angle(Vector2.up, diference) * sign;
	}

	/// <summary>
	/// Movement of the object according to the vector.
	/// </summary>
	/// <param name="deltaVector">Delta vector.</param>
	private void VectorHVMovement(Vector2 deltaVector)
	{
		/* 
		 * If statements are seperated because of frame rate differences
		 * on different devices causes the image to
		 * move beyond constraints.
		 */

		//Right border
		if(transform.localPosition.x < maxXPosition && deltaVector.x > 0)
		{
			transform.localPosition = new Vector2(transform.localPosition.x + deltaVector.x*screenWidth, transform.localPosition.y);
		}
		//Left border
		if(transform.localPosition.x > -maxXPosition && deltaVector.x < 0)
		{
			transform.localPosition = new Vector2(transform.localPosition.x + deltaVector.x*screenWidth, transform.localPosition.y);
		}
		//Top border
		if(transform.localPosition.y < maxYposition && deltaVector.y > 0)
		{
			transform.localPosition = new Vector2(transform.localPosition.x , transform.localPosition.y + screenHeight*deltaVector.y);
		}
		//Bottom border
		if(transform.localPosition.y > -maxYposition &&  deltaVector.y < 0)
		{
			transform.localPosition = new Vector2(transform.localPosition.x , transform.localPosition.y + screenHeight*deltaVector.y);
		}
	}
}
