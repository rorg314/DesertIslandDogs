using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines camera movement functions - receives input from InputController 
/// </summary>

public class CameraController : MonoBehaviour {
    
    public static CameraController Instance { get; set; }

    Camera mainCamera;
    public bool isFollowing { get; set; }

    public GameObject target_go { get; set; }

    // Start is called before the first frame update
    private void Awake(){
        if(Instance == null) { Instance = this; } 
        else { Debug.LogError("More than one cameracontroller instance"); }
        
        isFollowing = false;
        mainCamera = Camera.main;

    }


    float panSpeed = 5f;
    Vector3 localTranslate;
    public void PanCamera(Vector3 dir) {

        dir = Quaternion.Euler(currentAngles) * dir;
        localTranslate = Vector3.ProjectOnPlane(dir, Vector3.up);

        //Pan camera using rotation adjusted transform
        mainCamera.transform.Translate(localTranslate * panSpeed * Time.deltaTime * 50, Space.World);
        

    }

    float minY = 0.5f;
    float maxY = 20f;
    //Base camera rotation angle
    Vector3 baseAngles = new Vector3(40f, 0f, 0f);
    Vector3 currentAngles = new Vector3(0f, 0f, 0f);
    //Height below which camera starts rotating
    float turnHeight = 5f;
    //Max amount the camera will turn to
    float maxTurnAngle = 20f;

    public void ZoomCamera(float zoom) {
        
        //Global pos of camera (global zoom)
        Vector3 pos = mainCamera.transform.position;
        pos.y -= zoom;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        if(pos.y < turnHeight) {

            float percent = 1 - pos.y / turnHeight;
            float turnAmount = maxTurnAngle * percent;
            currentAngles.x = baseAngles.x - turnAmount;
            
        }
        else { currentAngles.x = baseAngles.x; }
        
        mainCamera.transform.position = pos;
        mainCamera.transform.rotation = Quaternion.Euler(currentAngles);
    }
    
    //Rotate camera 1-Q-anticlockwise 2-E-clockwise
    Vector3 q = new Vector3(0f, -1f, 0f);
    Vector3 e = new Vector3(0f, 1f, 0f);
    float turnSpeed = 5f;
    public void TurnCamera(int QEindex) {
        currentAngles = mainCamera.transform.rotation.eulerAngles;
        if(QEindex == 0) { mainCamera.transform.rotation = Quaternion.Euler(currentAngles); return; }
        if(QEindex == 1) { currentAngles += q * turnSpeed * Time.deltaTime * 50; }
        if (QEindex == 2) { currentAngles += e * turnSpeed * Time.deltaTime * 50; }
        mainCamera.transform.rotation = Quaternion.Euler(currentAngles);
    }

    //Start following focused object -- stores previous camera orientation 
    Transform previousCameraTransform;
    Vector3 initialCameraHeight;
    float radialOffset = 10;

    public void activateFollowCamera(Dog target) {
        InputController.Instance.followMode = true;
        InputController.Instance.cameraMode = false;
        
        
        previousCameraTransform = mainCamera.transform;
        target_go = target.dogMasterGO;
        initialCameraHeight = new Vector3(0, previousCameraTransform.position.y, 0);

        //Set intiial following camera position to be on radial offset circle, based on current camera look direction towards target
        Vector3 initialLookDirection = Vector3.ProjectOnPlane((target_go.transform.position - mainCamera.transform.position), Vector3.up).normalized;
        mainCamera.transform.position = target_go.transform.position;
        
        mainCamera.transform.Translate(-initialLookDirection*radialOffset, Space.World);
        
        mainCamera.transform.position += initialCameraHeight/4;

        isFollowing = true;
    }
    Vector3 targetPosition;

    public float followZoom = 0f;
    public void followTargetPosition() {
        targetPosition = target_go.transform.position;

        Transform currentCameraTransform = mainCamera.transform;

        //Move the camera position towards the target position
        Vector3 cameraMoveDirection = (targetPosition - currentCameraTransform.position).normalized;
        float distance = Vector3.Distance(targetPosition, currentCameraTransform.position);
        float cameraMoveSpeed = 3f;

        cameraMoveDirection = Vector3.ProjectOnPlane(cameraMoveDirection, Vector3.up);

        if(distance > radialOffset) {
            
            mainCamera.transform.Translate(cameraMoveDirection * distance * cameraMoveSpeed * Time.deltaTime, Space.World);
            
        }
        
         

        handleFollowOrbit(QEindex);

        handleFollowPan(panDirection);

        if (followZoom != 0) {

            handleFollowZoom(followZoom);
            return;
        }

        mainCamera.transform.LookAt(targetPosition, Vector3.up);

    }


    //Decrease spherical radial distance to target (translate directly towards the target)
    float followZoomSpeed = 100f;
    public void handleFollowZoom(float zoom) {

        Vector3 pos = Camera.main.transform.position;
        float distance = Vector3.Distance(targetPosition, pos);
        Vector3 dir = Vector3.Normalize(targetPosition - pos);

        if (distance > 1f) {
            mainCamera.transform.Translate( zoom * dir * distance * followZoomSpeed * Time.deltaTime, Space.World);
        }
        else {
            mainCamera.transform.Translate(- Mathf.Abs(zoom) * dir * distance * followZoomSpeed * Time.deltaTime, Space.World);
        }

    }

    //Translate the camera along the local x axis, then look back at target to approximate circular orbit 
    float orbitAmount = 10f;
    public int QEindex = 0;
    public void handleFollowOrbit(int QEindex) {
        int mod = 0;
        if(QEindex == 0) { return; }
        //Check if q (1) or e (2) pressed, rotate right for e left for q
        if(QEindex == 1) { mod = -1; }
        if(QEindex == 2) { mod = +1; }

        //Translate small amount left or right in local camera space
        mainCamera.transform.Translate(Vector3.right * mod * orbitAmount * Time.deltaTime, Space.Self);
        //Look back at the target
        mainCamera.transform.LookAt(targetPosition, Vector3.up);
    }


    //Pan the camera by moving in plane orthogonal to view plane (eg direction from swipe direction on screen)
    //Orbit the camera so this panning effectively slides camera around sphere (camera always pointing at target)
    float followPanAmount = 15f;
    public Vector3 panDirection = Vector3.zero; 
    public void handleFollowPan(Vector3 dir) {

        if (dir != Vector3.zero) {
            //Need to convert x-z direction vector into xy (for local space transformation of camera)
            Vector3 localDir = new Vector3(dir.x, dir.z, 0);

            //Scale pan amount based on distance to target
            float targetDistance = Vector3.Distance(mainCamera.transform.position, targetPosition);


            mainCamera.transform.Translate(localDir * targetDistance * followPanAmount * Time.deltaTime, Space.Self);


            mainCamera.transform.LookAt(targetPosition, Vector3.up);

        }
    }




}
