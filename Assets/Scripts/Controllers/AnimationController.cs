using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class AnimationController : MonoBehaviour {
    public static AnimationController instance { get; protected set; }


    List<Animator> allDogAnimators;
    
    Dictionary<TrickName, Animation> trickNameAnimationMap;
    List<Animation> allAnimations;

    // Start is called before the first frame update
    void Start(){
        instance = this;
        allAnimations = new List<Animation>();
        trickNameAnimationMap = new Dictionary<TrickName, Animation>();

        allDogAnimators = new List<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Called from on dog created 
    public void registerDogAnimatorComponents(Dog dog) {
        //Animator component attached to master dog gameobject
        Animator dogAnimator = dog.dogAI.dogAnimator;
        Animator dogUIAnimator = dog.dogAI.dogIconAnimator;
    }

    public void playAnimationClip(AnimationClip clip) {
        

    }
}
