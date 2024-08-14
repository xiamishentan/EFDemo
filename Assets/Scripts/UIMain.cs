using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    public Button RefreshBtn;
    public Button NextBtn;
    public Button ChangeBtn;
    public Button AudioBtn;
    public Button TipsBtn;
    public Button CloseBtn;
    public Image EnSelect;
    public Image CnSelect;

    public UISoltItem[] Items;

    //
    public RawImage uiTex;
    public float PaintSpeed;
    // Use this for initialization
    Texture2D tex;
    Texture2D MyTex;
    int mWidth;
    int mHeight;
    //[Header("Brush Size")]
    public int brushSize = 50;
    //[Header("Rate")]
    public int restoreWidth = 80;
    public int restoreHeigh = 80;
    float maxColorA;
    float colorA;

    private int m_curtAudioIndex;

    private void Awake()
    {
        tex = (Texture2D)uiTex.mainTexture;
        MyTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
        mWidth = MyTex.width;
        mHeight = MyTex.height;
        MyTex.SetPixels(tex.GetPixels());
        MyTex.Apply();
        uiTex.texture = MyTex;
        maxColorA = MyTex.GetPixels().Length;
        colorA = 0;
    }
    void Start()
    {
        RefreshBtn.onClick.AddListener(OnRefreshBtnClicked);
        NextBtn.onClick.AddListener(OnNextBtnClicked);
        ChangeBtn.onClick.AddListener(OnChangeBtnClicked);
        AudioBtn.onClick.AddListener(OnAudioBtnClicked);
        TipsBtn.onClick.AddListener(OnTipsBtnClicked);
        CloseBtn.onClick.AddListener(OnCloseBtnClicked);
        CnSelect.gameObject.SetActive(false);
        m_curtAudioIndex = 0;

        MaskableGraphic maskableGraphic = gameObject.AddComponent<MaskableGraphic>();

        GamePlayMgr.Instance.OnActPaintMove += OnPaintMove;
        GamePlayMgr.Instance.OnActPaintImmediately += OnActPaintImmediately;
        GamePlayMgr.Instance.OnActPaintRestore += OnPaintResotre;
        GamePlayMgr.Instance.OnActSoltItemRest += OnActSoltItemRest;
    }

    private void OnDestroy()
    {
        RefreshBtn.onClick.RemoveAllListeners();
        NextBtn.onClick.RemoveAllListeners();
        ChangeBtn.onClick.RemoveAllListeners();
        AudioBtn.onClick.RemoveAllListeners();
        TipsBtn.onClick.RemoveAllListeners();
        CloseBtn.onClick.RemoveAllListeners();
        if (GamePlayMgr.Instance)
        {
            GamePlayMgr.Instance.OnActPaintMove -= OnPaintMove;
            GamePlayMgr.Instance.OnActPaintImmediately -= OnActPaintImmediately;
            GamePlayMgr.Instance.OnActPaintRestore -= OnPaintResotre;
            GamePlayMgr.Instance.OnActSoltItemRest -= OnActSoltItemRest;
        }
    }

    public void InitItems()
    {
        for (int i = 0;i < Items.Length;i++)
        {
            Items[i].Init();
        }
    }

    void OnActSoltItemRest()
    {
    }

    bool m_startPaint = false;
    bool m_bRestore = false;
    Vector2 m_paintTarget;
    Vector2 m_paintStart;
    Vector2 m_curtPaintPos;
    Vector2 m_curtPaintDir;
    MoveDir m_curDir;
    void OnPaintMove(MoveDir dir,Vector2 start,Vector2 target)
    {
        m_curDir = dir;
        m_bRestore = false;
        m_paintStart = start;
        m_paintTarget = target;
        m_curtPaintPos = start;
        m_curtPaintDir = (target - start).normalized;
        m_startPaint = true;
    }

    void OnActPaintImmediately(Vector2 target)
    {
        CheckPoint(0,target);
    }

    void OnPaintResotre(Vector2 target)
    {
        //m_curDir = dir;
        m_bRestore = true;
        m_restrecount = 0;
        //m_paintStart = start;
        m_paintTarget = target;
        //m_curtPaintPos = start;
        //m_curtPaintDir = (target - start).normalized;
        //m_startPaint = true;
    }

    float m_restrecount = 0;
    // Update is called once per frame
    void Update()
    {
        if (m_startPaint)
        {
            m_curtPaintPos += m_curtPaintDir * Time.deltaTime * PaintSpeed;
            if (m_curDir == MoveDir.Right)
            {
                if (m_curtPaintPos.x >= m_paintTarget.x)
                {
                    m_startPaint = false;
                }
            }
            else if(m_curDir == MoveDir.Left)
            {
                if (m_curtPaintPos.x <= m_paintTarget.x)
                {
                    m_startPaint = false;
                }
            }
            else if (m_curDir == MoveDir.Up)
            {
                if (m_curtPaintPos.y >= m_paintTarget.y)
                {
                    m_startPaint = false;
                }
            }
            else if (m_curDir == MoveDir.Down)
            {
                if (m_curtPaintPos.y <= m_paintTarget.y)
                {
                    m_startPaint = false;
                }
            }

            //继续回退
            //if(!m_startPaint && m_bRestore)
            //{
            //    GamePlayMgr.Instance.ResetItemSelect();
            //}

            MoveTarget(0, m_paintStart, m_curtPaintPos);
        }

        if (m_bRestore)
        {
            m_restrecount += Time.deltaTime;
            if(m_restrecount > 0.5f)
            {
                m_bRestore = false;
                RestorePoint(restoreWidth,restoreHeigh,m_paintTarget);
                GamePlayMgr.Instance.ResetItemSelect();
            }
        }
    }

    void OnRefreshBtnClicked()
    {
        Debug.Log("OnRefreshBtnClicked");
        GamePlayMgr.Instance.SendResetItem();
    }
    void OnNextBtnClicked()
    {
        Debug.Log("OnNextBtnClicked");
        bool right =  GamePlayMgr.Instance.CheckAnswer();
        if (!right)
        {
            GamePlayMgr.Instance.TipsWrongAnswer();
        }
    }
    void OnChangeBtnClicked()
    {
        Debug.Log("OnChangeBtnClicked");
        CnSelect.gameObject.SetActive(EnSelect.gameObject.activeSelf);
        EnSelect.gameObject.SetActive(!EnSelect.gameObject.activeSelf);
    }
    void OnAudioBtnClicked()
    {
        Debug.Log("OnAudioBtnClicked");
        ConfigDatas datas = ConfigDataMgr.Instance.GetDatas(0);
        if (datas != null)
        {
            if(m_curtAudioIndex < datas.Activity.Stimulus.Count)
            {
                StimulusData _Stiumlusdatas = datas.Activity.Stimulus[m_curtAudioIndex];
                GamePlayMgr.Instance.LoadAudioFromWeb(_Stiumlusdatas.Body.item.audio.url);
            }

            m_curtAudioIndex++;
            if (m_curtAudioIndex >= datas.Activity.Stimulus.Count)
            {
                m_curtAudioIndex = 0;
            }
        }
        else
        {
            Debug.LogError("OnAudioBtnClicked ConfigDatas is Null!!");
        }

    }
    void OnTipsBtnClicked()
    {

        Debug.Log("OnTipsBtnClicked");
    }
    void OnCloseBtnClicked()
    {
        Debug.Log("OnCloseBtnClicked");
    }

    /// <summary>
    /// 贝塞尔平滑
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="mid">中点</param>
    /// <param name="end">终点</param>
    /// <param name="segments">段数</param>
    /// <returns></returns>
    public Vector2[] Beizier(Vector2 start, Vector2 mid, Vector2 end, int segments)
    {
        float d = 1f / segments;
        Vector2[] points = new Vector2[segments - 1];
        for (int i = 0; i < points.Length; i++)
        {
            float t = d * (i + 1);
            points[i] = (1 - t) * (1 - t) * mid + 2 * t * (1 - t) * start + t * t * end;
        }
        List<Vector2> rps = new List<Vector2>();
        rps.Add(mid);
        rps.AddRange(points);
        rps.Add(end);
        return rps.ToArray();
    }


    public void MoveTarget(float alpha, Vector2 startPos,Vector2 Targetposition)
    {
        if (Vector2.Distance(Targetposition, startPos) > 1)
        {
            Vector2 pos = Targetposition;
            CheckPoint(alpha,Targetposition);
        }
    }

    void RestorePoint(int width, int height, Vector3 targetPos)
    {
        Vector3 localPos = uiTex.gameObject.transform.InverseTransformPoint(targetPos);

        if (localPos.x > -mWidth / 2 && localPos.x < mWidth / 2 && localPos.y > -mHeight / 2 && localPos.y < mHeight / 2)
        {
            for (int i = (int)localPos.x - width; i < (int)localPos.x + width; i++)
            {
                for (int j = (int)localPos.y - height; j < (int)localPos.y + height; j++)
                {
                    //if (Mathf.Pow(i - localPos.x, 2) + Mathf.Pow(j - localPos.y, 2) > Mathf.Pow(brushSize, 2))
                    //    continue;
                    if (i < 0) { if (i < -mWidth / 2) { continue; } }
                    if (i > 0) { if (i > mWidth / 2) { continue; } }
                    if (j < 0) { if (j < -mHeight / 2) { continue; } }
                    if (j > 0) { if (j > mHeight / 2) { continue; } }

                    Color col = MyTex.GetPixel(i + (int)mWidth / 2, j + (int)mHeight / 2);
                    col.a = 1;
                    MyTex.SetPixel(i + (int)mWidth / 2, j + (int)mHeight / 2, col);
                }
            }
            MyTex.Apply();
        }
    }

    void CheckPoint(float alpha,Vector3 targetPos)
    {
        //Vector3 worldPos = Camera.main.ScreenToWorldPoint(pScreenPos);
        Vector3 localPos = uiTex.gameObject.transform.InverseTransformPoint(targetPos);

        if (localPos.x > -mWidth / 2 && localPos.x < mWidth / 2 && localPos.y > -mHeight / 2 && localPos.y < mHeight / 2)
        {
            for (int i = (int)localPos.x - brushSize; i < (int)localPos.x + brushSize; i++)
            {
                for (int j = (int)localPos.y - brushSize; j < (int)localPos.y + brushSize; j++)
                {
                    if (Mathf.Pow(i - localPos.x, 2) + Mathf.Pow(j - localPos.y, 2) > Mathf.Pow(brushSize, 2))
                        continue;
                    if (i < 0) { if (i < -mWidth / 2) { continue; } }
                    if (i > 0) { if (i > mWidth / 2) { continue; } }
                    if (j < 0) { if (j < -mHeight / 2) { continue; } }
                    if (j > 0) { if (j > mHeight / 2) { continue; } }

                    Color col = MyTex.GetPixel(i + (int)mWidth / 2, j + (int)mHeight / 2);
                    col.a = alpha;
                    MyTex.SetPixel(i + (int)mWidth / 2, j + (int)mHeight / 2, col);
                }
            }
            MyTex.Apply();
        }
    }
}