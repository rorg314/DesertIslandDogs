using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;




///////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////
//////////////////////////DOG BEHAVIOUR AI/////////////////////////////
///////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////

public class DogAI {


    public NavMeshAgent navAgent;
    //Callback to update dog state machine
    public event Action cbUpdateThisDogStateMachine;

    //Queue of upcoming tricks
    Queue<Trick> trickQueue;
    //Queue of upcoming trick parameters (each is an object list)
    Queue<object[]> trickParameterQueue;
    //Sphere collider attached to master game object (for overlapping triggers)
    public SphereCollider dogSphereCollider { get; set; }
    List<object> overlappingSphereCollider;

    //Callback when finished waiting
    public event Action cbFinishedWaiting;


    //OLD Callback to trigger trick when finished waiting - stores action to be called after wait - uses generic object as parameter
    //public event Action<Trick, object[]> cbTrickAfterWait;
    
    
    //Stores the pending trick to be invoked
    Trick pendingTrick { get; set; }
    //Stores the object that is passed as a parameter when finished waiting 
    object[] pendingTrickArguments { get; set; }


    System.Random rnd = DogController.Instance.rnd;

    //Attached dog objects
    Dog dog;
    //Attached animator components
    public Animator dogAnimator;
    public Animator dogIconAnimator;

    //Emote popup canvas
    public GameObject emoteCanvas { get; set; }
    //Object with image component
    public GameObject emoteSpriteObject { get; set; }


    //Dictionaries to hold method action object and parameter types based on method name string
    public Dictionary<string, Action<object[]>> methodNameActionMap;



    public GameObject dog_go { get; set; }
    //////////////////////////////STATE MACHINE BOOLS/////////////////////////////////////////
    public bool wandering { get; set; }
    public bool waiting { get; set; }
    public bool waited { get; set; }
    public bool idle { get; set; }

    /////////////////////////////////////////////////////////////////////////////////////////

    public DogAI(Dog dog) {
        this.dog = dog;
        this.dog_go = dog.dogMasterGO;
        this.navAgent = null;
        this.wandering = false;
        this.waiting = false;
        this.waited = false;
        this.waitTime = 0f;
        this.idle = true;

        this.trickQueue = new Queue<Trick>();
        this.trickParameterQueue = new Queue<object[]>();
        this.pendingTrickArguments = null;
        this.pendingTrick = null;

        //Register callbacks
        cbUpdateThisDogStateMachine += thisDogStateMachineUpdate;

        //Do pending trick action assignment
        doPendingTrick_action = doPendingTrick;

        //Finished waiting callback, will trigger a trick if the queue is not empty
        cbFinishedWaiting += doPendingTrick_action;
    }


    /// Idle dog wandering behaviour ///


    //Vector3 destinationBuffer = new Vector3(0.5f, 2f, 0.5f);
    float waitTime = 0f;

    public void Wander() {
        if (waiting) { Debug.Log("Trying to wander whilst waiting"); return; }

        idle = false;
        if (dogAnimator != null) {
            foreach (AnimatorControllerParameter p in dogAnimator.parameters) {
                if (p.name == "walking") {
                    p.Equals(false);
                }
            }


        }

        if (wandering == false) {
            dog.destination = pickRandomDestination();
            MovementSpeed speed = pickRandomSpeed();
            modifyNavAgentSpeed(dog, speed);


            MoveToDestination(dog.destination);
            wandering = true;
            //Debug.Log("wandering to " + dog.destination.name);
            
        }
        if (wandering) {
            //Play walking animation
            if (dogAnimator != null) {
                foreach (AnimatorControllerParameter p in dogAnimator.parameters) {
                    if (p.name == "walking") {
                        p.Equals(true);
                    }
                }


            }
            //Already wandering, check if reached destination
            //Check if navAgent has no path (i.e when finished pathing - MUST AUTO BRAKE)
            if (dog.dogAI.navAgent.hasPath == false && dog.dogAI.navAgent.pathPending == false) {
                //Should have finished pathing

                Debug.Log(dog.breed + " finished pathing");
                wandering = false;

                object[] parameters = new object[1];

                //Request sniff to be called after 1s wait, using destination game object as trick parameters
                parameters[0] = dog.destination.gameObject;
                waitTime = 0.5f;
                enqueueTrickAfterWait(waitTime, TrickName.Sniff, parameters);

                expressEmotion(Emotion.Curious);
                //Roll dice to also bark
                if (wonDiceRoll(40)) {

                    //Also add bark to trick queue
                    parameters[0] = dog;
                    waitTime = 1f;
                    enqueueTrickAfterWait(waitTime, TrickName.Bark, parameters);
                }

                //Tell dog to perform next (first) trick in queue
                getNextQueuedTrick();



            }

        }
    }

    public Transform pickRandomDestination() {
        List<Transform> allObstaclesInScene = DogController.Instance.allObstaclesInScene;
        int randInt = rnd.Next(0, allObstaclesInScene.Count);

        Transform randomDestination = allObstaclesInScene[randInt];
        return randomDestination;

    }

    public MovementSpeed pickRandomSpeed() {
        int randInt = rnd.Next(0, 3);
        MovementSpeed speed = (MovementSpeed)randInt;
        return speed;
    }


    //Receives raycast from input controller
    public void MoveToClickedPoint(RaycastHit hit) {

        MoveToDestination(hit.transform);

    }

    public void MoveToDestination(Transform dest) {
        navAgent.SetDestination(dest.position);

        Vector3 objectSize = Vector3.Scale(dest.transform.localScale, dest.GetComponentInChildren<MeshRenderer>().bounds.size);
        //Debug.Log(dest.name + "sizes: " + objectSize);
        //Calculate destination buffer based on objectSize and dogDimensions
        //destinationBuffer = ((objectSize + dog.dogDimensions) / 2) * 1.2f;
        //Debug.Log(dest.name + "buffer: " + destinationBuffer);

    }

    
    


    public void checkForNearbyDogs() {
        

    }


    float timer = 0;

    //Object is parameter that will be passed to the invoked function
    public void Wait(float time) {
        
        if (timer < time) {
            //Debug.Log("waiting");
            waiting = true;
            waited = false;
            timer += Time.deltaTime; }
        if (timer >= time) {
            //Debug.Log("waited");
            waited = true;
            timer = 0;
            
            Debug.Log("Finished waiting, invoking finishedWaiting callback ");

            cbFinishedWaiting?.Invoke();

            
        }

    }

    //Looks at all state bools and decides appropriate action
    public void thisDogStateMachineUpdate() {
        
        //Check if currently waiting 
        if (waiting == true && waited == false) { 
            if (waitTime > 0f) { 
                Wait(waitTime); 
            }
            //Return if wait time is zero
            return;
        }
        //If waiting and also finished waiting then set both to false and set as idle
        if (waiting && waited) { waiting = false; waited = false; idle = true; }


        //Check if idle and not waiting or wandering
        if (idle && !waiting && !wandering) { Wander(); }
        //Continue wandering if already
            if(wandering) { Wander(); }



    }
    //Call to update this dog state machine
    public void UpdateAI() {
        
        cbUpdateThisDogStateMachine?.Invoke();
        
    }


    //Method to add tricks to upcoming trick queue
    public void enqueueTrickAfterWait(float time, object trickObject, object[] trickParams) {


        Trick trick = null;
        //Parse generic trickObject into required Trick (type object)
        if (trickObject.GetType() == typeof(Trick)) {
            //Already passed a trick, simply cast as trick
            trick = (Trick)trickObject;
        }
        else if (trickObject.GetType() == typeof(TrickName)) {
            //Passed trick name, set as appropriate Trick
            trick = TrickManager.instance.trickNameMap[trickObject.ToString()];
        }

        float randPercent = rnd.Next(50, 100) * 0.01f;

        
        trick.trickDelay = time * randPercent;
        
        
        List<object> parameterCopyList = new List<object>(trickParams);
        object[] copiedParameters = parameterCopyList.ToArray();
        
        //Add copied trick parameters to queue
        trickParameterQueue.Enqueue(copiedParameters);
        //Add trick to queue
        trickQueue.Enqueue(trick);

        
    }



    //Action that does any passed trick with parameters object[]
    //Action<Trick, object[]> DoTrick_action;
    
    //Dog instantly does specific trick, eg sit, bark, lie down etc - can be passed Trick or TrickName 
    public void DoTrick(object trickObject, object[] trickParams) {
        Trick trick = null;
        TrickName trickName;
        
        if (trickObject.GetType() == typeof(Trick)) {
            //Already passed a trick, simply cast as trick
            trick = (Trick)trickObject;
        }
        else if(trickObject.GetType() == typeof(TrickName)) {
            //Passed trick name, set as appropriate Trick
            trick = TrickManager.instance.trickNameMap[(string)trickObject];
        }

        if (trick != null) {
            //Check if input params have the correct Types 
            for (int i = 0; i < trickParams.Length; i++) {
                if (trickParams[i].GetType() != trick.desiredMethodParameterTypes[i]) {
                    Debug.Log("input: " + trickParams[i].GetType().ToString() + " desired: " + trick.desiredMethodParameterTypes[i]);
                    Debug.LogWarning("Trick input parameter types are incorrect - bailing"); return;
                }
            }
            //Debug.Log("All trick parameters correct, invoking trick " + trick.trickName + " after wait " + time + " seconds");
        }

        //Action object attached to this trick
        Action<object[]> trick_action = trick.trick_action;


        trick_action?.Invoke(trickParams);


    }
    //Called whenever dog tries to do trick
    public void getNextQueuedTrick() {
        
               
        //If no tricks to do simply return
        if(trickQueue.Count == 0) { return; }

        pendingTrick = trickQueue.Dequeue();
        pendingTrickArguments = trickParameterQueue.Dequeue();

        
        if (pendingTrick.trickDelay > 0) {
            //Tell state machine to start waiting, will callback trick after the delay
            Debug.Log("waiting " + pendingTrick.trickDelay + "s to do trick");
            Wait(pendingTrick.trickDelay);
        }
        else {
            //Do the pending trick without waiting
            doPendingTrick();
        }

    }

    public Action doPendingTrick_action;
    //Does the currently pending trick (instantly) - usually called by cbFinishedWaiting
    public void doPendingTrick() {
        //Debug.Log("Doing current pending trick " + pendingTrick.trickName);

        DoTrick(pendingTrick, pendingTrickArguments);

        //If still have tricks on queue, call to get next trick
        if(trickQueue.Count > 0) {
            //Debug.Log("getting next trick from queue");
            getNextQueuedTrick();
        }
    }


    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////DOG EMOTIONS////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////

    //Used to add emotion to current list (stored on dog)
    public void addEmotion(Emotion emote, bool temporary) {
        List<Emotion> emotions = dog.dogEmotions;
        //if emotion is lasting eg hungry, (suprise would be temporary eg)
        if (temporary == false) {
            emotions.Add(emote);
        }
        expressEmotion(emote);
    }

    //Used to remove emotions when appropriate (eg remove hungry once fed)
    public void removeEmotion(Emotion emote) {
        List<Emotion> emotions = dog.dogEmotions;
        emotions.Remove(emote);
    }



    //Used to express dog emoticon popup above head
    public void expressEmotion(Emotion emote) {

        DogUIController.instance.emotePopup(dog, emote);

    }



    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////



    //Attempt to teach dog specific trick
    public void LearnTrick(Trick trick) {

    }

    //Dog uses specified item, 
    public void UseItem(Item item) {


    }

    public bool wonDiceRoll(float percentWinChance) {
        //Debug.Log("rolling");
        int random = rnd.Next(1, 100);

        if(random < percentWinChance) {
            //Debug.Log("won");
            return true;
            
        }
        else {
            //Debug.Log("lost");
                return false; 
        }

    }







    ////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////NAV AGENT SETTINGS/////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////
    float defaultAngularSpeed = 200f;
    float defaultSpeed = 10f;
    float defaultAcc = 6f;
    float[] sizeModifiers = { 1f, 0.9f, 0.8f }; //{small, medium, large}

    //Assign default walking speeds based on size
    public void assignDefaultNavAgentValues(Dog dog) {
        int sizeIndex = (int)dog.size;
        float sizeMod = sizeModifiers[sizeIndex];
        NavMeshAgent navAgent = dog.dogAI.navAgent;
        navAgent.angularSpeed = defaultAngularSpeed * sizeMod;
        navAgent.speed = defaultSpeed * sizeMod;
        navAgent.acceleration = defaultAcc * sizeMod;
    }

    //Used to change walking speed using MovementSpeed enum
    float[] speedMods = { 0.7f, 1f, 1.3f }; //{Slow, Normal, Run}
    public void modifyNavAgentSpeed(Dog dog, MovementSpeed speed) {
        int sizeIndex = (int)dog.size;
        float sizeMod = sizeModifiers[sizeIndex];

        int speedIndex = (int)speed;
        float speedMod = speedMods[speedIndex];
        NavMeshAgent navAgent = dog.dogAI.navAgent;

        navAgent.speed = defaultSpeed * sizeMod * speedMod;

    }
    ////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////


}
