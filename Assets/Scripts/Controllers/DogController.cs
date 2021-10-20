using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

/// <summary>
/// Controls dog movement/animation - receives commands from DogAI
/// </summary>


//Script will be attached to a dog
public class DogController : MonoBehaviour {

    public static DogController Instance;

    ///////////////////////DICTIONARIES & LISTS/////////////////////////

    //Map of breed -> prefab object
    Dictionary<Breed, UnityEngine.Object> breedPrefabMap;

    //Map of dog -> existing dog game object
    Dictionary<Dog, GameObject> dogMasterGameObjectsMap;
    //inverse map of gameObject to dog
    public Dictionary<GameObject, Dog> masterGameObjectDogMap { get; protected set; }

    Dictionary<Dog, GameObject> dogMeshGameObjectMap;
    public Dictionary<GameObject, Dog> meshDogMap;

    public Dictionary<Dog, SphereCollider> dogSphereColliderMap;

    //Map of breed name -> dog object
    Dictionary<Breed, Dog> breedDogMap;

    public List<Dog> allDogsInScene { get; set; }

    public List<Transform> allObstaclesInScene;
    public GameObject sceneObstacleHolder;
    public GameObject spawnPointsHolder;


    List<Transform> dogSpawnPoints;

    //Vector3 offset = new Vector3(1f, 0, 1f);

    //Random num generator (used for dog AI - need single instance to keep numbers from being the same for each dogAI instance)
    public System.Random rnd { get; protected set; } 
        
       

    ///////////////////////////////////////////////////////////////////


    private void Start() {
        if(Instance != null) { Debug.LogWarning("Already have dogcontroller instance"); }
        Instance = this;
        
        
        rnd = new System.Random();
        dogMasterGameObjectsMap = new Dictionary<Dog, GameObject>();
        masterGameObjectDogMap = new Dictionary<GameObject, Dog>();
        dogMeshGameObjectMap = new Dictionary<Dog, GameObject>();
        meshDogMap = new Dictionary<GameObject, Dog>();
        dogSphereColliderMap = new Dictionary<Dog, SphereCollider>();

        breedDogMap = new Dictionary<Breed, Dog>();
        allDogsInScene = new List<Dog>();
        dogSpawnPoints = new List<Transform>();
        allObstaclesInScene = new List<Transform>();
        //testSpawnPoints = new List<GameObject>();
        LoadDogPrefabs();
        
        Debug.Log("loaded prefabs");

        registerSceneObstacles(sceneObstacleHolder);
        registerSpawnPoints(spawnPointsHolder);

        

        SpawnDogBreed(Breed.Pomeranian, dogSpawnPoints[1]);

        

        SpawnDogBreed(Breed.Labrador, dogSpawnPoints[1]);
        SpawnDogBreed(Breed.animated, dogSpawnPoints[1]);
        

        //CameraController.Instance.activateFollowCamera(breedDogMap[Breed.Pomeranian]);
    }

    //Updates all dogs every frame
    void Update() {
        foreach (Dog d in allDogsInScene) {
            d.Update(Time.deltaTime);

        }
    }



    //Locate and load all dog prefabs
    void LoadDogPrefabs() {

        breedPrefabMap = new Dictionary<Breed, UnityEngine.Object>();
        UnityEngine.Object[] dogPrefabObjects = Resources.LoadAll("DogPrefabs");
        
        foreach(UnityEngine.Object prefab in dogPrefabObjects) {
            //Debug.Log(prefab);
            string prefabName = prefab.ToString();
            //Debug.Log(prefabName);

            //Get dog name based on prefab name string DOGNAME_ + (UnityEngine.Gameobject)
            //Search up to _ character
            int pos = prefabName.IndexOf("_");
            string prefabDogName = prefabName.Substring(0, pos);

            //Parse breed from prefabDogName string
            Breed breed = (Breed)Enum.Parse(typeof(Breed), prefabDogName);
            //Debug.Log(breed);
            //Add to breed -> prefab map
            breedPrefabMap.Add(breed, prefab);

        }
    }
    //Test function to spawn a dog based on breed at specific location, eg when entering new zones
    public void SpawnDogBreed(Breed breed, Transform location) {
        Dog dog = new Dog(breed);
        OnDogCreated(dog, location);

    }
    //Spawn existing dog based on dog data, ie for returning to an old zone
    public void RespawnDog(Dog dog) {


    }

    //On dog created - add dog to appropriate dictionaries, instantiate prefab etc
    public void OnDogCreated(Dog dog, Transform spawnPoint) {
        
        GameObject dog_master_go = (GameObject)Instantiate(breedPrefabMap[dog.getBreed()], spawnPoint);
        //Assign dog_go to dog and AI classes 
        dog.dogMasterGO = dog_master_go;
        dog.dogAI.dog_go = dog_master_go;
        dogMasterGameObjectsMap.Add(dog, dog_master_go);
        masterGameObjectDogMap.Add(dog_master_go, dog);
        breedDogMap.Add(dog.breed, dog);

        allDogsInScene.Add(dog);


        //Add nav mesh agent component and assign values
        dog.dogAI.navAgent = dog_master_go.AddComponent<NavMeshAgent>();
        //dog.dogAI.navAgent.angularSpeed = 200;
        dog.dogAI.assignDefaultNavAgentValues(dog);



        DogUIController.instance.registerEmotePoint(dog);

        //Add animator components and register to animator controller
        dog.dogAI.dogAnimator = dog.dogMasterGO.GetComponent<Animator>();
        dog.dogAI.dogIconAnimator = dog.emotePoint.GetComponentInChildren<Animator>(true);
        AnimationController.instance.registerDogAnimatorComponents(dog);


        


        //Register callback for dog changed
        dog.cbOnDogChanged += OnDogChanged;
        OnDogChanged(dog);

        //Trigger wander
        //dog.dogAI.Wander();

    }
    
    //Called whenever a dog is changed/updated
    public void OnDogChanged(Dog dog) {
        
        if(dogMasterGameObjectsMap.ContainsKey(dog) == false) { Debug.LogWarning("Changing dog not present!"); }
        


    }

    //On dog removed callback - clean up old associations/models/dictionaries
    public void RemoveDogFromScene(Dog dog) {
        if (dogMasterGameObjectsMap.ContainsKey(dog)) {
            GameObject dog_go = dogMasterGameObjectsMap[dog];
            
            dogMasterGameObjectsMap.Remove(dog);

            allDogsInScene.Remove(dog);

            Destroy(dog_go);
        }
    }

    public void processDogPrefabComponents(Dog dog) {
        GameObject dogMasterGO = dog.dogMasterGO;
        Transform[] children = dogMasterGO.GetComponentsInChildren<Transform>();
        foreach(Transform child in children) {
            if(child.gameObject.name == "Mesh") {
                dogMeshGameObjectMap.Add(dog, child.gameObject);
                meshDogMap.Add(child.gameObject, dog);
            }

        }

        dog.dogAI.dogSphereCollider = dogMasterGO.GetComponent<SphereCollider>();
        dogSphereColliderMap.Add(dog, dog.dogAI.dogSphereCollider);

    }



    //////////////////////////////REGISTER SCENE COMPONENTS////////////////////////////////////////

    public void registerSceneObstacles(GameObject obstacleHolder) {

        MeshRenderer[] allObstacles = obstacleHolder.GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer obs in allObstacles ){
            //Add obstacle to list (omit obstacle holder parent empty object)
            if (obs.name != "Obstacles") {
                allObstaclesInScene.Add(obs.gameObject.transform);
            }
            //print(obs.name);
        }


    }

    public void registerSpawnPoints(GameObject spawnPointsHolder) {

        Transform[] allSpawns = spawnPointsHolder.GetComponentsInChildren<Transform>();

        foreach (Transform spawn in allSpawns) {
            if(spawn.name != "SpawnPointsHolder")
            dogSpawnPoints.Add(spawn.transform);
        }
    }

    


    /////////////////////////////////////////////////////////////////////


    //////////////////////////////////////////////////////////////////////


    //////////////////////////////////////////////////////////////////////


    /////////////////////////////////////////////////////////////////////


    /////////////////////////////////////////////////////////////////////














}
