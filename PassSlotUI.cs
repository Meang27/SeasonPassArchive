using CustomAttributes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 시즌패스 보상 슬롯 UI
/// </summary>
public class PassSlotUI : MonoBehaviour, IPointerEnterHandler
{
    #region [Public Variables]
    [Header("기본 정보 Root")]
    [SerializeField] GameObject m_InfoRoot;
    [Header("선택 마크")]
    [SerializeField] GameObject m_SelectedMark;
    [Header("레벨")]
    [SerializeField] TextMeshProUGUI m_LevelText;
    [Header("보상 정보")]
    [SerializeField] RewardSlotUI m_RewardSlotUI;
    [Header("무료 마크")]
    [SerializeField] GameObject m_FreeMark;
    [Header("잠김 마크")]
    [SerializeField] GameObject m_LockMark;
    [HorizontalLine(1)]
    [Header("진행중 Root")]
    [SerializeField] GameObject m_ProgressRoot;
    [Header("경험치")]
    [SerializeField] GameObject m_ExpRoot;
    [SerializeField] TextMeshProUGUI m_ExpText;
    [SerializeField] Image m_ExpGauge;
    [Header("보상 받기 버튼")]
    [SerializeField] Button m_RewardButton;
    [HorizontalLine(1)]
    [Header("보상 완료 Root")]
    [SerializeField] GameObject m_RewardedRoot;
    #endregion

    #region [Private Variables]
    /// <summary>
    /// 슬롯의 부모 번들UI 타입
    /// </summary>
    private ePassBundleType m_BundleType;
    /// <summary>슬롯의 부모 패스 데이터</summary>
    private SeasonPassData m_PassData;

    /// <summary>슬롯의 상태</summary>
    private ePassState m_SlotState = ePassState.EMPTY;
    private PassExpRewardInfo m_RewardInfo;
    private int m_SlotIndex = 0;
    private int m_Level;
    private int m_PassID;
    #endregion

    #region [Properties]

    public PassExpRewardInfo RewardInfo { get { return m_RewardInfo; } }
    public ePassState SlotState { get { return m_SlotState; } }
    #endregion

    #region [Public Method]
    public void Initialize(int slotIndex, ePassBundleType bundleType, SeasonPassData passData)
    {
        m_SlotIndex = slotIndex;
        m_PassData = passData;
        m_SlotState = ePassState.EMPTY;
        //버튼 콜백 제거
        m_RewardButton.onClick.RemoveAllListeners();
        //번들 타입이 아카이브가 아닐때만 콜백 추가
        m_BundleType = bundleType;
        if (m_BundleType == ePassBundleType.ARCHIVE_PASS)
            ActiveControlSlotReward(passData);
        else
            m_RewardButton.onClick.AddListener(ClickRewardSlot);
        //슬롯 상태에 따른 갱신
        _UpdateSlotUIByState();    
    }

    /// <summary>
    /// 패스 슬롯 UI 셋팅
    /// </summary>
    public void SettingPassSlotUI(int passID, PassExpRewardInfo rewardInfo, int selectedIndex, int level)
    {
        m_PassID = passID;
        m_RewardInfo = rewardInfo;
        m_SlotState = rewardInfo.State;
        //레벨 셋팅
        m_Level = level;
        m_LevelText.text = m_Level.ToString();
        //슬롯 상태에 따른 갱신
        _UpdateSlotUIByState();
        //선택 상태 갱신
        UpdateSelectedMark(selectedIndex);        
    }

    /// <summary>
    /// 패스 슬롯 UI 보상받기 버튼 interatable 컨트롤
    /// </summary>
    public void ActiveControlSlotReward(SeasonPassData passData)
    {
        if (passData.IsPaid)
        {
            m_RewardButton.interactable = true;
            m_RewardButton.onClick.AddListener(ClickRewardSlot);
        }           
        else
        {
            m_RewardButton.interactable = false;
        }
    }

    /// <summary>
    /// 선택 상태 갱신
    /// </summary>
    public void UpdateSelectedMark(int selectedIndex)
    {
        if (m_SlotState == ePassState.EMPTY) return;
        m_SelectedMark.SetActive(m_SlotIndex == selectedIndex);
    }

    /// <summary>
    /// 마우스 오버에 대한 이벤트
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_SlotState == ePassState.EMPTY) return;
        if (m_BundleType == ePassBundleType.ARCHIVE_PASS)
            Eventboard.Instance.SendEvent(nameof(PassSlotUI), PassArchiveUI.ReceiverName, PassArchiveUI.EventType.UpdateSelectBundleInPassSlot, new object[] { m_SlotIndex });
        else
            Eventboard.Instance.SendEvent(nameof(PassSlotUI), LobbyInPassUI.ReceiverName, LobbyInPassUI.EventType.UpdateSlotIndexByMouseOver, new object[] { m_SlotIndex });
    }
    #endregion

    #region [Private Method]
    /// <summary>
    /// 슬롯 상태에 따른 UI 갱신
    /// </summary>
    private void _UpdateSlotUIByState()
    {
        switch (m_SlotState)
        {
            //슬롯이 비어 있는 상태
            case ePassState.EMPTY:
                {
                    //보상 정보 비활성
                    m_InfoRoot.SetActive(false);
                    //진행 상태 비활성
                    m_ProgressRoot.SetActive(false);
                    //보상 완료 비활성
                    m_RewardedRoot.SetActive(false);
                    //잠김 상태 비활성
                    m_LockMark.SetActive(false);
                }
                break;
            case ePassState.PROGRESS:
            case ePassState.REWARD_ABLE:
            case ePassState.REWARD_COMPLETE:
                {
                    //보상 정보 활성
                    m_InfoRoot.SetActive(true);
                    //보상 정보 셋팅
                    _SettingRewardInfo();
                    //잠김 및 무료 마크 셋팅
                    _ActiveControlByFreeAndLockMark();
                    //보상 버튼 셋팅
                    _UpdateRecvButtonByState();
                    //경험치 셋팅
                    _SettingEXPGauge();
                }
                break;
            case ePassState.NOT_PROGRESS:
                {
                    //진행 상태 비활성
                    m_ProgressRoot.SetActive(false);
                    //보상 완료 비활성
                    m_RewardedRoot.SetActive(false);
                    //보상 정보 활성
                    m_InfoRoot.SetActive(true);
                    //보상 정보 셋팅
                    _SettingRewardInfo();
                    //잠김 및 무료 마크 셋팅
                    _ActiveControlByFreeAndLockMark();
                    //보상 버튼 셋팅
                    _UpdateRecvButtonByState();
                }
                break;
        }
    }

    /// <summary>
    /// 패스 슬롯의 기본 정보를 셋팅합니다.
    /// </summary>
    private void _SettingRewardInfo()
    {
        List<(GoodsID, int)> rewardList = RewardTable.Instance.GetRewardInfoByID(m_RewardInfo.RewardID);
        m_RewardSlotUI.ShowRewardSlotUI(rewardList[0].Item1, rewardList[0].Item2);
    }

    /// <summary>
    /// 무료 마크 및 잠긴 마크 셋팅
    /// </summary>
    private void _ActiveControlByFreeAndLockMark()
    {
        //패스 정보 리턴
        SeasonPassData passData = PassDataManager.Instance.GetSeasonPassDataByID(m_PassID);
        if (passData.IsPaid)
        {
            m_FreeMark.SetActive(false);
            m_LockMark.SetActive(false);
        }
        else
        {
            //라이브
            if (m_BundleType == ePassBundleType.LIVE_PASS)
            {
                m_FreeMark.SetActive(m_RewardInfo.IsFree);
                m_LockMark.SetActive(!m_RewardInfo.IsFree);
            }
            else if (m_BundleType == ePassBundleType.ARCHIVE_PASS)
            {
                //아카이브
                m_FreeMark.SetActive(false);
                m_LockMark.SetActive(true);
            }
        }
    }

    /// <summary>
    /// EXP 셋팅
    /// </summary>
    private void _SettingEXPGauge()
    {
        if(m_RewardInfo.State != ePassState.PROGRESS) return;
        //패스 정보 리턴
        SeasonPassData passData = PassDataManager.Instance.GetSeasonPassDataByID(m_PassID);
        //현재 경험치 리턴
        float curExp = (float)passData.ProgressExpByCurrentLevel;
        //게이지 셋팅
        m_ExpGauge.fillAmount = curExp == 0 ? 0f : (curExp / (float)passData.MaxExpByCurrentLevel);
        //텍스트 셋팅
        m_ExpText.text = string.Format(TextManager.Instance.GetText(TextID.FORM_TWO_VALUE_SLASH), passData.ProgressExpByCurrentLevel, passData.MaxExpByCurrentLevel);
    }

    /// <summary>
    /// 슬롯 상태에 따른 버튼 셋팅
    /// </summary>
    private void _UpdateRecvButtonByState()
    {
        if(m_RewardInfo.State == ePassState.EMPTY || m_RewardInfo.State == ePassState.NOT_PROGRESS)
        {
            //진행 상태 비활성
            m_ProgressRoot.SetActive(false);
            return;
        }
        //진행 상태 활성
        m_ProgressRoot.SetActive(true);
        m_ExpRoot.SetActive(m_RewardInfo.State == ePassState.PROGRESS);
        m_RewardButton.gameObject.SetActive(m_RewardInfo.State == ePassState.REWARD_ABLE);
        m_RewardedRoot.SetActive(m_RewardInfo.State == ePassState.REWARD_COMPLETE);
    }
    #endregion

    #region [Event Method]
    /// <summary>
    /// 보상받기 클릭
    /// </summary>
    public void ClickRewardSlot()
    {
        if(m_SlotState == ePassState.PROGRESS)
        {
            //경험치가 부족합니다
            //CenterMessage.ShowMessage(TextID.NOT_ENOUGH_PASS_EXP);
            return;
        }
        if (m_SlotState != ePassState.REWARD_ABLE) return;
        //라이브 구매X, 유료구간
        if (m_PassData.IsLive && !m_PassData.IsPaid)
        {
            if (!m_RewardInfo.IsFree)
            {
                //구매 하라는 안내 표시
                CenterMessage.ShowMessage(TextID.PURCHASE_PASS_FIRST_PLEASE);
                return;
            }
        }
        //보상 요청 이벤트 전송
        Eventboard.Instance.SendEvent(nameof(PassSlotUI), LobbyInPassUI.ReceiverName, LobbyInPassUI.EventType.SendRewardPass, new object[] { m_Level });
    }
    #endregion
}
