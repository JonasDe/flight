using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class FlightHandlerPhys : MonoBehaviour
{

    public VRTK_BodyPhysics VRTK_bodyPhysics;
    private AudioManager audioManager;
    private GameObject actualLeftController;
    private GameObject actualRightController;

    public enum FlightState
    {
        Flying,
        Gliding,
        Falling,
        Ground
    }
    public enum RotationMode
    {
        Linear,
        Sine
    }
    public enum EngageMode
    {
        Automatic,
        Toggle
    }
    public enum FlightDirection
    {
        ControllerMovementAveraged,
        ControllerDirectionAveraged,
        AlwaysUp,
    }



    [Tooltip("If this is checked then the left controller will toggle flight.")]
    public bool leftController = true;
    [Tooltip("If this is checked then the right controller will toggle flight.")]
    public bool rightController = true;
    [Tooltip("Y-velocity multiplier when flapping.")]
    public float verticalAcceleration = 6f;
    [Tooltip("XZ-velocity multiplier when flapping or gliding.")]
    public float horizontalAcceleration = 1.0f;
    [Tooltip("Max Y-velocity.")]
    public float maxHorizontalSpeed = 4;
    [Tooltip("Max XZ-velocity.")]
    public float maxVerticalSpeed = 4;
    [Tooltip("Increasing this can give more directional control of the flight.")]
    public float airDrag = 1.0f;
    [Tooltip("Amount of samples looked at when averaging controller position and rotation vector. Used to determine thrust force and upwards/downwards hand movements and rotation.")]
    public int controllerHistorySampleSize = 6;

    [Range(0.0f, 100.0f), Tooltip("100% = zero gravity, 0% = normal gravity fall speed.")]
    public float gravityReduction = 50;

    [Tooltip("Keeping the controllers still for this many seconds is needed to leave flight-mode and go to either Falling or Gliding state.")]
    public float handMovementTimeThreshold = 0.001f;

    [Tooltip("Determines how fast the current velocity decellerates when changing direction to opposite the current velocity.")] 
    public float brakingSpeed = 0.3f;

    [Range(0.0f, 90.0f), Tooltip("The degrees of the controller rotation at which maximum horizontal speed is achieved.")]
    public float maxRotation = 45;

    [Tooltip("Determines how speed is increased when rotating the controller.")] 
    public RotationMode rotationMode = RotationMode.Sine;

    [HideInInspector]
    public FlightDirection flightDirection;

	[Range(0.0f, 100.0f)]
	public float diveIntensity = 100;

    [Tooltip("Will also increase fall speed when rotating in glide mode if enabled.")]
    public bool increaseFallSpeedWithRotation = false;

    [HideInInspector]
    public EngageMode engageMode = EngageMode.Automatic;
    [HideInInspector]
    public VRTK_ControllerEvents.ButtonAlias engageButton = VRTK_ControllerEvents.ButtonAlias.Touchpad_Press;
    [HideInInspector]
    public float glideControllerMinDistance = 1.5f;
    [HideInInspector]
    public float minThrustForceThreshold = 0.01f;

	[Range(0.0f, 90.0f)]
	public float tiltIntensity = 45.0f;

    public bool canGlideBackwards = true;

    [Tooltip("The maximum degree per fixed update with which the player can turn.")]
    public float maxTurnRate = 4.0f;

    public float timeBeforeGlideSound = 1.0f;
    private Vector3 averageControllerThrust;
    private Vector3 averageControllerMovement;
    private Queue<Vector3> controllerThrustHistory;
    private Queue<Vector3> controllerMovementHistory;
    private Queue<Vector3> controllerDirectionHistory;
    private bool leftSubscribed;
    private bool rightSubscribed;
    private Transform playAreaTransform;
    private Vector3 prevRightControllerPos;
    private Vector3 prevLeftControllerPos;
    private FlightState flightState = FlightState.Ground;

    private bool soundReady = true;
    private GameObject VRTK_leftController;
    private GameObject VRTK_rightController;
    private Vector3 averageControllerDirection = Vector3.zero;

    private Vector3 rightControllerPosDiff;
    private Vector3 leftControllerPosDiff;
    private Rigidbody playerRigidbody;
    private VRTK_ControllerEvents.ButtonAlias previousEngageButton;
    private bool engageActive;
    private bool controllersMovingUp;
    private VirtualArrow arrow;

    private float maxGlideInclineAngle = 89.0f;
    private float stillHandsCheck;
    private float timeGliding = 0.0f;


    void Start()
    {
        audioManager = GameObject.FindWithTag("AudioCenter").GetComponent<AudioManager>();
        VRTK_leftController = VRTK_DeviceFinder.GetControllerLeftHand();
        VRTK_rightController = VRTK_DeviceFinder.GetControllerRightHand();
        actualLeftController = VRTK_DeviceFinder.GetControllerLeftHand(getActual: true);
        actualRightController = VRTK_DeviceFinder.GetControllerRightHand(getActual: true);
    }

    protected virtual void OnEnable()
    {
        controllerThrustHistory = new Queue<Vector3>();
        controllerMovementHistory = new Queue<Vector3>();
        controllerDirectionHistory = new Queue<Vector3>();
        previousEngageButton = engageButton;
        VRTK_leftController = VRTK_DeviceFinder.GetControllerLeftHand();
        VRTK_rightController = VRTK_DeviceFinder.GetControllerRightHand();
        actualLeftController = VRTK_DeviceFinder.GetControllerLeftHand(getActual: true);
        actualRightController = VRTK_DeviceFinder.GetControllerRightHand(getActual: true);

        if (!actualLeftController)
        {
            Debug.LogError("No left controller found. Make sure you attach the Controller GameObject to the Flight Handler.");
        }
        if (!actualRightController)
        {
            Debug.LogError("No right controller found. Make sure you attach the Controller GameObject to the Flight Handler.");
        }

        playAreaTransform = VRTK_DeviceFinder.PlayAreaTransform();
        playerRigidbody = playAreaTransform.GetComponent<Rigidbody>();
        SetControllerListeners(VRTK_leftController, leftController, ref leftSubscribed);
        SetControllerListeners(VRTK_rightController, rightController, ref rightSubscribed);

    }

    protected virtual void OnDisable()
    {
        FlightOff();
    }
    public FlightState GetFlightState() { return flightState; }

    protected virtual void FixedUpdate()
    {
        UpdateSamples();

        if (FlightToggled())
        {
            FlightOn();
            UpdateFlightState();
        }
        else
        {
            FlightOff();
        }
    }

    private bool ArmsOut() { return Vector3.Distance(prevLeftControllerPos, prevRightControllerPos) > glideControllerMinDistance; }

    private bool FlightToggled()
    {
        if (engageMode == EngageMode.Toggle && !engageActive) return false;
        return VRTK_rightController.activeInHierarchy && VRTK_leftController.activeInHierarchy;
    }

    private void TrimQueue(Queue<Vector3> q, int size)
    {
        if (q.Count > size)
        {
            q.Dequeue();
        }
    }

    private void UpdateSamples()
    {
        var rightControllerPos = actualRightController.transform.localPosition;
        var leftControllerPos = actualLeftController.transform.localPosition;

        rightControllerPosDiff = prevRightControllerPos - rightControllerPos;
        leftControllerPosDiff = prevLeftControllerPos - leftControllerPos;

        prevRightControllerPos = rightControllerPos;
        prevLeftControllerPos = leftControllerPos;
        controllerThrustHistory.Enqueue(CalculateThrustVector());
        controllerMovementHistory.Enqueue(ControllerPosDiff());
        controllerDirectionHistory.Enqueue(WeightedForwardDirection());

        TrimQueue(controllerThrustHistory, controllerHistorySampleSize);
        TrimQueue(controllerDirectionHistory, controllerHistorySampleSize);
        TrimQueue(controllerMovementHistory, controllerHistorySampleSize);
        UpdateAverages();
    }

    // Returns the directional forward vector given that the right-hand controller is held in the right hand, and left-hand controller
    // is held in the left hand. Otherwise the directional vector it will be reversed.

    private Vector3 CalculateThrustVector()
    {
        Vector3 flightVector = ControllerPosDiff();
        controllersMovingUp = flightVector.y > 0;
        Vector3 direction = NormalizedXZControllerRotationDirection();
        return controllersMovingUp ? flightVector : Vector3.zero;
    }

    private Vector3 NormalizedXZControllerRotationDirection()
    {
        Vector3 diff = actualLeftController.transform.position - actualRightController.transform.position;
        diff.y = 0;
        diff = Vector3.Normalize(diff);
        Vector3 backwards = Vector3.Cross(diff, Vector3.up);
        Vector3 direction = Vector3.zero;
        if (ControllerRotatedBackwards())
        {
            direction = backwards;
        }
        else if (ControllerRotatedForward())
        {

            direction = -backwards;
        }
        return direction;

    }

    private float RotationWeight()
    {
        float weight = 0;
        float rotation = AvgXZControllerRotationInDegrees();
        // Past max rotation angle, gives full weight
        if (rotation > maxRotation && rotation < (360 - maxRotation)) return 1;
        // Always work in right upper quadrant for simplicity
        if (rotation > 270) rotation = 360 - rotation;

        switch (rotationMode)
        {
            case RotationMode.Sine:
                float rescaleFactor = 90.0f / maxRotation;
                rotation = rotation * rescaleFactor;
                weight = (float)Math.Sin(Mathf.Deg2Rad * rotation);
                break;
            case RotationMode.Linear:

                weight = rotation / maxRotation;
                break;
        }
        return weight;
    }
    public Vector3 WeightedForwardDirection() { return NormalizedXZControllerRotationDirection() * RotationWeight(); }
    public bool ControllerRotatedForward() { return AvgXZControllerRotationInDegrees() >= 0 && AvgXZControllerRotationInDegrees() <= 180; }

    public bool ControllerRotatedBackwards() { return AvgXZControllerRotationInDegrees() < 360 && AvgXZControllerRotationInDegrees() > 180; }

    private float AvgXZControllerRotationInDegrees()
    {
        float leftXZRotation = 360 -  actualLeftController.transform.rotation.eulerAngles.z;
        float rightXZRotation = actualRightController.transform.rotation.eulerAngles.z;

        // Needed since average value drops when one hand loops around the modulus ring while the other doesnt. 
        // Lefthand angle = 10 , righthand = 350 gives average of 180. While Lefthand 350 and righthand 350 gives 350. The below check ensures no sudden drop occurs.
        // Note: One could also shift both values by 180 for more consistency.
        if (leftXZRotation < maxGlideInclineAngle && rightXZRotation > maxGlideInclineAngle)
        {
            return rightXZRotation;
        }
        else if (leftXZRotation > maxGlideInclineAngle && rightXZRotation < maxGlideInclineAngle)
        {
            return rightXZRotation;
        }
        return (rightXZRotation + leftXZRotation) / 2;

    }

    private void UpdateAverages()
    {
        Vector3 thrustSum = Vector3.zero;
        Vector3 movementSum = Vector3.zero;
        Vector3 directionSum = Vector3.zero;

        var tIt = controllerThrustHistory.GetEnumerator();
        var dIt = controllerDirectionHistory.GetEnumerator();
        var mIt = controllerMovementHistory.GetEnumerator();
        while (tIt.MoveNext() && dIt.MoveNext() && mIt.MoveNext())
        {
            thrustSum = thrustSum + tIt.Current;
            movementSum = movementSum + mIt.Current;
            directionSum = directionSum + dIt.Current;
        }
        averageControllerMovement = movementSum / controllerHistorySampleSize;
        averageControllerThrust = thrustSum / controllerHistorySampleSize;
        averageControllerDirection = directionSum / controllerHistorySampleSize;
    }

    public Rigidbody Rigidbody() { return playerRigidbody; }

    private Vector3 ControllerPosDiff() { return (leftControllerPosDiff + rightControllerPosDiff) / 2; }

    private float GetAvgThrustMagnitude() { return Vector3.Magnitude(averageControllerThrust); }

    private float GetAvgMovementMagnitude() { return Vector3.Magnitude(averageControllerMovement); }

    private bool HandsMoving()
    {
        return !((Time.time - stillHandsCheck) > handMovementTimeThreshold);
    }

    private bool MovingHandsDown() { return Vector3.Normalize(Vector3.Project(averageControllerMovement, Vector3.up)) == Vector3.up; }

    private bool IsGliding()
    {
        bool handsStill = GetAvgThrustMagnitude() < minThrustForceThreshold;
        return ArmsOut() && (playerRigidbody.velocity.y < 0) && handsStill;
    }

    private bool IsFlappingUp()
    {
        bool movingHandsUp = MovingHandsDown();
        return GetAvgThrustMagnitude() > minThrustForceThreshold && movingHandsUp;
    }

    private bool IsFlappingDown()
    {
        bool movingHandsDown = !MovingHandsDown();
        return GetAvgMovementMagnitude() > minThrustForceThreshold && movingHandsDown;
    }

    public Vector3 GetHorizontalDirection()
    {
        switch (flightDirection)
        {
            case FlightDirection.AlwaysUp:
                return Vector3.zero;
            case FlightDirection.ControllerMovementAveraged:
                return XZOnlyNormalized(averageControllerThrust);
            case FlightDirection.ControllerDirectionAveraged:
                return XZOnlyNormalized(averageControllerDirection);
            default:
                return Vector3.zero;
        }
    }

    private Vector3 XZOnlyNormalized(Vector3 v) { return Vector3.Normalize(new Vector3(v.x, 0, v.z)); }

    private void ApplyGravityReduction(bool rotationAdjusted = false) {
        if (rotationAdjusted)
        {
			playerRigidbody.AddForce((1-(diveIntensity/100)*RotationWeight())*(-Physics.gravity * gravityReduction) / 100.0f, ForceMode.Acceleration);
        } else
        {
            playerRigidbody.AddForce(-Physics.gravity * gravityReduction / 100.0f, ForceMode.Acceleration);
        }

    }

    private bool OnGround() { return VRTK_bodyPhysics.OnGround(); }

    private void UpdateFlightState()
    {
        switch (flightState)
        {
            case FlightState.Flying:
                if (IsFlappingUp() || IsFlappingDown())
                { // Stay in flight

                }
                else if (IsGliding())
                {
                    flightState = FlightState.Gliding;
                }
                else if (OnGround())
                {
                    flightState = FlightState.Ground;
                }
                else if (!HandsMoving())
                {
                    flightState = FlightState.Falling;
                }
                break;
            case FlightState.Gliding:
                if (IsFlappingUp())
                {
                    timeGliding = 0;
                    flightState = FlightState.Flying;
                    if (audioManager) audioManager.StopWind();
                }
                else if (OnGround())
                {

                    timeGliding = 0;
                    flightState = FlightState.Ground;
                    if (audioManager) audioManager.StopWind();
                }
                else if (IsGliding())
                {// Stay gliding
                }
                else
                {
                    timeGliding = 0;
                    flightState = FlightState.Falling;
                    if (audioManager) audioManager.StopWind();
                }

                break;
            case FlightState.Falling:
                if (IsFlappingUp())
                {
                    flightState = FlightState.Flying;
                }
                else if (IsGliding())
                {
                    flightState = FlightState.Gliding;
                }
                else if (OnGround())
                {
                    flightState = FlightState.Ground;
                }
                break;
            case FlightState.Ground:
                if (IsFlappingUp())
                {
                    flightState = FlightState.Flying;
                }
                break;
        }
        
        StateIteration();
    }

    private void StateIteration()
    {
        switch (flightState)
        {
            case FlightState.Flying:
                
                playerRigidbody.drag = airDrag;
                if (!MovingHandsDown())
                {
                    Fly(handsMovingUp: true);
                    soundReady = true;
                }
                if (IsFlappingUp())
                {
                    FlightSound();
                    Fly();
                }

                break;
            case FlightState.Gliding:
			VRTK_bodyPhysics.TogglePreventSnapToFloor(false);
                timeGliding += Time.deltaTime;
                if (GlidingLongEnoughForWind() && audioManager)
                {
                    audioManager.PlayWind();
                }
                playerRigidbody.drag = airDrag;
                Glide();
                break;
		case FlightState.Falling:
			playerRigidbody.drag = 0;

			if (!canGlideBackwards && ControllerRotatedBackwards ()) {
			} else {
				Vector3 currentDirection = XZOnlyNormalized(averageControllerDirection);
				AdjustXZDirection(XZOnlyNormalized(currentDirection));
			}


                break;
            case FlightState.Ground:
                FlightOff();
                return;
        }

        VRTK_bodyPhysics.ApplyBodyVelocity(Vector3.zero, applyMomentum: true);
    }

    private bool GlidingLongEnoughForWind()
    {
        return timeGliding > timeBeforeGlideSound;
    }

    private void FlightSound()
    {

        if (soundReady && audioManager)
        {
            audioManager.PlayWingflap();
            soundReady = false;
        }
    }

    private void Glide()
    {
		var rightControllerPos = actualRightController.transform.localPosition;
		var leftControllerPos = actualLeftController.transform.localPosition;
		Vector3 diffY = rightControllerPos - leftControllerPos;
		Vector3 currentDirection = XZOnlyNormalized (averageControllerDirection);
		Vector3 tiltAdjustment;
		if (diffY.y > 0) {
			tiltAdjustment = Quaternion.Euler (0, -tiltIntensity, 0) * currentDirection * Mathf.Abs (diffY.y);
				
		} else {
			tiltAdjustment = Quaternion.Euler (0, tiltIntensity, 0) * currentDirection * Mathf.Abs (diffY.y);
		}
		currentDirection = currentDirection + tiltAdjustment;

		IncreaseHorizontalSpeed(XZOnlyNormalized(currentDirection), noReverseAllowed:!canGlideBackwards);


        ApplyGravityReduction(rotationAdjusted:increaseFallSpeedWithRotation);
    }

    private void Fly(bool handsMovingUp = false)
    {
        if (!handsMovingUp)
        {
            VRTK_bodyPhysics.ToggleOnGround(false);
            IncreaseVerticalSpeed();
        }
		var rightControllerPos = actualRightController.transform.localPosition;
		var leftControllerPos = actualLeftController.transform.localPosition;
		Vector3 diffY = rightControllerPos - leftControllerPos;
		Vector3 currentDirection = GetHorizontalDirection ();
		Vector3 tiltAdjustment;
		if (diffY.y > 0) {
			tiltAdjustment = Quaternion.Euler (0, -tiltIntensity, 0) * currentDirection * Mathf.Abs (diffY.y);

		} else {
			tiltAdjustment = Quaternion.Euler (0, tiltIntensity, 0) * currentDirection * Mathf.Abs (diffY.y);
		}
		currentDirection = currentDirection + tiltAdjustment;


		IncreaseHorizontalSpeed(XZOnlyNormalized(currentDirection), noReverseAllowed: !canGlideBackwards);
        ApplyGravityReduction();
        ResetInFlightTimer();
    }

    private void IncreaseHorizontalSpeed(Vector3 normalizedDirection, bool noReverseAllowed = false)
    {
        float horizontalSpeedNormalizer = 5.0f;
        Vector3 glideVelocity = normalizedDirection * horizontalAcceleration * horizontalSpeedNormalizer * RotationWeight();
        // We want to accelerate in the other direction
        if (Vector3.Dot(normalizedDirection, XZOnlyNormalized(playerRigidbody.velocity)) < 0) {
            float currentSpeed = Vector3.Magnitude(playerRigidbody.velocity);
            playerRigidbody.AddForce(glideVelocity * currentSpeed * brakingSpeed, ForceMode.Acceleration);
        } else if (Vector3.Magnitude(XZOnly(playerRigidbody.velocity)) < maxHorizontalSpeed)
        {
            if (noReverseAllowed && ControllerRotatedBackwards())
            {

            } else
            {
                playerRigidbody.AddForce(glideVelocity, ForceMode.Acceleration);
				AdjustXZDirection(XZOnlyNormalized(normalizedDirection));
            }
        }
            

    }

    private void AdjustXZDirection(Vector3 normalizedXZDirection)
    {
        if (Vector3.Dot(normalizedXZDirection, XZOnlyNormalized(playerRigidbody.velocity)) < 0) return;
        float prevY = playerRigidbody.velocity.y;
        float prevXZMagnitude = Vector3.Magnitude(XZOnly(playerRigidbody.velocity));
        Vector3 newDirection = Vector3.RotateTowards(XZOnlyNormalized(playerRigidbody.velocity), normalizedXZDirection, Mathf.Deg2Rad * maxTurnRate, 0.0f)*prevXZMagnitude;

        newDirection.y = prevY;
        playerRigidbody.AddForce(-playerRigidbody.velocity, ForceMode.VelocityChange);
        playerRigidbody.AddForce(newDirection, ForceMode.VelocityChange);

    }

    private void IncreaseVerticalSpeed()
    {

        float verticalSpeedNormalizer = 15.0f;
        Vector3 verticalVelocity = new Vector3(0, averageControllerThrust.y, 0) * verticalAcceleration * verticalSpeedNormalizer; // Always based on controller thrust
        if (playerRigidbody.velocity.y < maxVerticalSpeed)
        {
            playerRigidbody.AddForce(verticalVelocity, ForceMode.VelocityChange);
        }

    }

    private Vector3 XZOnly(Vector3 v) { return new Vector3(v.x, 0, v.z); }

    private void ResetInFlightTimer() { stillHandsCheck = Time.time; }

    public Vector3 GetCurrentSpeed() { return playerRigidbody ? playerRigidbody.velocity : Vector3.zero; }

    private void FlightOn()
    {
        VRTK_bodyPhysics.TogglePreventSnapToFloor(true);
    }

    private void FlightOff()
    {
        if (playerRigidbody)
        {
            playerRigidbody.drag = 0;
        }
        VRTK_bodyPhysics.TogglePreventSnapToFloor(false);
    }

    private void EvaluateFlightState()
    {
        if (VRTK_bodyPhysics.OnGround()) { flightState = FlightState.Ground; }
        else { flightState = FlightState.Falling; }
    }

    private void EngageButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        EvaluateFlightState();
        engageActive = true;
    }
    
    private void EngageButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (audioManager) audioManager.StopWind();
		EvaluateFlightState ();
        engageActive = false;
    }

    private void SetControllerListeners(GameObject controller, bool controllerState, ref bool subscribedState, bool forceDisabled = false)
    {
        if (controller)
        {
            bool toggleState = (forceDisabled ? false : controllerState);
            ToggleControllerListeners(controller, toggleState, ref subscribedState);
        }
    }

    private void ToggleControllerListeners(GameObject controller, bool toggle, ref bool subscribed)
    {
        var controllerEvent = controller.GetComponent<VRTK_ControllerEvents>();
        if (controllerEvent)
        {
            //If engage button has changed, then unsubscribe the previous engage button from the events
            if ((engageButton != previousEngageButton) && subscribed)
            {
                controllerEvent.UnsubscribeToButtonAliasEvent(previousEngageButton, true, EngageButtonPressed);
                controllerEvent.UnsubscribeToButtonAliasEvent(previousEngageButton, false, EngageButtonReleased);
                subscribed = false;
            }

            if (toggle && !subscribed)
            {
                controllerEvent.SubscribeToButtonAliasEvent(engageButton, true, EngageButtonPressed);
                controllerEvent.SubscribeToButtonAliasEvent(engageButton, false, EngageButtonReleased);
                subscribed = true;
            }
            else if (!toggle && subscribed)
            {
                controllerEvent.UnsubscribeToButtonAliasEvent(engageButton, true, EngageButtonPressed);
                controllerEvent.UnsubscribeToButtonAliasEvent(engageButton, false, EngageButtonReleased);
                subscribed = false;
            }

        }
    }
}
