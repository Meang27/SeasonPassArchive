using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 패스 아카이브 번들 UI
/// </summary>
public class PassBundleUIArchive : PassBundleUIBase
{
    #region [Public Variables]
    [CustomAttributes.HorizontalLine(1)]
    [Header("아카이브 번들 전용 장착 상태 마크")]
    [SerializeField] GameObject m_EquipedMark;
    #endregion

    #region [Private Variables]
    private int m_BundleSlotIndex = 0;
    private Button m_SelectEquipBtn;
    #endregion

    #region [Public Method]
    /// <summary>
    /// 아카이브 번들 UI 셋팅
    /// </summary>
    public void SettingPassBundleUI(SeasonPassData seasonPassData, int bundleSlotIndex, int selectedBundleIndex)
    {
        //시즌 패스 데이터 저장 후 초기화
        m_SeasonPassData = seasonPassData;
        //초기화
        base.Initialize();
        //활성화
        ActiveControl(true);
        //슬롯 인덱스 저장
        m_BundleSlotIndex = bundleSlotIndex;
        //금액 셋팅
        SettingPassPurchaseState();
        //SettingPassPriceUI();
        //보상을 받기 위한 남은 경험치 셋팅
        SettingRemainExp();
        //UI 갱신
        base.UpdateUI();
        //장착 마크 갱신
        ActiveControlByEquipedMark();
        ActiveControlCallback(true);
    }

    /// <summary>
    /// 아카이브 번들 갱신
    /// </summary>
    public void UpdateArchiveBundleUI()
    {
        base.UpdateUI();
        ActiveControlCallback(true);
    }

    /// <summary>
    /// 번들 선택상태 갱신
    /// </summary>
    public void UpdateSelectMark(int selectedBundleIndex)
    {
        base.UpdateSelectedMark(m_BundleSlotIndex == selectedBundleIndex);
    }

    /// <summary>
    /// 번들을 클릭해서 시즌패스를 장착하는 인풋 활성화 여부
    /// </summary>
    public void ActiveControlCallback(bool isActive)
    {
        if (m_SelectEquipBtn == null)
        {
            m_SelectEquipBtn = m_SelectedMark.GetComponent<Button>();
        }
        if (isActive)
        {
            m_SelectEquipBtn.onClick.RemoveAllListeners();
            m_SelectEquipBtn.onClick.AddListener(_OnClickEquip);
        }
        else
        {
            m_SelectEquipBtn.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// 장착 마크 갱신
    /// </summary>
    public void ActiveControlByEquipedMark()
    {
        if (m_SeasonPassData == null)
        {
            m_EquipedMark.SetActive(false);
            return;
        }
        m_EquipedMark.SetActive(m_BundleUIState == ePassBundleUIState.EQUIPED);
    }
    #endregion

    #region [Private Method]

    #endregion

    #region [Event Method]
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Eventboard.Instance.SendEvent(nameof(PassBundleUIArchive), PassArchiveUI.ReceiverName, PassArchiveUI.EventType.UpdateSelectMark, new object[] { m_BundleSlotIndex });
    }

    private void _OnClickEquip()
    {
        Eventboard.Instance.SendEvent(nameof(PassBundleUIArchive), PassArchiveUI.ReceiverName, PassArchiveUI.EventType.ActivatePass);
    }
#endregion
}
