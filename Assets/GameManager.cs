using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gm;

    private void OnEnable()
    {
        if (GameManager.gm == null)
            GameManager.gm = this;
        else
            if (GameManager.gm != this)
                Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public Computer computer = new Computer();

    public Computer GetComputer()
    {
        return this.computer;
    }

    public void SetComputer(Computer computerIn)
    {
        this.computer = computerIn;
    }

    public void SetComputer(string cpu, string memory)
    {
        this.computer.cpu = cpu;
        this.computer.memory = memory;
    }
}
