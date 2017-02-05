using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour {

    public string spriteSheetName;
	
    // Use this for initialization
	void Start () {
        spriteSheetName = "warrior";
    }

    // Update is called once per frame
    void LateUpdate() {
        var subSprites = Resources.LoadAll<Sprite>("Characters");
        string[] currentSpriteNameParts = GetComponent<SpriteRenderer>().sprite.name.Split('_');
        string newSpriteName = spriteSheetName + "_" + currentSpriteNameParts[1] + "_" + currentSpriteNameParts[2];

        var newSprite = Array.Find(subSprites, item => item.name == newSpriteName);
        
        if(newSprite)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }  
    }
}
