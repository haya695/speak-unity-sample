using UnityEngine;
using UnityEngine.XR.MagicLeap;

[RequireComponent(typeof(MLPrivilegeRequesterBehavior))]
public class RequestMicPrivilege : MonoBehaviour
{
	MLPrivilegeRequesterBehavior m_privilegeRequester;

	void Awake()
	{
		m_privilegeRequester = GetComponent<MLPrivilegeRequesterBehavior>();
		m_privilegeRequester.Privileges = new[]
		{
			MLPrivileges.RuntimeRequestId.AudioCaptureMic,
		};
		m_privilegeRequester.OnPrivilegesDone += HandlePrivilegesDone;
	}

	void HandlePrivilegesDone(MLResult result)
	{
		if (!result.IsOk)
		{
			Debug.Log("Application End Because Mic Privileges Denied");
			Application.Quit();
		}
	}

}
