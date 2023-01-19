using System;
using System.Collections.Generic;
using UnityEngine;

public class PropsExtendController_NodeTransByBust : PropsExtendController
{
    [Serializable]
    public class NodeTransInfo
    {
        [SerializeField]
        [Tooltip("元の値を保持する場合は\nOFFにしてください。")]
        private bool _isValid = true;

        [SerializeField]
        private Vector3 _translate = Vector3.zero;

        [SerializeField]
        private Vector3 _rotate = Vector3.zero;

        public bool isValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
            }
        }

        public Vector3 translate
        {
            get
            {
                return _translate;
            }
            set
            {
                _translate = value;
            }
        }

        public Vector3 rotate
        {
            get
            {
                return _rotate;
            }
            set
            {
                _rotate = value;
            }
        }

        public NodeTransInfo(Vector3 translate, Vector3 rotate)
        {
            _isValid = true;
            _translate = translate;
            _rotate = rotate;
        }
    }

    private const int DEFAULT_BUST_ID = 1;

    private static readonly Dictionary<int, int> SPECIAL_BUST_ID_CONVERT_TABLE = new Dictionary<int, int>
    {
        { 5, 2 },
        { 6, 1 }
    };

    [SerializeField]
    private Transform _node;

    [Header("※ 胸差分の値を 小, 中, 大, 特小, 特大 の順で設定してください")]
    [SerializeField]
    private NodeTransInfo[] _nodeTransInfoList = new NodeTransInfo[5]
    {
        new NodeTransInfo(Vector3.zero, Vector3.zero),
        new NodeTransInfo(Vector3.zero, Vector3.zero),
        new NodeTransInfo(Vector3.zero, Vector3.zero),
        new NodeTransInfo(Vector3.zero, Vector3.zero),
        new NodeTransInfo(Vector3.zero, Vector3.zero)
    };

    private int _bustId = 1;

    private bool _isValid = true;

    private Vector3 _translate = Vector3.zero;

    private Vector3 _rotate = Vector3.zero;

    public override void Initialize(int charaIndex)
    {
        int charaId = Director.instance.GetCharacterData(charaIndex + 1).charaId;
        MasterCharaData.CharaData charaData = null;
        charaData = MasterDBManager.instance.masterCharaData.Get(charaId);
        _bustId = charaData.modelBustId;
        if (SPECIAL_BUST_ID_CONVERT_TABLE.ContainsKey(_bustId))
        {
            _bustId = SPECIAL_BUST_ID_CONVERT_TABLE[_bustId];
        }
        if (_bustId < 0 || _nodeTransInfoList.Length <= _bustId)
        {
            _bustId = 1;
        }
        NodeTransInfo nodeTransInfo = _nodeTransInfoList[_bustId];
        _isValid = nodeTransInfo.isValid;
        _translate = nodeTransInfo.translate;
        _rotate = nodeTransInfo.rotate;
    }

    private void LateUpdate()
    {
        if (_node != null && _isValid)
        {
            _node.localPosition = _translate;
            _node.localEulerAngles = _rotate;
        }
    }
}
