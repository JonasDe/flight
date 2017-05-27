using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class VirtualArrow : MonoBehaviour {

    // Use this for initialization
    [Tooltip("Forward Start color")]
    public Color fc1 = Color.green;

    [Tooltip("Forward End color")]
    public Color fc2 = Color.blue;

    [Tooltip("Backward Start color")]
    public Color bc1 = Color.red;

    [Tooltip("Backward End color")]
    public Color bc2 = Color.yellow;
    [Tooltip("Downward Start color")]
    public Color dc1 = Color.black;
    [Tooltip("Downward end color")]
    public Color dc2 = Color.white;

    public FlightHandlerPhys flightHandler;
    public float width = 0.15f;
    public int bodyPoints = 20;
    public int bodyLength = 20;
    public float bodyStartWidth;
    public float bodyEndWidth;
    public float headStartWidth;
    public float headEndWidth;
    public float heightOffset = 0.4f;
    public float forwardOffset = 1.0f;
    private LineRenderer body;
    private LineRenderer leftHead;
    private LineRenderer rightHead;
    private Gradient forwardGradient;
    private Gradient backwardGradient;
    private Transform headset;
    private Gradient downwardGradient;
    private int headPoints = 2;
    public enum GradientType {
        Backwards,
        Forwards,
        Downwards

    }

    public void SetGradient(GradientType grad){
        if (grad == GradientType.Backwards) {
            body.colorGradient = backwardGradient;
            leftHead.startColor = leftHead.endColor = rightHead.startColor = rightHead.endColor = bc2;

        } else if(grad == GradientType.Forwards){
            body.colorGradient = forwardGradient;
            leftHead.startColor = leftHead.endColor = rightHead.startColor = rightHead.endColor = fc2;
        }
        else
        {
            body.colorGradient = downwardGradient;
            leftHead.startColor = leftHead.endColor = rightHead.startColor = rightHead.endColor = dc2;
        }

    }


    public void FixedUpdate()
    {
		if (!headset) headset = VRTK_DeviceFinder.HeadsetTransform();
        float yPosition = headset.position.y - heightOffset;
        if (flightHandler.GetFlightState() == FlightHandlerPhys.FlightState.Gliding || flightHandler.GetFlightState() == FlightHandlerPhys.FlightState.Flying)
        {

            EnableArrow();
            Vector3 start = headset.position + Vector3.Normalize(new Vector3(headset.forward.x, 0, headset.forward.z))*forwardOffset;
            start.y = yPosition;
            Vector3 direction = flightHandler.WeightedForwardDirection();
            if (flightHandler.ControllerRotatedBackwards())
            {
                SetGradient(GradientType.Backwards);

            }
            else
            {
                SetGradient(GradientType.Forwards);

            }

            direction.y = 0;

            DrawLine(start, direction, body, bodyPoints);
            Vector3 leftHead = Quaternion.Euler(0, -45, 0) * (-direction);
            Vector3 rightHead = Quaternion.Euler(0, 45, 0) * (-direction);
            Vector3 end = start + direction;

            DrawLine(end, leftHead, this.leftHead, headPoints);
            DrawLine(end, rightHead, this.rightHead, headPoints);


        }
        if (flightHandler.GetFlightState() == FlightHandlerPhys.FlightState.Falling)
        {
            EnableArrow();
            float downY = headset.position.y;
            Vector3 start = headset.position + Vector3.Normalize(new Vector3(headset.forward.x, 0, headset.forward.z));
            start.y = downY;
            Vector3 direction =  new Vector3(0, -0.5f, 0); //flightHandler.WeightedForwardDirection();
            SetGradient(GradientType.Downwards);

            DrawLine(start, direction, body, bodyPoints);
            Vector3 leftHead = Quaternion.Euler(-45,0 , -45) * (-direction);
            Vector3 rightHead = Quaternion.Euler(45, 0, 45) * (-direction);
            Vector3 end = start + direction;
            DrawLine(end, leftHead, this.leftHead, 2);
            DrawLine(end, rightHead, this.rightHead, 2);




        }
        else if( flightHandler.GetFlightState() == FlightHandlerPhys.FlightState.Ground)

        {
        DisableArrow();
        }
    }
    private void EnableArrow()
    {

        body.positionCount = bodyPoints;
        leftHead.positionCount = headPoints;
        rightHead.positionCount = headPoints;
    }
    private void DisableArrow()
    {

        body.positionCount = 0;
        leftHead.positionCount = 0;
        rightHead.positionCount = 0;
    }

    private void SetArrowHeadAttributes(LineRenderer line)
    {
        line.material = new Material(Shader.Find("Particles/Additive"));
        line.startColor = fc2;
        line.endColor = fc2;
        line.positionCount = 2;
        line.startWidth = headStartWidth;
        line.endWidth = headEndWidth;
    }
    void Start()
    {
        body = this.transform.FindChild("Body").gameObject.AddComponent<LineRenderer>();// gameObjec.AddComponent<LineRenderer>();
        leftHead = this.transform.FindChild("LeftHead").gameObject.AddComponent<LineRenderer>();// gameObjec.AddComponent<LineRenderer>();
        rightHead = this.transform.FindChild("RightHead").gameObject.AddComponent<LineRenderer>();// gameObjec.AddComponent<LineRenderer>();

        body.material = new Material(Shader.Find("Particles/Additive"));
        SetArrowHeadAttributes(leftHead);
        SetArrowHeadAttributes(rightHead);
        body.positionCount = bodyLength;
        headset = VRTK_DeviceFinder.HeadsetTransform();
        body.endWidth = bodyEndWidth;
        body.startWidth = bodyStartWidth;
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        forwardGradient  = new Gradient();
        forwardGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(fc1, 0.0f), new GradientColorKey(fc2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1.0f), new GradientAlphaKey(alpha, 0.0f) }
            );

        downwardGradient = new Gradient();
        downwardGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(dc1, 0.0f), new GradientColorKey(dc2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1.0f), new GradientAlphaKey(alpha, 0.0f) }
            );

        backwardGradient = new Gradient();
        backwardGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(bc1, 0.0f), new GradientColorKey(bc2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1.0f), new GradientAlphaKey(alpha, 0.0f) }
            );

        body.colorGradient = forwardGradient;
    }
 

    public void DrawLine(Vector3 start, Vector3 direction, LineRenderer lr, int points)
    {
        var t = Time.time;
        for(int i = 0; i < points; i++)
        {
        lr.SetPosition(i, start+(((float)i)/((float)points))*direction);
        }
    }

}
