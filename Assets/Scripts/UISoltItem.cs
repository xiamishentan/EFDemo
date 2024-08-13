using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISoltItem : MonoBehaviour
{
    public Button ItemButton;
    public Image Normal;
    public Image Select;
    public Image Dissolve;
    public Image Move;
    public Material DissolveMat;
    public int Index;    //下标索引
    public int RowIndex;  //行列值
    public int ColIndex;
    public Vector2 MoveTargetSize = Vector2.zero;

    private bool m_Select = false;
    private bool m_CanSelect = false;
    private bool m_startDissolve = false;

    private RectTransform m_MoveRectTrans;
    public RectTransform RectTrans { get { return m_MoveRectTrans; } }
    private void Awake()
    {
        m_MoveRectTrans = Move.GetComponent<RectTransform>();
    }
    // Use this for initialization
    void Start()
    {
        ItemButton.onClick.AddListener(OnItemClicked);
        GamePlayMgr.Instance.OnActItemSelected += OnItemSelected;
        GamePlayMgr.Instance.OnActSoltItemRestEnd += OnActSoltItemRestEnd;
        Material mat = new Material(DissolveMat);
        Dissolve.material = mat;
        m_MoveRectTrans = Move.GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        ItemButton.onClick.RemoveAllListeners();
        if (GamePlayMgr.Instance)
        {
            GamePlayMgr.Instance.OnActItemSelected -= OnItemSelected;
            GamePlayMgr.Instance.OnActSoltItemRestEnd -= OnActSoltItemRestEnd;
        }
    }
    // Update is called once per frame
    float m_DissolveSpeed = 0;
    void Update()
    {
        if(m_startDissolve)
            UpdateDissolve(m_DissolveSpeed);

    }

    public void Init()
    {
        string _ImgPath = GetImagePathByIndex();
        if (string.IsNullOrEmpty(_ImgPath))
        {
            Debug.LogError("Can Not Find ImagePath Url RowIndex = " + RowIndex + ";ColIndex = " + ColIndex);
        }

        GamePlayMgr.Instance.LoadTextureFromWeb(_ImgPath,Normal,Select);

        Normal.color = new Color(1, 1, 1, 0.7f);
        Normal.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        m_Select = false;
        if (RowIndex == 2 && ColIndex == 1)
        {
            m_CanSelect = true;
            OnItemClicked();
        }
        else
        {
            ResetDefaultState();
        }
        
    }

    void ResetDefaultState()
    {
        Select.gameObject.SetActive(false);
        Move.gameObject.SetActive(false);
        Normal.gameObject.SetActive(true);
        Normal.transform.DOScale(0.9f, 0.5f);
        Normal.color = new Color(1, 1, 1, 0.7f);
        Normal.GetComponent<Shadow>().enabled = false;
    }

    public void TipsWrongAnswer()
    {
        if (Select.gameObject.activeSelf)
        {
            Select.transform.DOKill(); // 停止所有之前的动画
            Select.transform.DORotate(new Vector3(0, 0, 20), 0.2f, RotateMode.Fast).SetEase(Ease.Linear).SetLoops(4, LoopType.Yoyo).OnComplete (() => 
            {
                GamePlayMgr.Instance.ReturnToNearRightAnswer();
            });
        }
    }

    public string GetImagePathByIndex() 
    {
        ConfigDatas datas = ConfigDataMgr.Instance.GetDatas(0);
        if (datas != null)
        {
            List<QuestionsData> _Questions = datas.Activity.Questions;

            for (int i = 0; i < _Questions.Count;i++)
            {
                List<QuestionsBodyOptionsData> _Options = _Questions[i].Body.options;
                for (int j = 0; j < _Options.Count;j++)
                {
                    if (_Options[j].rowIndex == RowIndex && _Options[j].colIndex == ColIndex)
                    {
                        return _Options[j].image.url;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Can Not Find ImagePath Url RowIndex = " + RowIndex + ";ColIndex = " + ColIndex);
        }
        return "";
    }

    void OnItemClicked()
    {
        //不可选和已经选择的不惦记
        if (!m_CanSelect || m_Select)
            return;
        Select.gameObject.SetActive(true);
        Normal.gameObject.SetActive(false);
        //Move.gameObject.SetActive(true);
        //Dissolve
        //Dissolve.material.SetFloat("_ThresholdAlpha",1);
        //Normal.
        m_Select = true;
        GamePlayMgr.Instance.SendItemSelect(this,RowIndex,ColIndex);
    }

    public void OnItemSelected(UISoltItem select,int row,int col)
    {
        RefreshNeighborState(row, col);
       
    }

    void OnActSoltItemRestEnd()
    {
        if (RowIndex == 2 && ColIndex == 1)
        {
            m_CanSelect = true;
            OnItemClicked();
        }
    }


    public void RefreshPrevSelectState(MoveDir dir)
    {
        //Move.gameObject.SetActive(true);
        //m_startMove = true;
        //m_CurMoveDir = dir;
    }
    public void RefreshSelectState()
    {
        Select.gameObject.SetActive(true);
        //Move.gameObject.SetActive(true);
        Normal.gameObject.SetActive(false);
        m_startDissolve = true;
        m_DissolveSpeed = 0.6f;
    }

    public void ResetItem()
    {
        m_startDissolve = true;
        m_DissolveSpeed = -0.8f;
        m_Select = false;
        m_CanSelect = false;
        ResetDefaultState();
    }

    public void SelectToRowCol()
    {
        m_CanSelect = true;
        OnItemClicked();
    }

    void UpdateDissolve(float value)
    {
        float  alpha =  Dissolve.material.GetFloat("_ThresholdAlpha");
        if (value > 0 && alpha >= 1) 
        {
            m_startDissolve = false;
        }
        else if (value < 0 && alpha <= 0 )
        {
            m_startDissolve = false;
        }

        alpha += Time.deltaTime * value;

        Dissolve.material.SetFloat("_ThresholdAlpha", alpha);
    }

    public void RefreshNeighborState(int row,int col)
    {
        if (RowIndex == row && col == ColIndex)
        {
            RefreshSelectState();
            return;
        }
        bool isNeighbor = false;
        if (RowIndex == row && (col == ColIndex + 1 || col == ColIndex - 1))
        {
            isNeighbor = true;
        }
        else if (col == ColIndex && (row == RowIndex + 1 || row == RowIndex - 1))
        {
            isNeighbor = true;
        }

        if (isNeighbor)
        {
            
            if (!m_Select)
            {
                Select.gameObject.SetActive(false);
                Move.gameObject.SetActive(false);
                Normal.gameObject.SetActive(true);
                Normal.transform.DOScale(1f, 0.5f);
                Normal.color = new Color(1, 1, 1, 1f);
                Normal.GetComponent<Shadow>().enabled = true;
                m_CanSelect = true;
            }

        }
        else
        {
            if (!m_Select)
            {
                m_CanSelect = false;
                ResetDefaultState();
            }

        }
    }
}