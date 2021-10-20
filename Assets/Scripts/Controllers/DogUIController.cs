using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;


//Used to display dog UI eg selected dog, dog emote popups etc
public class DogUIController : MonoBehaviour {

    public static DogUIController instance;

    public Dictionary<Emotion, Sprite> emotionSpriteMap;

    // Use this for initialization
    void Start() {
        instance = this;
        registerEmoteSprites();
    }

    // Update is called once per frame
    void Update() {

    }

    public void registerEmotePoint(Dog dog) {
        //Find the emote point gameobject in children and assign to dog
        Transform[] children = dog.dogMasterGO.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children) {
            if (t.name == "EmotePoint") {
                dog.emotePoint = t.gameObject;
            }
        }
        if (dog.emotePoint == null) { Debug.Log("Emote point is null for dog " + dog.breed.ToString()); }
        //Defines where the emote should appear (eventually want to have this dynamically attached to dogs head - use normal bone to dog head mesh somehow)
        
        RectTransform[] childrenn = dog.emotePoint.GetComponentsInChildren<RectTransform>(true);

        foreach (RectTransform t in childrenn) {
            if (t.name == "EmoteCanvas") { dog.dogAI.emoteCanvas = t.gameObject; }
            if (t.name == "EmoteSprite") { dog.dogAI.emoteSpriteObject = t.gameObject; }
        }
        
        dog.dogAI.dogIconAnimator = dog.dogAI.emoteSpriteObject.GetComponent<Animator>();
        //Debug.Log(dog.dogAI.dogIconAnimator.name);

    }

    //Used to display emote popup over dogs head
    public void emotePopup(Dog dog, Emotion emote) {
        //dog.dogAI.emoteCanvas.SetActive(false);
        //dog.emotePoint.transform.gameObject.SetActive(true);
        Image img = dog.dogAI.emoteSpriteObject.GetComponent<Image>();
        Animator animator = dog.dogAI.dogIconAnimator;


        animator.ResetTrigger("EmotePopup");

        if (emotionSpriteMap.ContainsKey(emote)) {
            if(img != null)
            img.sprite = emotionSpriteMap[emote];
        }
        else {
            Debug.LogWarning("Emotepopup: Sprite not found in dictionary!");
        }

        //Do the popup
        if (dog.dogAI.emoteCanvas != null) {
            dog.dogAI.emoteCanvas.SetActive(true);
            //Debug.Log(animator);
            animator.Play("EmotePopup");

            Task.Delay(10000).ContinueWith(t => dog.dogAI.emoteCanvas.SetActive(false));
            //dog.dogAI.dogIconAnimator.SetTrigger("EmotePopup");

        }
        

    }

   

    


    //Setup emote sprite dictionaries etc
    public void registerEmoteSprites() {

        emotionSpriteMap = new Dictionary<Emotion, Sprite>();

        Sprite[] emoteSprites = Resources.LoadAll<Sprite>("UI/Emotes/EmoteIcons");
        
        foreach (Sprite s in emoteSprites) {
            Sprite sp = (Sprite)s;
            
            Emotion emote = (Emotion)Enum.Parse(typeof(Emotion), sp.name);

            emotionSpriteMap.Add(emote, sp);

        }

    }

}
