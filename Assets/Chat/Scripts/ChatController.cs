using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class ChatController : MonoBehaviour
{
    [SerializeField] private Pictures pictures = null!;

    [Header("Chat UI Elements")]
    [SerializeField] private ChatNode chatNodePrefab = null!;
    [SerializeField] private Transform chatContentTransform = null!;


    private ChatEventPresenter eventPresenter = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
