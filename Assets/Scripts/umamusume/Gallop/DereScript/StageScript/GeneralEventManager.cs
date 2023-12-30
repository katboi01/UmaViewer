// Stage.GeneralEventManager
using System;

public class GeneralEventManager : SingletonMonoBehaviour<GeneralEventManager>
{
    private const int EVENT_JOB_NUM = 32;

    private EventBroker<GeneralEventDefine.EventData> _eventBroker = new EventBroker<GeneralEventDefine.EventData>(32);

    private void Awake()
    {
        if (this != SingletonMonoBehaviour<GeneralEventManager>.instance)
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private void Update()
    {
        _eventBroker.UpdateEventJob();
    }

    public void RegistEventHandler(BaseEventType eventType, EventBroker<GeneralEventDefine.EventData>.EventHandler eventHandler)
    {
        eventType.Init();
        _eventBroker.RegistEventHandler(eventType.GetEventType(), eventHandler);
    }

    public void UnregistEventHandler(BaseEventType eventType, EventBroker<GeneralEventDefine.EventData>.EventHandler eventHandler)
    {
        _eventBroker.UnregistEventHandler(eventType.GetEventType(), eventHandler);
    }

    public void UnregistEventHandler(EventBroker<GeneralEventDefine.EventData>.EventHandler eventHandler)
    {
        _eventBroker.UnregistEventHandler(eventHandler);
    }

    public bool TriggerEvent(GeneralEventDefine.EventData eventData, IEventSender eventSender = null)
    {
        return _eventBroker.TriggerEvent(eventSender, eventData);
    }

    public void AddEventJob(GeneralEventDefine.EventData eventData, IEventSender eventSender = null, Action<bool> endCallback = null, float delayTime = 0f)
    {
        _eventBroker.AddEventJob(eventSender, eventData, endCallback, delayTime);
    }

    public int GetEventJobCount()
    {
        return _eventBroker.GetEventJobCount();
    }
}
