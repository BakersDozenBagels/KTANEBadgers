using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLayout : MonoBehaviour {
    public Texture[] Textures;
    public GameObject CardTemplate;

	// Use this for initialization
	void Start () {
        float offset = 0f;
		foreach (Texture texture in Textures)
        {
            var card = Instantiate(CardTemplate);
            card.GetComponent<MeshRenderer>().material.mainTexture = texture;
            card.transform.localPosition = new Vector3((offset % 7f) / 10f, 0f, (offset - (offset % 7f)) / 70f);
            offset += 1f;
        }
	}
}
