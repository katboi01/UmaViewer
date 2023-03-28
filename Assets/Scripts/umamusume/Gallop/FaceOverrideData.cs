using System;
using UnityEngine;

namespace Gallop
{
	[Serializable]
	public class FaceOverrideData : ScriptableObject 
	{
		public bool Enable;
		private readonly string[] OverrideTypeText;
		public bool IsBothEyesSetting;
		public bool IsDisableEyeBlink;
		public FaceOverrideReplaceDataSet[] FaceOverrideArray;

		public void SetEnable(bool val)
        {
			Enable = val;
		}

		public enum OverrideType
		{
			Invalid = -1,
			RightEye = 0,
			LeftEye = 1,
			Max = 2
		}
	}
}

