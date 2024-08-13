using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppRoot : MonoBehaviour
{
    public static AppRoot Instance;

    public UIMain MainUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitClientDatas());
    }


    IEnumerator InitClientDatas()
    {
        yield return  new WaitForEndOfFrame();

        MainUI.InitItems();

        GamePlayMgr.Instance.Init();
    }
        // Update is called once per frame
    void Update()
    {
        
    }
}
