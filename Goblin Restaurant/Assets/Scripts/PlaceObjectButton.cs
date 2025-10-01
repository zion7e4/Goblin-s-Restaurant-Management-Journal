using UnityEngine;

public class PlaceObjectButton : MonoBehaviour
{
    public int tablePrice = 100; // 테이블 가격\
    public GameObject UpgradeTableButton; // 테이블 업그레이드 버튼
    public GameObject UpgradeTablePannal; // 테이블 업그레이드 패널

    public void OnButtonClick()
    {
        if(GameManager.instance.totalGoldAmount >= tablePrice)
        {
            GameManager.instance.AddTable(this.transform, tablePrice);

            gameObject.SetActive(false); // 버튼 비활성화
            UpgradeTableButton.SetActive(false); // 테이블 업그레이드 버튼 비활성화
            UpgradeTablePannal.SetActive(false); // 테이블 업그레이드 패널 비활성화


        }
        else
        {
            Debug.Log("골드가 부족합니다!");
        }
    }
}
