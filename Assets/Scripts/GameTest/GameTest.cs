using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTest : MonoBehaviour
{
    private UIManager mUIMgr;
    
    // Start is called before the first frame update
    void Start()
    {
        mUIMgr = UIManager.Inst;
        mUIMgr.Register();
        mUIMgr.Show(UIPanelName.UITest);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
