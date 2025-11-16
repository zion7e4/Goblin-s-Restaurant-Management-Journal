using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
https://bonnate.tistory.com/

Insert the script into the game object
insert the TMP font in the inspector
and press the button to find and replace all components.

It may work abnormally, so make sure to back up your scene before using it!!
*/

public class TMP_FontChanger : MonoBehaviour
{
    [SerializeField] public TMP_FontAsset FontAsset;
}

#if UNITY_EDITOR
[CustomEditor(typeof(TMP_FontChanger))]
public class TMP_FontChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Change Font!"))
        {
            TMP_FontAsset fontAsset = ((TMP_FontChanger)target).FontAsset;

            // [수정됨]
            // 사용 중인 유니티 버전(2020.1 추정)에서는 FindObjectsByType이
            // 비활성 객체를 찾는 기능을 지원하지 않습니다.
            // 경고(warning)가 발생하더라도 예전 방식인 FindObjectsOfType(true)를 사용해야 합니다.
            // 또한 'GameObject.' 없이 호출합니다.
            foreach (TextMeshPro textMeshPro3D in FindObjectsOfType<TextMeshPro>(true))
            {
                textMeshPro3D.font = fontAsset;
            }

            // [수정됨] 동일하게 변경
            foreach (TextMeshProUGUI textMeshProUi in FindObjectsOfType<TextMeshProUGUI>(true))
            {
                textMeshProUi.font = fontAsset;
            }
        }
    }
}
#endif