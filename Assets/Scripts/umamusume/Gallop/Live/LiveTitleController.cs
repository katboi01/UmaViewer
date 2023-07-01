using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class LiveTitleController : MonoBehaviour
    {
        private const int MAX_AUTHOR_TEXT_FONT_SIZE = 32;
        //[SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        //private TextCommon _titleText; // 0x18
        //[SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        //private TextCommon _authorText; // 0x20
        //[SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        //private ImageCommon _titleImage; // 0x28
        //[SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        //private UIGradation _titleGradation; // 0x30
        //[SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        //private FadeGui _fadeGui; // 0x38
        private bool _isInitialized; // 0x40
        private bool _isDisappear; // 0x41
        private bool _isScaleSettingsVertical; // 0x42
        private const float HORIZONTAL_SCALE = 1;
        private const float VERTICAL_SCALE = 0.8f;
        private Transform _transform; // 0x48
        private Transform _landscapeTransform; // 0x50
        private Transform _portraitTransform; // 0x58
    }
}
