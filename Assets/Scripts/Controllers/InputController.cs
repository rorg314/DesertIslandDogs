using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles inputs - sends input to other controllers eg camera controller
/// </summary>
public class InputController : MonoBehaviour {
    public static InputController Instance { get; protected set; }


    //Defines whether touch or keyboard input
    bool touchInput;
    //Defines if in free camera mode
    public bool cameraMode { get; set; }
    //Defines if focusing on dog/object
    public bool followMode { get; set; }

    Vector3 panDirection;
    float zoom;

    void Start() {
        Instance = this;

        touchInput = false;
        cameraMode = true;
        rotating = false;
        panning = false;
        zooming = false;
        panDirection = Vector3.zero;

    }

    void Update() {
        //Process KBM input
        if (touchInput == false) {

            UpdateCamera();

            processClicks();
        }

        //Process touch input
        if (touchInput == true) {





        }

    }

    bool panning;

    private Vector3 processWASD() {
        //Basis directions in camera plane w/s z-axis, a/d x-axis
        Vector3 w = new Vector3(0, 0, 1);
        Vector3 a = new Vector3(-1, 0, 0);
        Vector3 s = new Vector3(0, 0, -1);
        Vector3 d = new Vector3(1, 0, 0);

        //Process WASD input - convert to panDirection -> send to CameraController
        //Add direction when key down
        if (Input.GetKey("w")) { panning = true; panDirection += w; }
        if (Input.GetKey("a")) { panning = true; panDirection += a; }
        if (Input.GetKey("s")) { panning = true; panDirection += s; }
        if (Input.GetKey("d")) { panning = true; panDirection += d; }
        //Subtract direction when key up
        if (Input.GetKeyUp("w") || Input.GetKeyUp("s")) { panning = false; panDirection.z = 0; }
        if (Input.GetKeyUp("a") || Input.GetKeyUp("d")) { panning = false; panDirection.x = 0; }


        //Get overall pan direction
        panDirection = panDirection.normalized * 0.1f;

        return panDirection;

    }

    //For zooming with scroll wheel
    bool zooming;
    private float processScroll() {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float scrollSpeed = 5f;


        float zoom = scroll * scrollSpeed * Time.deltaTime * 50;
        //zoom = Mathf.Clamp(zoom, minY, maxY);

        if (scroll == 0) { zooming = false; return 0f; }
        else {
            zooming = true;
            return zoom;
        }
    }

    //For rotating camera E-clockwise Q-anticlockwise
    //Return 0 if off, 1- Q , 2- E
    bool rotating;
    int QEindex;
    private int processQE() {
        if (Input.GetKeyDown("q")) { rotating = true; return 1; }
        if (Input.GetKeyDown("e")) { rotating = true; return 2; }

        if (Input.GetKeyUp("q") || Input.GetKeyUp("e")) { rotating = false; return 0; }

        if (rotating) { return QEindex; }
        return 0;
    }

    public void processClicks() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                Vector3 hit_point = hit.point;
                //Check if hit any colliders
                if (hit.collider != null) {
                    Transform[] hitParentTransforms = hit.collider.GetComponentsInParent<Transform>();
                    
                    foreach (Transform parent in hitParentTransforms) {
                        if (DogController.Instance.masterGameObjectDogMap.ContainsKey(parent.gameObject)) {
                            //Hit a dog
                            Dog hitDog = DogController.Instance.masterGameObjectDogMap[parent.gameObject];
                            //Debug.Log(hitDog.breed);
                            CameraController.Instance.activateFollowCamera(hitDog);
                        }

                        
                    }
                    
                    //GameObject hit_go = hit_transform.gameObject;
                    //Debug.Log(hit_go);
                    //Check if hit any dogs
                    
                }
                
            }

        }
    }
    //Testing - will replace with more generic click processiung later, only for moving dog rn 
    public void processClicksForDogMove(Dog dog) {

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                //Call move to clicked point in DOgAI
                foreach (Dog d in DogController.Instance.allDogsInScene) {
                    dog.dogAI.MoveToClickedPoint(hit);
                }
            }
        }


    }

    public void UpdateCamera() {


        //Default is camera mode
        if (cameraMode && !followMode) {

            //Process WASD for panDirection
            panDirection = processWASD();
            //Send to CameraController
            if (panning) {
                CameraController.Instance.PanCamera(panDirection);
            }

            //Process scroll for zoom
            zoom = processScroll();
            //Send to controller
            if (zooming) {
                CameraController.Instance.ZoomCamera(zoom);
            }

            //Process QE input for rotation (returns 0 if off, 1-Q, 2-E)
            QEindex = processQE();
            if (rotating) {
                CameraController.Instance.TurnCamera(QEindex);
            }



        }

        //Selected some object - sets mode to focus mode

        if (followMode && !cameraMode) {

            


            zoom = processScroll();
            if (zooming) {
                CameraController.Instance.followZoom = zoom;
            }
            if (!zooming) {
                CameraController.Instance.followZoom = 0f;
            }

            QEindex = processQE();
            if (rotating) {
                CameraController.Instance.QEindex = QEindex;
            }
            if (!rotating) {
                CameraController.Instance.QEindex = 0;
            }

            panDirection = processWASD();

            if (panning) {
                CameraController.Instance.panDirection = panDirection;
            }
            if (!panning) {
                CameraController.Instance.panDirection = Vector3.zero;
            }

            CameraController.Instance.followTargetPosition();

        }


    }


}
