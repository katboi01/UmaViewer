using System;
using System.Collections.Generic;

namespace Cutt
{
    public class LiveTimelineEventPublisher
    {
        private class EventWrapper
        {
            public LiveTimelineEventID id;

            public event LiveTimelineEventHandler handler;

            public EventWrapper(LiveTimelineEventID id_)
            {
                id = id_;
            }

            public void fire(LiveTimelineKeyEventData.EventData data)
            {
                if (this.handler != null)
                {
                    this.handler(data);
                }
            }
        }

        public interface IParamGenerator
        {
            CuttEventParamBase ToObject(string json);

            CuttEventParamBase Create();
        }

        public class ParamGenerator<TypeParam> : IParamGenerator where TypeParam : CuttEventParamBase, new()
        {
            public CuttEventParamBase ToObject(string json)
            {
                return JsonMapper.ToObject<TypeParam>(json);
            }

            public CuttEventParamBase Create()
            {
                return new TypeParam();
            }
        }

        public static Dictionary<LiveTimelineEventID, IParamGenerator> paramGeneratorDict;

        private Dictionary<LiveTimelineEventID, EventWrapper> eventDict = new Dictionary<LiveTimelineEventID, EventWrapper>();

        static LiveTimelineEventPublisher()
        {
            paramGeneratorDict = new Dictionary<LiveTimelineEventID, IParamGenerator>();
            paramGeneratorDict.Add(LiveTimelineEventID.SwitchCamera, new ParamGenerator<CuttEventParam_SwitchCamera>());
            paramGeneratorDict.Add(LiveTimelineEventID.CameraHandShake, new ParamGenerator<CuttEventParam_CameraHandShake>());
            paramGeneratorDict.Add(LiveTimelineEventID.ChangeEyeTrackTarget, new ParamGenerator<CuttEventParam_ChangeEyeTrackTarget>());
            paramGeneratorDict.Add(LiveTimelineEventID.SwitchSheetFromCharacterCondition, new ParamGenerator<CuttEventParam_SwitchSheetFromCharacterCondition>());
            paramGeneratorDict.Add(LiveTimelineEventID.SwitchSheetAltFromCharacterCondition, new ParamGenerator<CuttEventParam_SwitchSheetAltFromCharacterCondition>());
            paramGeneratorDict.Add(LiveTimelineEventID.SwitchSheetFromCharacterConditionVm, new ParamGenerator<CuttEventParam_SwitchSheetFromCharacterConditionVm>());
        }

        public void BeginPublish()
        {
            foreach (object value in Enum.GetValues(typeof(LiveTimelineEventID)))
            {
                eventDict[(LiveTimelineEventID)value] = new EventWrapper((LiveTimelineEventID)value);
            }
        }

        public void EndPublish()
        {
            eventDict.Clear();
        }

        public void Subscribe(LiveTimelineEventID id, LiveTimelineEventHandler handler)
        {
            EventWrapper @event = GetEvent(id);
            if (@event != null)
            {
                @event.handler += handler;
            }
        }

        public void Unsubscribe(LiveTimelineEventID id, LiveTimelineEventHandler handler)
        {
            EventWrapper @event = GetEvent(id);
            if (@event != null)
            {
                @event.handler -= handler;
            }
        }

        public void FireEvent(LiveTimelineKeyEventData eventData)
        {
            foreach (LiveTimelineKeyEventData.EventData @event in eventData.eventList)
            {
                GetEvent(@event.eventId).fire(@event);
            }
        }

        private EventWrapper GetEvent(LiveTimelineEventID id)
        {
            if (eventDict.TryGetValue(id, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
