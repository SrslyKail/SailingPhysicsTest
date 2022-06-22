using System;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;

[ExecuteAlways]
public class BNBCamToolKit : MonoBehaviour
{
    private Camera cam;
    private float rotateSensitivity=0.1f;       
    private float zoomSensitivity=0.1f;      
    private float panningSensitivity = 0.1f;
    private float flyMoveSensitivity = 0.01f;
    private float lookArroundSensitivity=0.1f;
    private float tiltSensitivity=0.1f;
    private Vector3 currentPos,currentRot;
    private Vector3 rotatingPivot= new Vector3();
    private float val;
    private bool toolEnable, lookEnable;
 
  
    private void OnEnable()
    {
        cam = gameObject.GetComponent<Camera>(); 
    }
    public void RotateCam(Vector2 dir,int rotated)
    {
        if (rotated == 1)
        {
            currentPos = transform.position;
            float difference = Vector3.Distance(rotatingPivot, transform.position);
            rotatingPivot = transform.position + transform.forward * difference;
        }
        transform.position = rotatingPivot;
        transform.Rotate(new Vector3(1, 0, 0), dir.y * rotateSensitivity);
        transform.Rotate(new Vector3(0, 1, 0), dir.x * rotateSensitivity, Space.World);

        if (rotated == 1)
        {
            val = transform.InverseTransformPoint(currentPos).z;
        }

        transform.Translate(new Vector3(0, 0, val));
    }
  
    public void PanCam(Vector2 dir)
    {
        transform.Translate(new Vector3(1, 0, 0)* -dir.x * panningSensitivity);
        transform.Translate(new Vector3(0, 1, 0)* dir.y * panningSensitivity);
    }
    public void ZoomCam(Vector3 dir)
    {
        currentPos = transform.position;
        currentPos.x += dir.y * transform.forward.x * zoomSensitivity;
        currentPos.y += dir.y * transform.forward.y *zoomSensitivity;
        currentPos.z += dir.y * transform.forward.z* zoomSensitivity;

        transform.position = currentPos;
    }
    public void Fly(Vector3 flying,float speedFactor)          //flying.y is transform.z;
    {
        transform.Translate(transform.right * flying.x * flyMoveSensitivity*speedFactor, Space.World);
        transform.Translate(transform.forward * -flying.z * flyMoveSensitivity*speedFactor, Space.World);
        transform.Translate(transform.up * flying.y * flyMoveSensitivity*speedFactor, Space.World);
    }
    public void KeepLooking(Transform target)
    {
        Vector3 lookAngles = Quaternion.LookRotation(target.position - transform.position, Vector3.up).eulerAngles;
        transform.eulerAngles = new Vector3(lookAngles.x,lookAngles.y, transform.eulerAngles.z);
    }
    public void TiltCamera(Vector2 dir)
    {
        currentRot = transform.eulerAngles;
        currentRot.z += dir.x * tiltSensitivity;
        transform.eulerAngles = currentRot;
    }
 
    public void FovIncrease()
    {
        cam.fieldOfView++;
    }
    public void FovDecrease()
    {
        cam.fieldOfView--;
    }
    public void LookingArround(Vector3 dir)
    {
        var currEuler = transform.eulerAngles;
        currEuler -= dir * lookArroundSensitivity;
        transform.eulerAngles= currEuler;
        
    }
    public void FocusObj(Vector3 pos)
    {
        rotatingPivot = pos;
        Vector3 lookAngles = Quaternion.LookRotation(pos - transform.position, Vector3.up).eulerAngles;
        transform.eulerAngles = new Vector3(lookAngles.x, lookAngles.y, transform.eulerAngles.z);

        currentPos = transform.position;
        float dist = Vector3.Distance(transform.position, pos)-5f;
        currentPos.x += dist * transform.forward.x ;
        currentPos.y += dist * transform.forward.y ;
        currentPos.z += dist * transform.forward.z ;

        transform.position = currentPos;

    }
    public void EnableTool(bool val)
    {
        toolEnable = val;
    }
    public void EnableLook(bool val)
    {
        lookEnable = val;
    }
    private void OnGUI()
    {
        GUI.contentColor = Color.blue;

        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        if(toolEnable)
            GUILayout.Label("[CamTool]");
        if(lookEnable)
            GUILayout.Label("Looking At");

        GUILayout.EndArea();
    }
}
