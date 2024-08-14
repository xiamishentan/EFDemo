using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEditor.Progress;

//
public enum MoveDir
{
    None,
    Left,
    Right,
    Up,
    Down,
}

public class GamePlayMgr : MonoBehaviour
{
    public static GamePlayMgr Instance;

    public AudioSource AudioPlayer;

    public Vector3 MoveOffset = new Vector3(80,-20,0);

    public Action<UISoltItem,int, int> OnActItemSelected;

    public Action<MoveDir,Vector2, Vector2> OnActPaintMove;
    public Action<Vector2> OnActPaintImmediately;
    public Action<Vector2> OnActPaintRestore;
    public Action OnActSoltItemRestEnd;
    public Action OnActSoltItemRest;

    public int CurtSelectRowIndex { get; set; }
    public int CurtSelectColIndex { get;set; }

    //当前正确答案
    public List<string> CurtAnstwers = new List<string>();
    //save 
    Dictionary<string,AudioClip> m_audioClips = new Dictionary<string, AudioClip>();

    //全部选择的items
    List<UISoltItem> m_SlectItems = new List<UISoltItem>();

    //需要重置的items
    List<UISoltItem> m_NeedRestorItems = new List<UISoltItem>();

    //保存一个起始的
    UISoltItem m_RightRestorItems = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
    // Use this for initialization
    void Start()
    {
       
    }

    public void Init()
    {
        //初始一个正确答案
        RereshCurtAnswers(0);
    }

    public void RereshCurtAnswers(int questrionIndex)
    {
        CurtAnstwers.Clear();
        ConfigDatas data = ConfigDataMgr.Instance.GetDatas(0);
        if(data != null)
        {
            QuestionsData _qusetion = data.Activity.Questions[questrionIndex];
            //for (int i = 0; i < data.Activity.Questions.Count;i++)
            //{
                if (_qusetion != null)
                {
                    for (int n = 0; n < _qusetion.Body.answers.Count;n++)
                    {
                        for (int m = 0; m < _qusetion.Body.answers[n].Count;m++ )
                        {
                            CurtAnstwers.Add(_qusetion.Body.answers[n][m]);
                        }
                    }
                }
            //}
        }
    }

    public bool IsRightAnswer(int index)
    {
        if (CurtAnstwers.Contains(index.ToString()))
            return true;
        return false;
    }

    public bool CheckAnswer()
    {
        if(m_SlectItems.Count  != CurtAnstwers.Count)
            return false;

        for (int i = 0; i < m_SlectItems.Count;i++)
        {
            if (!CurtAnstwers[i].Equals(m_SlectItems[i].Index.ToString()))
                return false;
        }
        return true;
    }

    bool m_hasEnterReturn = false;
    public void ReturnToNearRightAnswer()
    {
        if (!m_hasEnterReturn)
        {
            m_NeedRestorItems.Clear();
            m_RightRestorItems = null;

            if (m_SlectItems.Count <= CurtAnstwers.Count)
            {
                int index = 0;
                for (int i = 0; i < m_SlectItems.Count; i++)
                {
                    if (!CurtAnstwers[i].Equals(m_SlectItems[i].Index.ToString()))
                    {
                        if(index == 0 && i > 0)
                        {
                            m_NeedRestorItems.Add(m_SlectItems[i - 1]);
                            m_RightRestorItems = m_SlectItems[i - 1];
                            index = i;
                            break;
                        }
                    }
                }
                //错误全部加到后面
                for (int i = index; i < m_SlectItems.Count;i++)
                {
                    m_NeedRestorItems.Add(m_SlectItems[i]);
                }

                //移除掉错误选择
                for (int i = 0; i < m_NeedRestorItems.Count; i++)
                {
                    m_SlectItems.Remove(m_NeedRestorItems[i]);
                }

                ResetItemSelect();
            }
            m_hasEnterReturn = true;
        }

    }
    int m_tips_count;
    public void TipsWrongAnswer()
    {
        m_hasEnterReturn = false;
        // tip
        if (m_tips_count >= 2)
        {
            m_tips_count = 0;

            SendResetItem();

            return;
        }

        //if (m_SlectItems.Count <= CurtAnstwers.Count)
        //{
            int startIndex = 0;
            for (int i = 0; i < m_SlectItems.Count; i++)
            {
                //超出正确答案
                if ( i >= CurtAnstwers.Count)
                {
                    startIndex = i;
                    break;
                }
                if (!CurtAnstwers[i].Equals(m_SlectItems[i].Index.ToString()))
                {
                    startIndex = i;
                    break;
                }
            }

            //
            for (int i = startIndex; i< m_SlectItems.Count; i++)
            {
                m_SlectItems[i].TipsWrongAnswer();
            }
            m_tips_count++;
        //}
    }

    public void LoadAudioFromWeb(string path)
    {
        if (m_audioClips.ContainsKey(path))
        {
            AudioClip clip = m_audioClips[path];
            AudioPlayer.clip = clip;
            AudioPlayer.Play();
        }
        else
        {
            StartCoroutine(LoadAudioFromURL(path));
        }
    }

    IEnumerator LoadAudioFromURL(string path)
    {
        yield return new WaitForEndOfFrame();

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);

                if (!m_audioClips.ContainsKey(path))
                {
                    m_audioClips.Add(path, myClip);
                }

                //加载成功自动播放
                AudioPlayer.clip = myClip;
                AudioPlayer.Play();
            }
            else
            {
                Debug.Log(www.error);
            
            }
        }
    }

    public void LoadTextureFromWeb(string filePath, Image imgNormal,Image imgSelect) 
    {
        StartCoroutine(LoadTextureFromUrl(filePath, imgNormal, imgSelect));
    }

    IEnumerator LoadTextureFromUrl(string filePath, Image imgNormal, Image imgSelect)
    {
        // 创建一个UnityWebRequest对象并加载本地图片
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(filePath);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // 获取加载的纹理
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            //把贴图赋到RawImage
            Sprite  sp = Sprite.Create(
                          texture,
                          new Rect(0, 0, texture.width, texture.height),
                          new Vector2(0.5f, 0.5f));
            imgNormal.sprite = sp;
            imgSelect.sprite = sp;
        }
        else
        {
            Debug.LogError("下载失败：" + www.error);
        }
    }

    public MoveDir  GetMoveDir(int prevRolIndex,int prevColIndex,int curRowIndex,int curColIndex) 
    {
        if (prevRolIndex == curRowIndex)
        {
            if(prevColIndex < curColIndex)
                return MoveDir.Right;
            else if(prevColIndex > curColIndex)
                return MoveDir.Left;
        }
        if (prevColIndex == curColIndex)
        {
            if (prevRolIndex < curRowIndex)
                return MoveDir.Down;
            else if (prevRolIndex > curRowIndex)
                return MoveDir.Up;
        }
        return MoveDir.None;
    }
    public void SendItemSelect(UISoltItem select,int row,int col)
    {
        if (m_SlectItems.Contains(select))
            return;
        if (m_SlectItems.Count > 0 )
        {
            UISoltItem items = m_SlectItems[m_SlectItems.Count - 1];
            if (items)
            {
                MoveDir dir = GetMoveDir(items.RowIndex, items.ColIndex, row, col);

                OnActPaintMove?.Invoke(dir,items.RectTrans.position + MoveOffset, select.RectTrans.position + MoveOffset);
            }
        }
        else
        {
            OnActPaintMove?.Invoke(MoveDir.Right,select.RectTrans.position + MoveOffset, select.RectTrans.position + MoveOffset + new Vector3(1,0,0));;
        }

        m_SlectItems.Add(select);

        OnActItemSelected?.Invoke(select,row, col);
    }

    public void SendResetItem()
    {
        m_NeedRestorItems.Clear();
        for (int i = 0; i < m_SlectItems.Count;i++)
        {
            m_NeedRestorItems.Add(m_SlectItems[i]);
        }

        m_SlectItems.Clear();

        Debug.Log(" m_SlectItems.Clear();");

        GamePlayMgr.Instance.ResetItemSelect();

        OnActSoltItemRest?.Invoke();
    }
    public void ResetItemSelect()
    {
        if (m_NeedRestorItems.Count > 0)
        {
            UISoltItem items1 = m_NeedRestorItems[m_NeedRestorItems.Count - 1];
            if (items1)
            { 
                OnActPaintRestore?.Invoke(items1.RectTrans.position);
                items1.ResetItem();
            }
            m_NeedRestorItems.RemoveAt(m_NeedRestorItems.Count - 1);
        }
        else 
        {
            if(m_RightRestorItems != null)
            {
                m_RightRestorItems.SelectToRowCol();
                SendPaintImmediately(m_RightRestorItems, m_RightRestorItems.RowIndex, m_RightRestorItems.ColIndex);
                m_RightRestorItems = null;
            }
            OnActSoltItemRestEnd?.Invoke();
        }
    }

    public void SendPaintImmediately(UISoltItem select, int row,int col)
    {
        //发送
        if (m_SlectItems.Contains(select))
            return;
        if (m_SlectItems.Count > 0)
        {
            UISoltItem items = m_SlectItems[m_SlectItems.Count - 1];
            if (items)
            {
                MoveDir dir = GetMoveDir(items.RowIndex, items.ColIndex, row, col);

                OnActPaintMove?.Invoke(dir, items.RectTrans.position, select.RectTrans.position);

                Vector3 pos1 = items.RectTrans.position + (select.RectTrans.position - items.RectTrans.position).normalized * 120;

                OnActPaintImmediately?.Invoke(pos1);

                OnActPaintImmediately?.Invoke(select.RectTrans.position);
            }
        }
        else
        {
            OnActPaintImmediately?.Invoke(select.RectTrans.position + MoveOffset);
        }

        m_SlectItems.Add(select);

        OnActItemSelected?.Invoke(select, row, col);
    }
}