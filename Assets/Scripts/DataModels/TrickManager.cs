using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Defines all dog tricks
/// </summary>

//Enum of all possible tricks/actions
public enum TrickName { Bark, Sniff, Sit, Lie, Roll, Stand, Fetch, Twirl, Paw, Highfive }


//Class managing all dog tricks
public class TrickManager : MonoBehaviour {

    public static TrickManager instance;
    
    //List of all possible tricks
    List<Trick> allTricksList;

    //Dictionary of tricks based on name string
    public Dictionary<string, Trick> trickNameMap;
    
    //Dictionaries to hold method action object and parameter types based on method name string
    public Dictionary<string, Action<object[]>> objectBasedActionsMap;

    private void Start() {
        instance = this;
        defineActionDelegates();
        setupTricks();

    }


    public void setupTricks() {
        trickNameMap = new Dictionary<string, Trick>();
        //String array of all tricknames (for counting etc)
        string[] allTricks = Enum.GetNames(typeof(TrickName));
        //List of all tricks
        List<Trick> allTricksList = new List<Trick>();

        for (int i = 0; i < allTricks.Length; i++) {
            //Create new trick object for each trick
            Trick trick = new Trick((TrickName)i);

            allTricksList.Add(trick);
            trickNameMap.Add(allTricks[i], trick);
            
        }

    }


    ////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////ALL TRICK METHODS//////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// All tricks receive object[] array as input, first object must always be dog performing the trick
    /// </summary>
    



    /////////////////////////////////////////////PARAMATERLESS//////////////////////////////////
    //Bark doesn't need object, ignore passed object (use dog as passed object for example or just null)
    public void Bark(object[] input) {
        Dog dog = (Dog)input[0];
        CodeMonkey.CMDebug.TextPopup("BARK", dog.dogMasterGO.transform.position);
        Debug.Log(dog.breed + " barked");

    }

    /////////////////////////////////////TARGET-OBJECT-BASED TRICKS ////////////////////////////

    //Sniff the target object 
    public void Sniff(object[] input) {


        //Inputs: targetObject
        
        GameObject target_go = (GameObject)input[0];
        
        Debug.Log("Sniffed " + target_go.name);
        CodeMonkey.CMDebug.TextPopup("Sniff", target_go.transform.position);

        

    }



    ////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////ALL TRICK ACTIONS//////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Assigned before runtime, at runtime new trick objects are created with correct actions assigned
    /// </summary>
    public void defineActionDelegates() {
        //Dictionary of all method objects based on name string
        objectBasedActionsMap = new Dictionary<string, Action<object[]>>();
        

        //All method action objects (all take generic object[] as input - method itself should perform appropriate cast
        Action<object[]> sniff_action = Sniff;
        objectBasedActionsMap.Add("Sniff", sniff_action);

        Action<object[]> bark_action = Bark;
        objectBasedActionsMap.Add("Bark", bark_action);


    }

}



//Class that defines each trick
public class Trick {

    //Some tricks require special items e.g stick/ball for fetch
    List<Item> trickItemRequirements;
    public TrickName trickName;
    
    //For executing the trick after a delay
    public float trickDelay;

    public Action<object[]> trick_action;
    public List<Type> desiredMethodParameterTypes;

    public Trick(TrickName _trick) {
        this.trickName = _trick;
        this.trickDelay = 0f;
        trickItemRequirements = new List<Item>();

        if (TrickManager.instance.objectBasedActionsMap.ContainsKey(trickName.ToString())) {
            trick_action = TrickManager.instance.objectBasedActionsMap[trickName.ToString()];
        }

        desiredMethodParameterTypes = new List<Type>();
        //Add dog as first parameter object (same for all methods)
        //desiredMethodParameterObjects.Add(typeof(Dog));
        assignDesiredMethodInputs();
    }

    //Add the appropriate method parameters to desired list
    private void assignDesiredMethodInputs() {

        if(trickName == TrickName.Sniff) {
            desiredMethodParameterTypes.Add(typeof(GameObject));
        }
        if(trickName == TrickName.Bark) {
            desiredMethodParameterTypes.Add(typeof(Dog));
        }


    }



}
