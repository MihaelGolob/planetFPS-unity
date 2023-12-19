using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Manager  {
    public void Awake() {
        Console.WriteLine("AudioManager initialized");
        DontDestroyOnLoad(gameObject);
    }
}
