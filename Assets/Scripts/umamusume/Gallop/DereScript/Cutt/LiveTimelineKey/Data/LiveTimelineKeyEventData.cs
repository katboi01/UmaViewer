using System;
using System.Collections.Generic;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyEventData : LiveTimelineKey
    {
        [Serializable]
        public class EventData
        {
            public LiveTimelineEventID eventId;

            private CuttEventParamBase eventParam;

            public string serializedParamter = "";

            public TypeValue GetParameter<TypeValue>() where TypeValue : CuttEventParamBase
            {
                return eventParam as TypeValue;
            }

            public void OnLoad()
            {
                if (!string.IsNullOrEmpty(serializedParamter) && LiveTimelineEventPublisher.paramGeneratorDict.TryGetValue(eventId, out var value))
                {
                    eventParam = null;
                    if (value != null)
                    {
                        eventParam = value.ToObject(serializedParamter);
                    }
                }
            }
        }

        public List<EventData> eventList = new List<EventData>();

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Event;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
            foreach (EventData @event in eventList)
            {
                @event.OnLoad();
            }
        }
    }
}

