// 파일 이름: IconVisibilityTester.cs
using UnityEngine;

public class IconVisibilityTester : MonoBehaviour
{
    // 테스트할 아이콘 프리팹
    public GameObject orderIconPrefab;

    // 아이콘이 생성될 월드 캔버스
    public Canvas worldCanvas;

    void Awake()
    {
        Debug.Log("'T' 키 입력! 아이콘 강제 생성 테스트를 시작합니다.");

        if (orderIconPrefab == null || worldCanvas == null)
        {
            Debug.LogError("테스트 스크립트에 프리팹 또는 캔버스가 연결되지 않았습니다!");
            return;
        }

        // 캔버스의 정중앙 위치에 아이콘을 강제로 생성
        GameObject iconInstance = Instantiate(orderIconPrefab, worldCanvas.transform.position, Quaternion.identity);

        // 캔버스의 자식으로 만들어 렌더링되도록 함
        iconInstance.transform.SetParent(worldCanvas.transform, false);

        Debug.Log($"아이콘 생성 완료! 생성 위치: {iconInstance.transform.position}");
    }
}