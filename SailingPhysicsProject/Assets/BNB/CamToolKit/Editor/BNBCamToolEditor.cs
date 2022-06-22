using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class BNBCamToolEditor : EditorWindow
{
    private BNBCamToolKit camToolKit;
    private int lookArround, tilt, panning, rotating, zooming;
    private Vector3 flying;
    private GameObject target;
    private bool arrowPressed, enableTool, enableLook, speedUpPressed, speedDownPressed;
    private float speedFactor = 1;
    private Event currEvent;
    private int selecLoc, currLoc;
    private Rect top, middle, bottom, left, right;
    private List<string> tabs = new List<string>();
    private List<Vector3> posList = new List<Vector3>();
    private List<Vector3> rotList = new List<Vector3>();
    private bool preTool, preLook;
    private SceneView scene;


#if UNITY_EDITOR_WIN
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(UInt16 virtualKeyCode);
#endif

    [MenuItem("BNBCreations/CamToolEditor")]
    public static void ShowWindow()
    {
        BNBCamToolEditor window = (BNBCamToolEditor)GetWindow<BNBCamToolEditor>("CamTool");
        window.minSize = new Vector3(300, 0);
        window.Show();
    }
    private void OnEnable()
    {
        #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        #else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        #endif
    }
    private void OnDestroy() {
        enableTool=enableLook=false;
        try{
        camToolKit.EnableTool(enableTool);
        camToolKit.EnableLook(enableTool);
        Repaint();
        }
        catch(NullReferenceException e){}
       
    }
    private void OnGUI()
    {
        currEvent = Event.current;
        TopRect();
        MiddleRect();
        BottomRect();
    }

    private void TopRect()
    {
        top.width = Screen.width;
        top.height = 60;
        top.x = 0;
        top.y = 0;

        GUILayout.BeginArea(top);
        LeftRect();
        RightRect();
        GUILayout.EndArea();
    }
    private void LeftRect()
    {
        left.width = Screen.width / 2;
        left.height = 40;
        left.x = 0;
        left.y = 0;

        GUILayout.BeginArea(left);
        EditorGUI.DrawRect(left, new Color(0.45f, 0.45f, 0.45f, 1));

        GUI.color = Color.yellow;
        //Camera
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Camera");
        camToolKit = (BNBCamToolKit)EditorGUILayout.ObjectField(camToolKit, typeof(BNBCamToolKit), true);
        EditorGUILayout.EndHorizontal();

        //Tool Enable
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Tool");
        enableTool = EditorGUILayout.Toggle(enableTool);
        if (preTool != enableTool)
        {
            preTool = enableTool;
            try
            {
                camToolKit.EnableTool(enableTool);
                if(enableTool)
                    EditorGUIUtility.SetWantsMouseJumping(2);
                else
                    EditorGUIUtility.SetWantsMouseJumping(0);
            }
            catch(NullReferenceException e){
                enableTool=false;
                preTool=enableTool;
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;


        GUILayout.EndArea();
    }
    private void RightRect()
    {
        right.width = Screen.width / 2;
        right.height = 40;
        right.x = Screen.width / 2;
        right.y = 0;

        GUILayout.BeginArea(right);
        EditorGUI.DrawRect(left, new Color(0.45f, 0.45f, 0.45f, 1));

        GUI.color = Color.yellow;
        //Target
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Target");
        target = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();

        //Look To
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Look");
        enableLook = EditorGUILayout.Toggle(enableLook);
        if (preLook != enableLook)
        {
            preLook = enableLook;
            try
            {
                camToolKit.EnableLook(enableLook);
            }
            catch(NullReferenceException e)
            {
                enableLook=false;
                preLook=enableLook;
                Repaint();
            }

        }
        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        GUILayout.EndArea();

    }
    private void MiddleRect()
    {
        middle.width = Screen.width;
        middle.height = 20;
        middle.x = 0;
        middle.y = 40;

        GUILayout.BeginArea(middle);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Copy", GUILayout.Width(Screen.width / 5)))
        {
            CopyLocation();
        }
        try
        {
            string name = EditorGUILayout.TextField(tabs[currLoc], GUILayout.Width(Screen.width * 4 / 15));

            tabs[currLoc] = name;

        }
        catch (NullReferenceException e) { }
        catch (ArgumentOutOfRangeException e) { }


        if (GUILayout.Button("Remove", GUILayout.Width(Screen.width * 3 / 15)))
        {
            RemoveLocation();
        }

        if (GUILayout.Button("Remove All", GUILayout.Width(Screen.width * 5 / 15)))
        {
            tabs.Clear();
            posList.Clear();
            rotList.Clear();
            currLoc = 0;
            selecLoc = 0;
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    private void BottomRect()
    {
        bottom.width = Screen.width;
        bottom.height = 40;
        bottom.x = 0;
        bottom.y = 60;

        GUILayout.BeginArea(bottom);
        SelecGrid();
        GUILayout.EndArea();
    }
    private void SelecGrid()
    {
        try
        {
            selecLoc = GUILayout.SelectionGrid(selecLoc, tabs.ToArray(), 5);
        }
        catch (NullReferenceException e) { }

        if (currLoc != selecLoc)
        {
            currLoc = selecLoc;
            camToolKit.transform.position = posList[currLoc];
            camToolKit.transform.eulerAngles = rotList[currLoc];

        }

    }
    private void CopyLocation()
    {
        try
        {
            if (posList.Count < 10)
            {
                posList.Add(camToolKit.transform.position);
                rotList.Add(camToolKit.transform.eulerAngles);
                tabs.Add("Loc");
                Repaint();
            }

        }
        catch (NullReferenceException e) { }


    }
    private void RemoveLocation()
    {
        try
        {
            tabs.RemoveAt(currLoc);
            posList.RemoveAt(currLoc);
            rotList.RemoveAt(currLoc);
            Repaint();
        }
        catch (ArgumentOutOfRangeException e) { }

    }
    void OnSceneGUI(SceneView sceneView)
    {
        scene=sceneView;
        Handles.BeginGUI();

        GUI.contentColor = Color.blue;
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));

        if (enableTool)
            GUILayout.Label("[CamTool]");
        if (enableLook)
            GUILayout.Label("Looking At");


        GUILayout.EndArea();
        Handles.EndGUI();
    }
    void Update()
    {
        try
        {
            if (EditorWindow.focusedWindow.ToString().EndsWith("GameView)"))
            {
                if (currEvent.keyCode == KeyCode.T)
                {
                    if(camToolKit!=null)
                    {
                        enableTool = !enableTool;
                        preTool = enableTool;
                        Repaint();
                        camToolKit.EnableTool(enableTool);
                        if(enableTool)
                            EditorGUIUtility.SetWantsMouseJumping(2);
                        else
                            EditorGUIUtility.SetWantsMouseJumping(0);
                    }
                }
                else if (currEvent.keyCode == KeyCode.L)
                {
                    if(camToolKit!=null)
                    {
                        enableLook = !enableLook;
                        preLook = enableLook;
                        Repaint();
                        camToolKit.EnableLook(enableLook);
                    }
                   
                }
                else if (currEvent.keyCode == KeyCode.C)
                    CopyLocation();
                else if (currEvent.keyCode == KeyCode.R)
                    RemoveLocation();

                if (enableTool)
                {

                    try
                    {
                        //pan (ctrl+alt)
                        if (currEvent.control && currEvent.alt)
                        {
                            panning++;
                            PanCamera(currEvent.delta);
                        }

                        //tilt  (right+ctrl)
                        else if (currEvent.button == 1 && currEvent.control)
                        {
                            tilt++;
                            Tilt(currEvent.delta);
                        }

                        //rotating (alt)
                        else if (currEvent.alt)
                        {
                            rotating++;
                            RotateCamera(currEvent.delta);
                        }

                        //zoom(shift) 
                        else if (currEvent.shift)
                        {
                            zooming++;
                            ZoomCamera(currEvent.delta);
                        }

                        //FovDecrease(Down)
                        else if (currEvent.keyCode == KeyCode.DownArrow)
                        {
                            camToolKit.FovDecrease();
                        }

                        //FovIncrease(UP)
                        else if (currEvent.keyCode == KeyCode.UpArrow)
                        {
                            camToolKit.FovIncrease();
                        }


                        //Look Arround
                        else if (currEvent.button == 1)
                        {
                            lookArround++;
                            LookArround(currEvent.delta);
                        }
                        else if (currEvent.keyCode == KeyCode.F)
                        {
                            camToolKit.FocusObj(UnityEditor.Selection.activeGameObject.transform.position);
                        }

                        else
                        {
                            if (currEvent.character.Equals('+') && !speedUpPressed)    //Num Pad+   (Speed Up)
                            {
                                speedUpPressed = true;
                                speedFactor *= 2;
                            }
                            else if (speedUpPressed && !currEvent.character.Equals('+'))
                            {
                                speedUpPressed = false;
                            }
                            if (currEvent.character.Equals('-') && !speedDownPressed)    //Num Pad-  (speed Down) 
                            {
                                speedDownPressed = true;
                                speedFactor *= 0.5f;
                            }
                            else if (speedDownPressed && !currEvent.character.Equals('-'))
                            {
                                speedDownPressed = false;
                            }

#if UNITY_EDITOR_WIN
                            WindowsFly();
#else
                            OtherFly();
#endif

                            if (arrowPressed)
                                FlyCamera();


                            lookArround = tilt = panning = rotating = zooming = 0;

                        }

                    }
                    catch (MissingReferenceException e) { }
                    catch (NullReferenceException e) { }
                }
            }
        }
        catch (NullReferenceException e) { }
    }

    private void WindowsFly()
    {
        if (GetAsyncKeyState(0x41) != 0)   //Move Left   (A)      
        {
            flying.x -= 1;
            arrowPressed = true;

        }
        if (GetAsyncKeyState(0x57) != 0)   //Move Forward   (W)      
        {
            flying.z -= 1;                //y means z in camera transform
            arrowPressed = true;

        }
        if (GetAsyncKeyState(0x44) != 0)   //Move Right     (D)
        {
            flying.x += 1;
            arrowPressed = true;

        }
        if (GetAsyncKeyState(0x53) != 0)   //Move Back        (S)
        {
            flying.z += 1;        //y means z in camera transform
            arrowPressed = true;

        }
        if (GetAsyncKeyState(0x45) != 0)     //Move Up(E)
        {
            flying.y += 1;
            arrowPressed = true;
        }
        if (GetAsyncKeyState(0x51) != 0)    //Move Down(Q)
        {
            flying.y -= 1;
            arrowPressed = true;
        }
    }

    private void OtherFly()
    {
        if (currEvent.character.Equals('a') || currEvent.character.Equals('A'))   //Move Left   (A)      
        {
            flying.x -= 1;                //y means z in camera transform
            arrowPressed = true;

        }
        if (currEvent.character.Equals('w') || currEvent.character.Equals('W'))   //Move Forward   (W)      
        {
            flying.z -= 1;                //y means z in camera transform
            arrowPressed = true;

        }
        if (currEvent.character.Equals('d') || currEvent.character.Equals('D'))   //Move Right     (D)
        {
            flying.x += 1;
            arrowPressed = true;

        }
        if (currEvent.character.Equals('s') || currEvent.character.Equals('S'))   //Move Back        (S)
        {
            flying.z += 1;        //y means z in camera transform
            arrowPressed = true;

        }
        if (currEvent.character.Equals('e') || currEvent.character.Equals('E'))     //Move Up(E)
        {
            flying.y += 1;
            arrowPressed = true;
        }
        if (currEvent.character.Equals('q') || currEvent.character.Equals('Q'))    //Move Down(Q)
        {
            flying.y -= 1;
            arrowPressed = true;
        }
    }
    private void RotateCamera(Vector2 dir)
    {
        if (rotating == 1)
        {
            panning = zooming = lookArround = tilt = 0;
        }  
        camToolKit.RotateCam(dir, rotating);
    }

    private void PanCamera(Vector2 dir)
    {
        if (panning == 1)
        {
            rotating = zooming = lookArround = tilt = 0;
        }
        camToolKit.PanCam(dir);
    }
    private void ZoomCamera(Vector2 dir)
    {
        if (zooming == 1)
        {
            panning = rotating = lookArround = tilt = 0;
        }
        camToolKit.ZoomCam(dir);
    }
    private void FlyCamera()
    {
        if (enableLook)
        {
            try
            {
                camToolKit.KeepLooking(target.transform);
            }
            catch (UnassignedReferenceException e) { }

        }
        camToolKit.Fly(flying, speedFactor);
        flying = Vector3.zero;
        arrowPressed = false;
    }
    private void LookArround(Vector2 dir)                //check 
    {
        if (lookArround == 1)
        {
            panning = rotating = zooming = tilt = 0;
        }
        Vector2 currDir=new Vector2(-dir.y,-dir.x);
        camToolKit.LookingArround(currDir);
    }
    private void Tilt(Vector2 dir)
    {
        if (tilt == 1)
        {
            lookArround = 0;
        }
        camToolKit.TiltCamera(dir);
    }

}
