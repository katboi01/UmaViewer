using UnityEngine;

public class ClothParameter
{
    private NativeClothParameter[] _nativeClothParameter = new NativeClothParameter[1]
    {
        new NativeClothParameter
        {
            _stiffnessForceRate = 1f,
            _dragForceRate = 1f,
            _gravityRate = 1f,
            _collisionSwitch = true,
            _capability0 = CySpring.eCapability.None,
            _capability1 = CySpring.eCapability.None,
            _capability2 = CySpring.eCapability.None,
            _capability3 = CySpring.eCapability.None,
            _capability4 = CySpring.eCapability.None,
            _gravityDirection0 = Vector3.down,
            _gravityDirection1 = Vector3.down,
            _gravityDirection2 = Vector3.down,
            _gravityDirection3 = Vector3.down,
            _gravityDirection4 = Vector3.down,
            _forceScale = Vector3.one
        }
    };

    public NativeClothParameter[] nativeClothParameter => _nativeClothParameter;

    public float stiffnessForceRate
    {
        get
        {
            return _nativeClothParameter[0]._stiffnessForceRate;
        }
        set
        {
            _nativeClothParameter[0]._stiffnessForceRate = value;
        }
    }

    public float dragForceRate
    {
        get
        {
            return _nativeClothParameter[0]._dragForceRate;
        }
        set
        {
            _nativeClothParameter[0]._dragForceRate = value;
        }
    }

    public float gravityRate
    {
        get
        {
            return _nativeClothParameter[0]._gravityRate;
        }
        set
        {
            _nativeClothParameter[0]._gravityRate = value;
        }
    }

    public bool collisionSwitch
    {
        get
        {
            return _nativeClothParameter[0]._collisionSwitch;
        }
        set
        {
            _nativeClothParameter[0]._collisionSwitch = value;
        }
    }

    public CySpring.eCapability capability0
    {
        get
        {
            return _nativeClothParameter[0]._capability0;
        }
        set
        {
            _nativeClothParameter[0]._capability0 = value;
        }
    }

    public CySpring.eCapability capability1
    {
        get
        {
            return _nativeClothParameter[0]._capability1;
        }
        set
        {
            _nativeClothParameter[0]._capability1 = value;
        }
    }

    public CySpring.eCapability capability2
    {
        get
        {
            return _nativeClothParameter[0]._capability2;
        }
        set
        {
            _nativeClothParameter[0]._capability2 = value;
        }
    }

    public CySpring.eCapability capability3
    {
        get
        {
            return _nativeClothParameter[0]._capability3;
        }
        set
        {
            _nativeClothParameter[0]._capability3 = value;
        }
    }

    public CySpring.eCapability capability4
    {
        get
        {
            return _nativeClothParameter[0]._capability4;
        }
        set
        {
            _nativeClothParameter[0]._capability4 = value;
        }
    }

    public Vector3 gravityDirection0
    {
        get
        {
            return _nativeClothParameter[0]._gravityDirection0;
        }
        set
        {
            _nativeClothParameter[0]._gravityDirection0 = value;
        }
    }

    public Vector3 gravityDirection1
    {
        get
        {
            return _nativeClothParameter[0]._gravityDirection1;
        }
        set
        {
            _nativeClothParameter[0]._gravityDirection1 = value;
        }
    }

    public Vector3 gravityDirection2
    {
        get
        {
            return _nativeClothParameter[0]._gravityDirection2;
        }
        set
        {
            _nativeClothParameter[0]._gravityDirection2 = value;
        }
    }

    public Vector3 gravityDirection3
    {
        get
        {
            return _nativeClothParameter[0]._gravityDirection3;
        }
        set
        {
            _nativeClothParameter[0]._gravityDirection3 = value;
        }
    }

    public Vector3 gravityDirection4
    {
        get
        {
            return _nativeClothParameter[0]._gravityDirection4;
        }
        set
        {
            _nativeClothParameter[0]._gravityDirection4 = value;
        }
    }

    public Vector3 forceScale
    {
        get
        {
            return _nativeClothParameter[0]._forceScale;
        }
        set
        {
            _nativeClothParameter[0]._forceScale = value;
        }
    }

    public void ResetCapability()
    {
        capability0 = CySpring.eCapability.None;
        capability1 = CySpring.eCapability.None;
        capability2 = CySpring.eCapability.None;
        capability3 = CySpring.eCapability.None;
        capability4 = CySpring.eCapability.None;
    }

    public void ExcludeCapability(CySpring.eCapability caps)
    {
        capability0 &= ~caps;
        capability1 &= ~caps;
        capability2 &= ~caps;
        capability3 &= ~caps;
        capability4 &= ~caps;
    }
}
