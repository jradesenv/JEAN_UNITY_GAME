using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour {

    public static Sprite[] sprites;

    public PlayerStats stats;
    public string spriteSheetName;
	
    // Use this for initialization
	void Start () {
        if (SpriteController.sprites == null)
        {
            SpriteController.sprites = Resources.LoadAll<Sprite>("");
        }

        stats = GetComponent<PlayerStats>();
    }

    void LateUpdate() {
        spriteSheetName = Converters.CharacterClassToSpriteSheetName(stats.characterClass);

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer.sprite != null)
        {
            string[] currentSpriteNameParts = renderer.sprite.name.Split('_');
            string newSpriteName = spriteSheetName + "_" + currentSpriteNameParts[1] + "_" + currentSpriteNameParts[2];

            var newSprite = Array.Find(SpriteController.sprites, item => item.name == newSpriteName);

            if (newSprite)
            {
                renderer.sprite = newSprite;
            }
        }
    }
}
