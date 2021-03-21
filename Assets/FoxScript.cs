using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FoxScript : MonoBehaviour {
    public Texture[] cardTextures;
    public GameObject cardTemplate;
    public Transform spawnPoint, midPoint, endPoint;

    public KMBombModule Module;
    public KMSelectable LeftButton, RightButton, BadgerButton;

    private const float VOFFSET = 0.0005f;
    private Stack<GameObject> CardsRight = new Stack<GameObject>(), CardsLeft = new Stack<GameObject>();

    private bool _solved = false;

    private List<Card> Cards = new List<Card>();

    private Card target;
    private int targetPos;

	// Use this for initialization
	void Start () {
        Cards = new List<Card>();
        for (int i = 0; i < cardTextures.Length; i++)
        {
            Cards.Add(new Card(cardTextures[i], (i % 13) + 2, Mathf.FloorToInt(i / 13)));
            if (Cards.Last().Rank == 14) Cards.Last().Rank = 1;
        }
        Cards = Cards.FoxShuffle();
        bool chk = true;
        do
        {
            Texture targetTexture = cardTextures.PickRandom();
            Card target = new Card(targetTexture, (System.Array.IndexOf(cardTextures, targetTexture) % 13) + 2, Mathf.FloorToInt(System.Array.IndexOf(cardTextures, targetTexture) / 13));
            for (int i = Cards.Count() - 2; i >= 0; i--)
            {
                if (!((Cards[i].Rank == target.Rank) || (target.Rank >= 11 && Cards[i].Suit == target.Suit) || (Cards[i].Rank > target.Rank && Cards[i].Suit == target.Suit)) && !((target.Rank == Cards[i+1].Rank) || (Cards[i+1].Rank >= 11 && target.Suit == Cards[i+1].Suit) || (Cards[i+1].Rank < target.Rank && target.Suit == Cards[i + 1].Suit)))
                {
                    Cards.Insert(i + 1, target);
                    targetPos = i + 1;
                    chk = false;
                    break;
                }
            }
        }
        while (chk);

        for (int i = 0; i < Cards.Count; i++)
        {
            CardsRight.Push(Instantiate(cardTemplate, spawnPoint.position, spawnPoint.rotation, transform));
            CardsRight.Peek().transform.localPosition = new Vector3(CardsRight.Peek().transform.localPosition.x, CardsRight.Peek().transform.localPosition.y + i * VOFFSET, CardsRight.Peek().transform.localPosition.z);
            CardsRight.Peek().GetComponent<MeshRenderer>().material.mainTexture = Cards[i].Texture;
            CardsRight.Peek().transform.localEulerAngles = new Vector3(90f, 0f, 180f);
        }

        LeftButton.OnInteract += Left;
        RightButton.OnInteract += Right;
        BadgerButton.OnInteract += Badger;
	}

    private bool Left()
    {
        if (CardsRight.Count == 0) return false;
        StartCoroutine(MoveLeft(CardsRight.Peek(), CardsRight.Count * VOFFSET - VOFFSET, CardsLeft.Count * VOFFSET));
        CardsLeft.Push(CardsRight.Pop());
        return false;
    }

    private bool Right()
    {
        if (CardsLeft.Count == 0) return false;
        StartCoroutine(MoveRight(CardsLeft.Peek(), CardsLeft.Count * VOFFSET - VOFFSET, CardsRight.Count * VOFFSET));
        CardsRight.Push(CardsLeft.Pop());
        return false;
    }

    private bool Badger()
    {
        if (_solved) return false;
        if (CardsRight.Count == targetPos)
        {
            _solved = true;
            Module.HandlePass();
        }
        else
        {
            Module.HandleStrike();
        }
        return false;
    }

    private IEnumerator MoveLeft(GameObject card, float start, float end)
    {
        float time = 0f;
        while (time < 0.25f)
        {
            yield return null;
            time += Time.deltaTime;
            card.transform.localPosition = new Vector3(Mathf.Lerp(spawnPoint.localPosition.x, midPoint.localPosition.x, time * 4f), Mathf.Lerp(spawnPoint.localPosition.y + start, midPoint.localPosition.y, time * 4f), Mathf.Lerp(spawnPoint.localPosition.z, midPoint.localPosition.z, time * 4f));
            card.transform.localRotation = Quaternion.Lerp(spawnPoint.localRotation, midPoint.localRotation, time * 4f);
        }
        time = 0f;
        while (time < 0.25f)
        {
            yield return null;
            time += Time.deltaTime;
            card.transform.localPosition = new Vector3(Mathf.Lerp(midPoint.localPosition.x, endPoint.localPosition.x, time * 4f), Mathf.Lerp(midPoint.localPosition.y, endPoint.localPosition.y + end, time * 4f), Mathf.Lerp(midPoint.localPosition.z, endPoint.localPosition.z, time * 4f));
            card.transform.localRotation = Quaternion.Lerp(midPoint.localRotation, endPoint.localRotation, time * 4f);
        }
    }

    private IEnumerator MoveRight(GameObject card, float start, float end)
    {
        float time = 0f;
        while (time < 0.25f)
        {
            yield return null;
            time += Time.deltaTime;
            card.transform.localPosition = new Vector3(Mathf.Lerp(endPoint.localPosition.x, midPoint.localPosition.x, time * 4f), Mathf.Lerp(endPoint.localPosition.y + start, midPoint.localPosition.y, time * 4f), Mathf.Lerp(endPoint.localPosition.z, midPoint.localPosition.z, time * 4f));
            card.transform.localRotation = Quaternion.Lerp(endPoint.localRotation, midPoint.localRotation, time * 4f);
        }
        time = 0f;
        while (time < 0.25f)
        {
            yield return null;
            time += Time.deltaTime;
            card.transform.localPosition = new Vector3(Mathf.Lerp(midPoint.localPosition.x, spawnPoint.localPosition.x, time * 4f), Mathf.Lerp(midPoint.localPosition.y, spawnPoint.localPosition.y + end, time * 4f), Mathf.Lerp(midPoint.localPosition.z, spawnPoint.localPosition.z, time * 4f));
            card.transform.localRotation = Quaternion.Lerp(midPoint.localRotation, spawnPoint.localRotation, time * 4f);
        }
    }
}

public class Card
{
    public Texture Texture { get; set; }
    public int Rank { get; set; }
    public int Suit { get; set; }

    public Card(Texture Texture, int Rank, int Suit)
    {
        this.Texture = Texture;
        this.Rank = Rank;
        this.Suit = Suit;
    }
}

public static class FoxExtensions
{
    public static List<Card> FoxShuffle(this List<Card> list)
    {
        list.AddRange(list);
        list.AddRange(list);
        List<Card> outList = new List<Card>();
        outList.Add(list.PickRandom());
        list.Remove(outList[0]);
        while (list.Where(c => (c.Rank > outList.Last().Rank && c.Suit == outList.Last().Suit) || (c.Rank == outList.Last().Rank) || (11 <= outList.Last().Rank && outList.Last().Rank <= 13 && c.Suit == outList.Last().Suit)).Count() > 0 && outList.Count() < 51)
        //while (list.Where(c => c.Rank == outList.Last().Rank || c.Suit == outList.Last().Suit).Count() > 0)
        {
            outList.Add(list.Where(c => (c.Rank > outList.Last().Rank && c.Suit == outList.Last().Suit) || (c.Rank == outList.Last().Rank) || (11 <= outList.Last().Rank && outList.Last().Rank <= 13 && c.Suit == outList.Last().Suit)).PickRandom());
            //outList.Add(list.Where(c => c.Rank == outList.Last().Rank || c.Suit == outList.Last().Suit).PickRandom());
            list.Remove(outList.Last());
        }
        outList.Reverse();
        list = outList;
        return outList;
    }
}