/// <summary>
/// 직원이 관리창에서 할당받는 역할(Zone)을 정의합니다.
/// </summary>
public enum EmployeeRole
{
    Unassigned, // 미지정 (모든 일을 함)
    Kitchen,    // 주방 (요리)
    Hall,        // 홀 (서빙, 청소)
    AllRounder // 만능 (주방과 홀 모두 가능)
}
