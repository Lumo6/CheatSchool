using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    bool iscopying = false;
    bool copied = false;
    public GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void copying()
    {
        iscopying = true;
    }

    public void stopcopying()
    {
        iscopying = false;
    }

    public void copieddone()
    {
        copied = true;
    }

    public bool IsCopying()
    {
        return iscopying;
    }

}
