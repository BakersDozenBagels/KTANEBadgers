﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MoleScript : MonoBehaviour
{
    public Texture[] cardTextures;
    public GameObject cardTemplate;
    public Transform spawnPoint, midPoint, endPoint;

    public KMBombModule Module;
    public KMSelectable LeftButton, RightButton, BadgerButton;

    private const float VOFFSET = 0.00025f;
    private Stack<GameObject> CardsRight = new Stack<GameObject>(), CardsLeft = new Stack<GameObject>();

    private bool _solved = false;

    private List<Card> Cards = new List<Card>();

    private Card target;
    private int targetPos;

    private MoleExtensions.MoleMode mode = MoleExtensions.MoleMode.None;

    // Use this for initialization
    void Start()
    {
        Cards = new List<Card>();
        for (int i = 0; i < cardTextures.Length; i++)
        {
            Cards.Add(new Card(cardTextures[i], (i % 13) + 2, Mathf.FloorToInt(i / 13)));
            if (Cards.Last().Rank == 14) Cards.Last().Rank = 1;
        }

        int a = UnityEngine.Random.Range(0, 2);
        mode |= a == 0 ? MoleExtensions.MoleMode.Up : MoleExtensions.MoleMode.Down;
        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0:
                mode |= MoleExtensions.MoleMode.Right1;
                break;
            case 1:
                mode |= MoleExtensions.MoleMode.Right3;
                break;
            case 2:
                mode |= MoleExtensions.MoleMode.Right5;
                break;
            case 3:
                mode |= MoleExtensions.MoleMode.Right7;
                break;
        }

        Cards = Cards.MoleShuffle(mode);

        bool chk = true;
        do
        {
            Texture targetTexture = cardTextures.PickRandom();
            Card target = new Card(targetTexture, (System.Array.IndexOf(cardTextures, targetTexture) % 13) + 2, Mathf.FloorToInt(System.Array.IndexOf(cardTextures, targetTexture) / 13));
            for (int i = Cards.Count() - 2; i >= 0; i--)
            {
                if (!MoleExtensions.MoleCheck(mode, target, Cards[i + 1]) && !MoleExtensions.MoleCheck(mode, Cards[i], target))
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

public static class MoleExtensions
{
    public static List<Card> MoleShuffle(this List<Card> list, MoleMode mode)
    {
        list.AddRange(list);
        List<Card> outList = new List<Card>();
        outList.Add(list.PickRandom());
        list.Remove(outList[0]);
        while (list.Where(c => MoleCheck(mode, c, outList.Last())).Count() > 0 && outList.Count() < 103)
        //while (list.Where(c => c.Rank == outList.Last().Rank || c.Suit == outList.Last().Suit).Count() > 0)
        {
            outList.Add(list.Where(c => MoleCheck(mode, c, outList.Last())).PickRandom());
            //outList.Add(list.Where(c => c.Rank == outList.Last().Rank || c.Suit == outList.Last().Suit).PickRandom());
            list.Remove(outList.Last());
        }
        outList.Reverse();
        list = outList;
        return list;
    }

    public static bool MoleCheck(MoleMode mode, Card played, Card on)
    {
        bool outBool = true;
        if (mode == MoleMode.None)
            outBool &= true;
        if ((mode & MoleMode.Uno) == MoleMode.Uno)
            outBool &= played.Rank == on.Rank || played.Suit == on.Suit;
        if ((mode & MoleMode.AntiUno) == MoleMode.AntiUno)
            outBool &= played.Rank != on.Rank && played.Suit != on.Suit;
        if ((mode & MoleMode.Up) == MoleMode.Up)
            outBool &= played.Rank >= on.Rank || on.Rank >= 11;
        if ((mode & MoleMode.Down) == MoleMode.Down)
            outBool &= played.Rank <= on.Rank || played.Rank >= 11;
        if ((mode & MoleMode.Right1) == MoleMode.Right1)
            outBool &= played.Suit == on.Suit || played.Suit == (on.Suit + 1) % 8 || played.Rank == on.Rank;
        if ((mode & MoleMode.Right3) == MoleMode.Right3)
            outBool &= played.Suit == on.Suit || played.Suit == (on.Suit + 3) % 8 || played.Rank == on.Rank;
        if ((mode & MoleMode.Right5) == MoleMode.Right5)
            outBool &= played.Suit == on.Suit || played.Suit == (on.Suit + 5) % 8 || played.Rank == on.Rank;
        if ((mode & MoleMode.Right7) == MoleMode.Right7)
            outBool &= played.Suit == on.Suit || played.Suit == (on.Suit + 7) % 8 || played.Rank == on.Rank;
        return outBool;
    }

    public enum MoleMode
    {
        None = 0,
        Uno = 1,
        AntiUno = 2,
        Up = 4,
        Down = 8,
        Right1 = 16,
        Right3 = 32,
        Right5 = 64,
        Right7 = 128
    }
}