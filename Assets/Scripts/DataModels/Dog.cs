using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Data class to hold all dog attributes/stats
/// </summary>

/////////////////////////FIXED PROPERTIES/////////////////////////
//Dog breeds
public enum Breed { Labrador, Shiba, Pomeranian, Poodle, Pug, animated };
/////////////////////////DYNAMIC PROPERTIES//////////////////////
//Personality traits
public enum Trait {Shy, Loud, Energetic, Calm, Friendly, Aggressive,  };

//Dog emotions
public enum Emotion { Hungry, Thirsty, Bored, Tired, Happy, Sad, Curious, Excited};

//Enum of dog sizes -- for setting movement properties etc
public enum DogSize { Small, Medium, Large}

public enum MovementSpeed {Slow, Normal, Run }


public class Dog {

    /////////////////////////Global dog properties/////////////////////////

    //Dog changed event 
    public event Action<Dog> cbOnDogChanged;
    //Dog AI agent 
    public DogAI dogAI;
    //Master game object this dog represents (has mesh, nose etc objects as children)
    public GameObject dogMasterGO { get; set; }
    
    //Destination dog currently travelling to
    public Transform destination { get; set; }

    

    /////////////////////////Individual properties//////////////////////
    public Breed breed { get; set; }
    public DogSize size { get; protected set; }

    //List of tricks the dog knows how to do
    List<Trick> knownTricks { get; set; }

    //List of personality traits this dog has (fixed (possibly random) at creation)
    List<Trait> dogTraits { get; set; }

    //List of current dog emotions (dynamic)
    public List<Emotion> dogEmotions { get; set; }
    //Emote popup point (empty game object)
    public GameObject emotePoint { get; set; }
 

    //Dog has favourite item, some dogs are sad unless item is available 
    Item favItem;

    //List defining dog sizes based on breed
    List<Breed> smallDogs = new List<Breed> {Breed.Pomeranian, Breed.Pug };
    List<Breed> mediumDogs = new List<Breed> { Breed.Poodle, Breed.Shiba };
    List<Breed> largeDogs = new List<Breed> { Breed.Labrador };
    //Vector3 with dog dimensions - set from ondogcreated
    public Vector3 dogDimensions { get; set; }
    ////////////////////////////CONSTRUCTORS/////////////////////////


    //Default dog constructor
    public Dog() {
    }

    //Constructs dog of specific breed with randomized personality traits
    public Dog(Breed _breed) {
        breed = _breed;
        dogMasterGO = null;
        dogAI = new DogAI(this);
        size = assignSize(_breed);
        destination = null;
        dogEmotions = new List<Emotion>();
        emotePoint = null; //set when dog prefab object spawns
    }

    

    public Breed getBreed() {

        return this.breed;
    }

    public DogSize assignSize (Breed breed) {
        if (smallDogs.Contains(breed)) {
            return DogSize.Small;
        }
        if (mediumDogs.Contains(breed)) {
            return DogSize.Medium;
        }
        if (largeDogs.Contains(breed)) {
            return DogSize.Large;
        }

        
        //Return medium as default
        return DogSize.Medium;
    }






    ///////////////////////////DOG_UPDATE///////////////////////////////
    //For updating dog movement/animation/etc
    //Calls onDogChanged  
    
    //Interval between updates
    float deltaTime;
    public void Update(float deltaTime) {
        
        
        InputController.Instance.processClicksForDogMove(this);
        
        
        cbOnDogChanged?.Invoke(this);
        
        dogAI.UpdateAI();

    }


    //////////////////////////////////////////////////////////////////////


    /////////////////////////////////////////////////////////////////////


    /////////////////////////////////////////////////////////////////////


    //////////////////////////////////////////////////////////////////////


    //////////////////////////////////////////////////////////////////////


    /////////////////////////////////////////////////////////////////////


    /////////////////////////////////////////////////////////////////////






}
