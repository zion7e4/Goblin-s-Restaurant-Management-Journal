// using UnityEngine;
// using UnityEngine.EventSystems;
// using System.Collections.Generic;
// using UnityEngine.InputSystem; // Input System 사용

// public class WhoIsBlocking : MonoBehaviour
// {
//     void Update()
//     {
//         // 1. 현재 마우스 위치 가져오기 (New Input System 방식)
//         if (Mouse.current == null) return;
//         Vector2 mousePos = Mouse.current.position.ReadValue();

//         // 2. 가상의 레이저 쏘기 준비
//         PointerEventData pointerData = new PointerEventData(EventSystem.current);
//         pointerData.position = mousePos;

//         // 3. 레이저 쏘기 (UI 관통)
//         List<RaycastResult> results = new List<RaycastResult>();
//         EventSystem.current.RaycastAll(pointerData, results);

//         // 4. 맞은 녀석이 있으면 이름 출력
//         if (results.Count > 0)
//         {
//             // results[0]이 가장 맨 앞에 있는(마우스를 가로채는) 녀석입니다.
//             Debug.Log("마우스 아래 감지됨: " + results[0].gameObject.name);
//         }
//     }
// }