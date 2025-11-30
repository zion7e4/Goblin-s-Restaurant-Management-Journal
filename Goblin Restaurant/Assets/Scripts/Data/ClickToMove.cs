using UnityEngine;
using Pathfinding; // A* 플러그인 기능 가져오기

public class ClickToMove : MonoBehaviour
{
    private AIPath aiPath;

    void Start()
    {
        // 내 캐릭터에 붙어있는 AIPath(다리) 컴포넌트를 찾아서 저장
        aiPath = GetComponent<AIPath>();
    }

    void Update()
    {
        // 마우스 왼쪽 버튼(0)을 눌렀을 때
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스가 클릭한 화면 위치를 게임 월드 좌표로 변환
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // AIPath에게 "저 좌표(target)로 이동해!"라고 명령
            // (그러면 AIPath가 알아서 Pathfinder(지도)를 보고 길을 찾아 움직임)
            aiPath.destination = target;
        }
    }
}