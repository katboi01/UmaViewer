// EventBroker<EventData>
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEventSender
{
}


public class EventBroker<EventData> where EventData : BaseEventData
{
    public delegate bool EventHandler(IEventSender eventSender, EventData eventData);

    private class EventJob
    {
        private IEventSender _eventSender;

        private EventData _eventData;

        private Action<bool> _endCallback;

        private float _delayTime;

        public IEventSender eventSender => _eventSender;

        public EventData eventData => _eventData;

        public Action<bool> endCallback => _endCallback;

        public void SetParam(IEventSender eventSender, EventData eventData, Action<bool> endCallbac, float delayTime)
        {
            _eventSender = eventSender;
            _eventData = eventData;
            _endCallback = endCallbac;
            _delayTime = delayTime;
        }

        public bool Update()
        {
            _delayTime -= Time.deltaTime;
            if (_delayTime <= 0f)
            {
                return true;
            }
            return false;
        }
    }

    private Dictionary<int, EventHandler> _eventHandlerTable = new Dictionary<int, EventHandler>();

    private Dictionary<EventHandler, int> _eventTypeTable = new Dictionary<EventHandler, int>();

    private Queue<EventJob> _inactiveJobQueue;

    private Queue<EventJob> _readyJobQueue;

    private LinkedList<EventJob> _activeJobList;

    public EventBroker()
    {
    }

    public EventBroker(int jobArrayNum)
    {
        CreateEventJobArray(jobArrayNum);
    }

    private void CreateEventJobArray(int jobArrayNum)
    {
        _inactiveJobQueue = new Queue<EventJob>(jobArrayNum);
        _readyJobQueue = new Queue<EventJob>(jobArrayNum);
        _activeJobList = new LinkedList<EventJob>();
        for (int i = 0; i < jobArrayNum; i++)
        {
            EventJob item = new EventJob();
            _inactiveJobQueue.Enqueue(item);
        }
    }

    public void RegistEventHandler(int eventType, EventHandler eventHandler)
    {
        if (!_eventTypeTable.ContainsKey(eventHandler))
        {
            if (!_eventHandlerTable.ContainsKey(eventType))
            {
                _eventHandlerTable.Add(eventType, null);
            }
            Dictionary<int, EventHandler> eventHandlerTable = _eventHandlerTable;
            eventHandlerTable[eventType] = (EventHandler)Delegate.Combine(eventHandlerTable[eventType], eventHandler);
            _eventTypeTable.Add(eventHandler, eventType);
        }
    }

    public void UnregistEventHandler(int eventType, EventHandler eventHandler)
    {
        if (_eventHandlerTable.ContainsKey(eventType))
        {
            _eventTypeTable.Remove(eventHandler);
            Dictionary<int, EventHandler> eventHandlerTable = _eventHandlerTable;
            eventHandlerTable[eventType] = (EventHandler)Delegate.Remove(eventHandlerTable[eventType], eventHandler);
        }
    }

    public void UnregistEventHandler(EventHandler eventHandler)
    {
        if (_eventTypeTable.ContainsKey(eventHandler))
        {
            int eventType = _eventTypeTable[eventHandler];
            UnregistEventHandler(eventType, eventHandler);
        }
    }

    public bool TriggerEvent(IEventSender eventSender, EventData eventData)
    {
        int eventType = eventData.GetEventType();
        if (!isRegisteredEventReciever(eventType))
        {
            return false;
        }
        return _eventHandlerTable[eventType](eventSender, eventData);
    }

    public void AddEventJob(IEventSender eventSender, EventData eventData, Action<bool> endCallback, float delayTime)
    {
        if (eventData != null && _inactiveJobQueue != null && _inactiveJobQueue.Count > 0)
        {
            EventJob eventJob = _inactiveJobQueue.Dequeue();
            eventJob.SetParam(eventSender, eventData, endCallback, delayTime);
            _readyJobQueue.Enqueue(eventJob);
        }
    }

    public int GetEventJobCount()
    {
        return _readyJobQueue.Count;
    }

    public void UpdateEventJob()
    {
        if (_inactiveJobQueue == null)
        {
            return;
        }
        while (_readyJobQueue.Count > 0)
        {
            EventJob value = _readyJobQueue.Dequeue();
            _activeJobList.AddLast(value);
        }
        LinkedListNode<EventJob> linkedListNode = _activeJobList.First;
        while (linkedListNode != null)
        {
            EventJob value2 = linkedListNode.Value;
            if (!value2.Update())
            {
                linkedListNode = linkedListNode.Next;
                continue;
            }
            bool obj = TriggerEvent(value2.eventSender, value2.eventData);
            if (value2.endCallback != null)
            {
                value2.endCallback(obj);
            }
            _activeJobList.Remove(linkedListNode);
            _inactiveJobQueue.Enqueue(value2);
            linkedListNode = linkedListNode.Next;
        }
    }

    private bool isRegisteredEventReciever(int eventType)
    {
        if (!_eventHandlerTable.ContainsKey(eventType))
        {
            return false;
        }
        if (_eventHandlerTable[eventType] == null)
        {
            return false;
        }
        return true;
    }
}
